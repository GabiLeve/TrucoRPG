using System.Text.Json;

namespace TrucoRPG.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
               // Console.WriteLine("Middleware ejecutado");
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = ex.Message,
                        tipo = ex.GetType().Name
                    }));
            }
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = ex.Message,
                        tipo = ex.GetType().Name
                    }));
            }
            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = 404;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = ex.Message,
                        tipo = ex.GetType().Name
                    }));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = ex.Message,
                        tipo = ex.GetType().Name
                    }));
            }
        }
    }
}
