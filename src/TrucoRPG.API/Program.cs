using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TrucoRPG.API.Hubs;
using TrucoRPG.Infraestructura.Data;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Infraestructura.Repositorios;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Infraestructura.Provider;
using TrucoRPG.Dominio.UseCases;

var builder = WebApplication.CreateBuilder(args);

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
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("FrontPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/gamehub");

app.Run();