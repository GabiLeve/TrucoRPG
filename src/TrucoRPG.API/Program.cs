using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
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

// ── Logging estructurado (Serilog) ───────────────────────────────
// Reemplaza el logger por defecto. Escribe a consola (con plantilla legible)
// y a un archivo rotativo diario en logs/trucorpg-YYYYMMDD.log.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)   // permite ajustar niveles desde appsettings
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/trucorpg-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

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
builder.Services.AddScoped<IRegisterUseCase, RegisterUseCase>();
builder.Services.AddScoped<ILoginUseCase, LoginUseCase>();
builder.Services.AddScoped<ICambiarPasswordUseCase, CambiarPasswordUseCase>();
builder.Services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddScoped<ISolicitarResetPasswordUseCase>(sp =>
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
builder.Services.AddScoped<VerificarPersonajeUseCase>();
builder.Services.AddScoped<CrearPersonajeUseCase>();
builder.Services.AddScoped<ObtenerPersonajeDelUsuarioUseCase>();
builder.Services.AddScoped<IInventarioRepositorio, InventarioRepositorio>();
builder.Services.AddScoped<ComprarItemUseCase>();
builder.Services.AddScoped<ObtenerInventarioDelUsuarioUseCase>();
builder.Services.AddScoped<ObtenerMonedasUseCase>();

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
builder.Services.AddScoped<ConfirmarDestelloUseCase>();
builder.Services.AddScoped<ConfirmarEspejismoUseCase>();
builder.Services.AddScoped<ConfirmarMandingaEspejoUseCase>();
builder.Services.AddScoped<ConfirmarMandingaEnganoUseCase>();
builder.Services.AddScoped<ConfirmarMandingaMaldicionUseCase>();
builder.Services.AddScoped<AvanzarMaquinaHistoriaUseCase>();
builder.Services.AddScoped<GanarAutomaticoDebugUseCase>();
builder.Services.AddScoped<SumarPuntosHumanoDebugUseCase>();

// ── Servicios de sala (singleton: el estado de salas debe sobrevivir entre requests) ──
builder.Services.AddSingleton<SalaService>();

// ── API ───────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
        opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);

// Genera respuestas ProblemDetails (RFC 7807) también para los errores
// automáticos del framework (validación de modelo, 401/403, etc.).
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TrucoRPG API",
        Version = "v1",
        Description = "API del Truco RPG. Los errores se devuelven en formato ProblemDetails (RFC 7807)."
    });

    // Botón "Authorize" en Swagger UI para mandar el JWT (Bearer).
    var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Pegá el token JWT (sin el prefijo 'Bearer ').",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    opt.AddSecurityDefinition("Bearer", jwtScheme);
    opt.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });

    // Incluye los comentarios XML (/// <summary>) en la documentación de Swagger.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        opt.IncludeXmlComments(xmlPath);
});
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

// Log estructurado de cada request HTTP (método, ruta, status, duración).
app.UseSerilogRequestLogging();

app.UseRouting();
app.UseCors("FrontPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();
