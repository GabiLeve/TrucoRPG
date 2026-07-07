using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using TrucoRPG.API.Middlewares;
using Xunit;

namespace TrucoRPG.Tests.API
{
    public class ExceptionMiddlewareTest
    {
        private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock = new();
        private readonly Mock<IHostEnvironment> _envMock = new();

        public ExceptionMiddlewareTest()
        {
            // Por defecto, entorno productivo (no Development)
            _envMock.SetupGet(e => e.EnvironmentName).Returns("Production");
        }

        private async Task<(HttpContext Contexto, string Cuerpo)> Ejecutar(RequestDelegate next)
        {
            var middleware = new ExceptionMiddleware(next, _loggerMock.Object, _envMock.Object);

            var contexto = new DefaultHttpContext();
            contexto.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(contexto);

            contexto.Response.Body.Seek(0, SeekOrigin.Begin);
            var cuerpo = await new StreamReader(contexto.Response.Body).ReadToEndAsync();
            return (contexto, cuerpo);
        }

        [Fact]
        public async Task Invoke_SinExcepcion_NoTocaLaRespuesta()
        {
            var (contexto, cuerpo) = await Ejecutar(_ => Task.CompletedTask);

            Assert.Equal(StatusCodes.Status200OK, contexto.Response.StatusCode);
            Assert.Empty(cuerpo);
        }

        [Theory]
        [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized)]
        [InlineData(typeof(KeyNotFoundException), StatusCodes.Status404NotFound)]
        [InlineData(typeof(InvalidOperationException), StatusCodes.Status400BadRequest)]
        [InlineData(typeof(ArgumentException), StatusCodes.Status400BadRequest)]
        public async Task Invoke_ConExcepcionConocida_DevuelveProblemDetailsConElStatusCorrecto(
            Type tipoExcepcion, int statusEsperado)
        {
            var excepcion = (Exception)Activator.CreateInstance(tipoExcepcion, "mensaje de prueba")!;

            var (contexto, cuerpo) = await Ejecutar(_ => throw excepcion);

            Assert.Equal(statusEsperado, contexto.Response.StatusCode);
            Assert.Equal("application/problem+json", contexto.Response.ContentType);

            using var json = JsonDocument.Parse(cuerpo);
            Assert.Equal(statusEsperado, json.RootElement.GetProperty("status").GetInt32());
            // En 4xx el detalle expone el mensaje original
            Assert.Equal("mensaje de prueba", json.RootElement.GetProperty("detail").GetString());
            Assert.Equal(tipoExcepcion.Name, json.RootElement.GetProperty("tipo").GetString());
        }

        [Fact]
        public async Task Invoke_ConExcepcionGenericaEnProduccion_Devuelve500SinDetalleInterno()
        {
            var (contexto, cuerpo) = await Ejecutar(_ => throw new Exception("secreto interno"));

            Assert.Equal(StatusCodes.Status500InternalServerError, contexto.Response.StatusCode);

            using var json = JsonDocument.Parse(cuerpo);
            var detalle = json.RootElement.GetProperty("detail").GetString();
            Assert.DoesNotContain("secreto interno", detalle);
        }

        [Fact]
        public async Task Invoke_ConExcepcionGenericaEnDesarrollo_ExponeElDetalle()
        {
            _envMock.SetupGet(e => e.EnvironmentName).Returns("Development");

            var (contexto, cuerpo) = await Ejecutar(_ => throw new Exception("detalle visible"));

            Assert.Equal(StatusCodes.Status500InternalServerError, contexto.Response.StatusCode);

            using var json = JsonDocument.Parse(cuerpo);
            Assert.Equal("detalle visible", json.RootElement.GetProperty("detail").GetString());
        }
    }
}
