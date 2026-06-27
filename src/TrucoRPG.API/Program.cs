using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrucoRPG.API.Hubs;
using TrucoRPG.API.Middlewares;
using TrucoRPG.API.Services;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Infraestructura.Data;
using TrucoRPG.Infraestructura.Provider;
using TrucoRPG.Infraestructura.Repositorios;
using TrucoRPG.Infraestructura.Servicios;
using TrucoRPG.Logica.UseCases;

var builder = WebApplication.CreateBuilder(args);

// Escuchar en todas las interfaces de red (permite acceso desde la red local)
builder.WebHost.UseUrls("http://0.0.0.0:5001");

builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

// ── Base de datos (MySQL via Pomelo) ──────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' no encontrada.");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ── Identity ──────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit           = true;
    opt.Password.RequiredLength         = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase       = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT ───────────────────────────────────────────────────────────
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };

    // SignalR necesita leer el token desde el query string (?access_token=...)
    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Query["access_token"];
            var path  = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/gamehub"))
                context.Token = token;
            return Task.CompletedTask;
        }
    };
});

// ── Inyección de dependencias (Infrastructure → Domain) ───────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUsuarioActualServicio, UsuarioActualServicio>();
builder.Services.AddScoped<IRivalRepositorio, RivalRepositorio>();
builder.Services.AddScoped<IProgresoPartidaRepositorio, ProgresoPartidaRepositorio>();
builder.Services.AddScoped<HistoriaValidacionServicio>();
builder.Services.AddScoped<ObtenerRivalesHistoriaUseCase>();
builder.Services.AddScoped<ObtenerProgresoHistoriaUseCase>();
builder.Services.AddScoped<PuedePelearConRivalUseCase>();
builder.Services.AddScoped<RegistrarVictoriaHistoriaUseCase>();
builder.Services.AddScoped<RegisterUseCase>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<CambiarPasswordUseCase>();
builder.Services.AddScoped<ResetPasswordUseCase>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<SolicitarResetPasswordUseCase>(sp =>
{
    var repo       = sp.GetRequiredService<IUsuarioRepositorio>();
    var email      = sp.GetRequiredService<IEmailService>();
    var config     = sp.GetRequiredService<IConfiguration>();
    var frontendUrl = config["Email:FrontendUrl"] ?? "http://localhost:4200";
    return new SolicitarResetPasswordUseCase(repo, email, frontendUrl);
});
builder.Services.AddScoped<ReglasUseCase>();
builder.Services.AddScoped<IItemTiendaRepositorio, ItemTiendaRepositorio>();
builder.Services.AddScoped<ObtenerTiendaUseCase>();

// ── Use Cases de Truco (vs. Máquina) ─────────────────────────────
builder.Services.AddScoped<NuevaManoUseCase>();
builder.Services.AddScoped<ConfigurarNivelMentiraUseCase>();
builder.Services.AddScoped<CantarEnvidoUseCase>();
builder.Services.AddScoped<ResponderEnvidoUseCase>();
builder.Services.AddScoped<CantarTrucoUseCase>();
builder.Services.AddScoped<ResponderTrucoUseCase>();
builder.Services.AddScoped<EscalarTrucoUseCase>();
builder.Services.AddScoped<IrseAlMazoUseCase>();
builder.Services.AddScoped<JugarCartaUseCase>();
builder.Services.AddScoped<ActivarHabilidadUseCase>();
builder.Services.AddScoped<ConfirmarSalpicaduraUseCase>();
builder.Services.AddScoped<ConfirmarTravesuraUseCase>();
builder.Services.AddScoped<ConfirmarRasgunoUseCase>();
builder.Services.AddScoped<ConfirmarAullidoUseCase>();
builder.Services.AddScoped<AvanzarMaquinaHistoriaUseCase>();
builder.Services.AddScoped<GanarAutomaticoDebugUseCase>();

// ── Servicios de sala (singleton: el estado de salas debe sobrevivir entre requests) ──
builder.Services.AddSingleton<SalaService>();

// ── API ───────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// ── CORS ──────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("FrontPolicy", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://192.168.1.45:4200"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            var pendientes = await db.Database.GetPendingMigrationsAsync();
            var listaPendientes = pendientes.ToList();
            if (listaPendientes.Count > 0)
            {
                logger.LogInformation(
                    "Aplicando {Count} migración(es) pendiente(s): {Migrations}",
                    listaPendientes.Count,
                    string.Join(", ", listaPendientes));
            }

            await db.Database.MigrateAsync();
            logger.LogInformation("Base de datos actualizada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(
                ex,
                "Error al aplicar migraciones. Verificá que MySQL esté encendido y la connection string en appsettings.Development.json o appsettings.Local.json.");
            throw;
        }
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// ── Inicialización de Roles ──────────────────────────────────────────────────────
await InicializadorDatosIdentity.InicializarRolesAsync(app.Services);

app.UseRouting();
app.UseCors("FrontPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();
