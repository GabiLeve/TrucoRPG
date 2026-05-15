using Microsoft.Extensions.FileProviders;
using TrucoRPG.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// CORS: permite que el front (Phaser) pueda hablarle al backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
        policy.WithOrigins(
                "http://localhost:5500",   // VS Code Live Server
                "http://127.0.0.1:5500",  // VS Code Live Server (alternativo)
                "http://localhost:3000",   // npx serve
                "http://localhost:8080",   // python http.server
                "http://localhost:4200"    // Angular dev server
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // Necesario para SignalR cross-origin
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontPolicy");

// Solo redirigir a HTTPS en producción; en desarrollo el front llama directo por HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// UseDefaultFiles hace que "/" sirva "index.html" del wwwroot (el juego Phaser)
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub"); // Endpoint WebSocket del juego

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
