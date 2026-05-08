using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: permite que el front (Phaser) pueda hablarle al backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontPolicy", policy =>
        policy.WithOrigins(
                "http://localhost:5500",   // VS Code Live Server
                "http://127.0.0.1:5500",  // VS Code Live Server (alternativo)
                "http://localhost:3000",   // npx serve
                "http://localhost:8080"    // python http.server
              )
              .AllowAnyMethod()
              .AllowAnyHeader());
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

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Imagenes")),
    RequestPath = "/Imagenes"
});

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
