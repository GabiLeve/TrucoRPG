using TrucoRPG.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// CORS: permite que el front Angular (localhost:4200) pueda hablarle al backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()); // Necesario para SignalR cross-origin
});

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

app.UseCors("FrontPolicy");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gamehub"); // Endpoint WebSocket del juego

app.Run();
