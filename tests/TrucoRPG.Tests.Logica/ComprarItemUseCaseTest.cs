using Moq;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;
using Xunit;

namespace TrucoRPG.Tests.Logica;

public class ComprarItemUseCaseTest
{
    private readonly Mock<IInventarioRepositorio> _inventarioMock = new();
    private readonly Mock<IUsuarioRepositorio> _usuariosMock = new();
    private readonly Mock<IItemTiendaRepositorio> _itemsMock = new();
    private readonly ComprarItemUseCase _useCase;

    private const string UserId = "user-123";

    public ComprarItemUseCaseTest()
    {
        _useCase = new ComprarItemUseCase(
            _inventarioMock.Object, _usuariosMock.Object, _itemsMock.Object);
    }

    private void DadoItem(int id, int precio, bool acumulable = false) =>
        _itemsMock.Setup(r => r.ObtenerItemPorIdAsync(id))
            .ReturnsAsync(new ItemTienda { Id = id, Precio = precio, Acumulable = acumulable });

    private void DadoUsuarioConMonedas(int monedas) =>
        _usuariosMock.Setup(r => r.ObtenerPorIdAsync(UserId))
            .ReturnsAsync(new Usuario { Id = UserId, Monedas = monedas });

    [Fact]
    public async Task Ejecutar_CompraValida_DescuentaMonedasYAgregaItem()
    {
        DadoItem(1, precio: 50);
        DadoUsuarioConMonedas(200);
        _inventarioMock.Setup(r => r.ItemExistente(UserId, 1)).ReturnsAsync(false);
        _usuariosMock.Setup(r => r.ActualizarMonedasAsync(UserId, 150)).ReturnsAsync(true);
        _inventarioMock.Setup(r => r.Agregar(UserId, 1, 1)).ReturnsAsync(true);

        var resultado = await _useCase.Ejecutar(UserId, 1);

        Assert.True(resultado);
        _usuariosMock.Verify(r => r.ActualizarMonedasAsync(UserId, 150), Times.Once);
        _inventarioMock.Verify(r => r.Agregar(UserId, 1, 1), Times.Once);
    }

    [Fact]
    public async Task Ejecutar_ItemAcumulable_NoValidaSiYaLoTiene()
    {
        DadoItem(1, precio: 50, acumulable: true);
        DadoUsuarioConMonedas(200);
        _usuariosMock.Setup(r => r.ActualizarMonedasAsync(UserId, 150)).ReturnsAsync(true);
        _inventarioMock.Setup(r => r.Agregar(UserId, 1, 1)).ReturnsAsync(true);

        var resultado = await _useCase.Ejecutar(UserId, 1);

        Assert.True(resultado);
        _inventarioMock.Verify(r => r.ItemExistente(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Ejecutar_ItemInexistente_LanzaExcepcion()
    {
        _itemsMock.Setup(r => r.ObtenerItemPorIdAsync(99)).ReturnsAsync((ItemTienda?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() => _useCase.Ejecutar(UserId, 99));
        Assert.Contains("no existe", ex.Message);
    }

    [Fact]
    public async Task Ejecutar_UsuarioInexistente_LanzaExcepcion()
    {
        DadoItem(1, precio: 50);
        _usuariosMock.Setup(r => r.ObtenerPorIdAsync(UserId)).ReturnsAsync((Usuario?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() => _useCase.Ejecutar(UserId, 1));
        Assert.Contains("usuario no existe", ex.Message);
    }

    [Fact]
    public async Task Ejecutar_ItemNoAcumulableYaComprado_LanzaExcepcion()
    {
        DadoItem(1, precio: 50, acumulable: false);
        DadoUsuarioConMonedas(200);
        _inventarioMock.Setup(r => r.ItemExistente(UserId, 1)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<Exception>(() => _useCase.Ejecutar(UserId, 1));
        Assert.Contains("ya tiene este item", ex.Message);
    }

    [Fact]
    public async Task Ejecutar_MonedasInsuficientes_LanzaExcepcion()
    {
        DadoItem(1, precio: 500);
        DadoUsuarioConMonedas(10);
        _inventarioMock.Setup(r => r.ItemExistente(UserId, 1)).ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<Exception>(() => _useCase.Ejecutar(UserId, 1));
        Assert.Contains("suficientes monedas", ex.Message);
    }

    [Fact]
    public async Task Ejecutar_FallaAlActualizarMonedas_LanzaExcepcion()
    {
        DadoItem(1, precio: 50);
        DadoUsuarioConMonedas(200);
        _inventarioMock.Setup(r => r.ItemExistente(UserId, 1)).ReturnsAsync(false);
        _usuariosMock.Setup(r => r.ActualizarMonedasAsync(UserId, 150)).ReturnsAsync(false);

        await Assert.ThrowsAsync<Exception>(() => _useCase.Ejecutar(UserId, 1));
        _inventarioMock.Verify(r => r.Agregar(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}
