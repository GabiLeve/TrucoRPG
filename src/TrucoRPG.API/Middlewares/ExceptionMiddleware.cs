using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TrucoRPG.API.Middlewares
{
    /// <summary>
    /// Middleware global de manejo de errores. Captura las excepciones que se
    /// escapan de los controllers, las traduce a un <see cref="ProblemDetails"/>
    /// (formato estándar RFC 7807) con el status code correspondiente y las
    /// registra con el logger estructurado.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                await EscribirProblemaAsync(context, StatusCodes.Status401Unauthorized,
                    "No autorizado", ex);
            }
            catch (KeyNotFoundException ex)
            {
                await EscribirProblemaAsync(context, StatusCodes.Status404NotFound,
                    "Recurso no encontrado", ex);
            }
            catch (InvalidOperationException ex)
            {
                await EscribirProblemaAsync(context, StatusCodes.Status400BadRequest,
                    "Solicitud inválida", ex);
            }
            catch (ArgumentException ex)
            {
                await EscribirProblemaAsync(context, StatusCodes.Status400BadRequest,
                    "Solicitud inválida", ex);
            }
            catch (Exception ex)
            {
                await EscribirProblemaAsync(context, StatusCodes.Status500InternalServerError,
                    "Error interno del servidor", ex);
            }
        }

        private async Task EscribirProblemaAsync(
            HttpContext context, int statusCode, string titulo, Exception ex)
        {
            // 4xx => warning (error esperable del cliente); 5xx => error real del servidor.
            if (statusCode >= 500)
                _logger.LogError(ex, "Excepción no controlada en {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning(ex, "Solicitud rechazada ({StatusCode}) en {Method} {Path}: {Mensaje}",
                    statusCode, context.Request.Method, context.Request.Path, ex.Message);

            var problema = new ProblemDetails
            {
                Status = statusCode,
                Title = titulo,
                // En 500 no exponemos el detalle interno salvo en desarrollo.
                Detail = statusCode >= 500 && !_env.IsDevelopment()
                    ? "Ocurrió un error inesperado. Intentá de nuevo más tarde."
                    : ex.Message,
                Instance = context.Request.Path
            };
            problema.Extensions["tipo"] = ex.GetType().Name;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problema));
        }
    }
}
