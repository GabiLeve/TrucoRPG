using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrucoRPG.API.Hubs;
using TrucoRPG.API.Middlewares;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Infraestructura.Data;
using TrucoRPG.Infraestructura.Provider;
using TrucoRPG.Infraestructura.Repositorios;
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
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<RegisterUseCase>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<ReglasUseCase>();

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

// ── API ───────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// ── CORS ──────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("FrontPolicy", policy =>
        policy.WithOrigins("https://trucoymana.vercel.app")
                  .AllowAnyHeader()
                  .AllowAnyMethod()));

var app = builder.Build();

var app = builder.Build();

// 1. LO PRIMERO: El middleware de excepciones debe envolver TODO lo demás
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// 2. ENRUTAMIENTO Y CORS (Tienen que ir pegados y antes de autenticar)
app.UseRouting();

// Esto interceptará los comandos OPTIONS del navegador antes de que lleguen a Identity
app.UseCors("FrontPolicy");

// 3. SEGURIDAD
app.UseAuthentication();
app.UseAuthorization();

// 4. ENDPOINTS
app.MapControllers();
app.MapHub<GameHub>("/gamehub");

// 5. INICIALIZACIÓN DE DATOS (Fuera del pipeline de middlewares de peticiones)
using (var scope = app.Services.CreateScope())
{
    // Es mucho más seguro inicializar roles usando un Scope limpio
    await InicializadorDatosIdentity.InicializarRolesAsync(scope.ServiceProvider);
}

// 6. ENCHUFAR EL SERVIDOR
app.Run();
