using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TrucoRPG.API.Controllers;
using TrucoRPG.API.DTO;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;
using Xunit;

namespace TrucoRPG.Tests.API
{
    public class TiendaControllerTest
    {
        private readonly Mock<IItemTiendaRepositorio> _itemsMock = new();
        private readonly Mock<IUsuarioRepositorio> _usuariosMock = new();
        private readonly Mock<IInventarioRepositorio> _inventarioMock = new();
        private readonly TiendaController _controller;

        private const string UserId = "user-123";

        public TiendaControllerTest()
        {
            _controller = new TiendaController(
                new ObtenerTiendaUseCase(_itemsMock.Object),
                new ComprarItemUseCase(_inventarioMock.Object, _usuariosMock.Object, _itemsMock.Object),
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

        private static ItemTienda Item(int id, int precio, bool acumulable = false) =>
            new() { Id = id, Nombre = $"Item {id}", Precio = precio, Categoria = "General", Acumulable = acumulable };

        // ── Obtener tienda ────────────────────────────────────────

        [Fact]
        public async Task ObtenerTienda_ConUsuarioAutenticado_RetornaOkConCatalogoYMonedas()
        {
            AutenticarUsuario(UserId);
            _itemsMock.Setup(r => r.ObtenerTodosLosItemsAsync())
                .ReturnsAsync(new List<ItemTienda> { Item(1, 50), Item(2, 80) });
            _usuariosMock.Setup(r => r.ObtenerPorIdAsync(UserId))
                .ReturnsAsync(new Usuario { Id = UserId, Monedas = 200 });

            var resultado = await _controller.ObtenerTienda();

            var ok = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        }

        [Fact]
        public async Task ObtenerTienda_CuandoElUsuarioNoExiste_RetornaBadRequest()
        {
            // ObtenerMonedasUseCase lanza si no encuentra el usuario → catch → BadRequest
            AutenticarUsuario(UserId);
            _itemsMock.Setup(r => r.ObtenerTodosLosItemsAsync())
                .ReturnsAsync(new List<ItemTienda>());
            _usuariosMock.Setup(r => r.ObtenerPorIdAsync(UserId)).ReturnsAsync((Usuario?)null);

            var resultado = await _controller.ObtenerTienda();

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        // ── Comprar ───────────────────────────────────────────────

        [Fact]
        public async Task Comprar_ConMonedasSuficientes_RetornaOkYAgregaAlInventario()
        {
            AutenticarUsuario(UserId);
            _itemsMock.Setup(r => r.ObtenerItemPorIdAsync(1)).ReturnsAsync(Item(1, 50));
            _usuariosMock.Setup(r => r.ObtenerPorIdAsync(UserId))
                .ReturnsAsync(new Usuario { Id = UserId, Monedas = 200 });
            _inventarioMock.Setup(r => r.ItemExistente(UserId, 1)).ReturnsAsync(false);
            _usuariosMock.Setup(r => r.ActualizarMonedasAsync(UserId, 150)).ReturnsAsync(true);
            _inventarioMock.Setup(r => r.Agregar(UserId, 1, 1)).ReturnsAsync(true);

            var resultado = await _controller.Comprar(new ComprarItemDto { ItemTiendaId = 1 });

            Assert.IsType<OkObjectResult>(resultado);
            _usuariosMock.Verify(r => r.ActualizarMonedasAsync(UserId, 150), Times.Once);
            _inventarioMock.Verify(r => r.Agregar(UserId, 1, 1), Times.Once);
        }

        [Fact]
        public async Task Comprar_SinClaimDeUsuario_RetornaUnauthorized()
        {
            AutenticarUsuario(null);

            var resultado = await _controller.Comprar(new ComprarItemDto { ItemTiendaId = 1 });

            Assert.IsType<UnauthorizedObjectResult>(resultado);
        }

        [Fact]
        public async Task Comprar_SinMonedasSuficientes_RetornaBadRequest()
        {
            AutenticarUsuario(UserId);
            _itemsMock.Setup(r => r.ObtenerItemPorIdAsync(1)).ReturnsAsync(Item(1, 500));
            _usuariosMock.Setup(r => r.ObtenerPorIdAsync(UserId))
                .ReturnsAsync(new Usuario { Id = UserId, Monedas = 10 });

            var resultado = await _controller.Comprar(new ComprarItemDto { ItemTiendaId = 1 });

            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task Comprar_ItemInexistente_RetornaBadRequest()
        {
            AutenticarUsuario(UserId);
            _itemsMock.Setup(r => r.ObtenerItemPorIdAsync(99)).ReturnsAsync((ItemTienda?)null);

            var resultado = await _controller.Comprar(new ComprarItemDto { ItemTiendaId = 99 });

            Assert.IsType<BadRequestObjectResult>(resultado);
        }
    }
}
