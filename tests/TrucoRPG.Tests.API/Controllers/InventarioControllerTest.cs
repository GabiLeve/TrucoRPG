using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TrucoRPG.API.Controllers;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;
using Xunit;

namespace TrucoRPG.Tests.API
{
    public class InventarioControllerTest
    {
        private readonly Mock<IInventarioRepositorio> _inventarioMock = new();
        private readonly Mock<IUsuarioRepositorio> _usuariosMock = new();
        private readonly InventarioController _controller;

        private const string UserId = "user-123";

        public InventarioControllerTest()
        {
            _controller = new InventarioController(
                new ObtenerInventarioDelUsuarioUseCase(_inventarioMock.Object, _usuariosMock.Object),
                new ObtenerMonedasUseCase(_usuariosMock.Object));
        }

        private void AutenticarUsuario(string? userId)
        {
            var identity = userId is null
                ? new ClaimsIdentity()
                : new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task TraerInventario_ConUsuarioValido_RetornaOkConItemsYMonedas()
        {
            AutenticarUsuario(UserId);
            _usuariosMock.Setup(r => r.ObtenerPorIdAsync(UserId))
                .ReturnsAsync(new Usuario { Id = UserId, Monedas = 120 });
            _inventarioMock.Setup(r => r.ObtenerInventarioDeUsuario(UserId))
                .ReturnsAsync(new List<Inventario>
                {
                    new() { UsuarioId = UserId, ItemTiendaId = 1, Cantidad = 2 }
                });

            var resultado = await _controller.TraerInventario();

            var ok = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        }

        [Fact]
        public async Task TraerInventario_SinClaimDeUsuario_RetornaUnauthorized()
        {
            AutenticarUsuario(null);

            var resultado = await _controller.TraerInventario();

            Assert.IsType<UnauthorizedObjectResult>(resultado);
        }

        [Fact]
        public async Task TraerInventario_CuandoElUsuarioNoExiste_RetornaBadRequest()
        {
            AutenticarUsuario(UserId);
            _usuariosMock.Setup(r => r.ObtenerPorIdAsync(UserId)).ReturnsAsync((Usuario?)null);

            var resultado = await _controller.TraerInventario();

            Assert.IsType<BadRequestObjectResult>(resultado);
        }
    }
}
