using Moq;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;
using Xunit;

namespace TrucoRPG.Tests.Logica;

public class AuthUseCasesTest
{
    private readonly Mock<IUsuarioRepositorio> _usuariosMock = new();
    private readonly Mock<ITokenService> _tokenMock = new();
    private readonly Mock<IProgresoPartidaRepositorio> _progresoMock = new();
    private readonly Mock<IEmailService> _emailMock = new();

    private static readonly Usuario UsuarioDemo = new()
    {
        Id = "user-123", UserName = "gonza", Email = "gonza@test.com",
    };

    // ── LoginUseCase ──────────────────────────────────────────────

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveToken()
    {
        _usuariosMock.Setup(r => r.ValidarPasswordAsync("gonza@test.com", "pass")).ReturnsAsync(true);
        _usuariosMock.Setup(r => r.ObtenerPorEmailAsync("gonza@test.com")).ReturnsAsync(UsuarioDemo);
        _tokenMock.Setup(t => t.GenerarToken(UsuarioDemo)).Returns("jwt-token");

        var useCase = new LoginUseCase(_usuariosMock.Object, _tokenMock.Object);
        var token = await useCase.EjecutarAsync("gonza@test.com", "pass");

        Assert.Equal("jwt-token", token);
    }

    [Fact]
    public async Task Login_ConPasswordInvalida_LanzaUnauthorized()
    {
        _usuariosMock.Setup(r => r.ValidarPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        var useCase = new LoginUseCase(_usuariosMock.Object, _tokenMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => useCase.EjecutarAsync("gonza@test.com", "mala"));
    }

    [Fact]
    public async Task Login_UsuarioInexistenteTrasValidar_LanzaUnauthorized()
    {
        _usuariosMock.Setup(r => r.ValidarPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _usuariosMock.Setup(r => r.ObtenerPorEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Usuario?)null);

        var useCase = new LoginUseCase(_usuariosMock.Object, _tokenMock.Object);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => useCase.EjecutarAsync("gonza@test.com", "pass"));
    }

    // ── RegisterUseCase ───────────────────────────────────────────

    [Fact]
    public async Task Register_ConEmailNuevo_CreaUsuarioProgresoYDevuelveToken()
    {
        _usuariosMock.Setup(r => r.ExisteEmailAsync("gonza@test.com")).ReturnsAsync(false);
        _usuariosMock.Setup(r => r.ObtenerPorEmailAsync("gonza@test.com")).ReturnsAsync(UsuarioDemo);
        _tokenMock.Setup(t => t.GenerarToken(UsuarioDemo)).Returns("jwt-nuevo");

        var useCase = new RegisterUseCase(_usuariosMock.Object, _tokenMock.Object, _progresoMock.Object);
        var token = await useCase.EjecutarAsync("gonza", "gonza@test.com", "pass123");

        Assert.Equal("jwt-nuevo", token);
        _usuariosMock.Verify(r => r.CrearAsync("gonza", "gonza@test.com", "pass123"), Times.Once);
        _progresoMock.Verify(r => r.ObtenerOCrearAsync(UsuarioDemo.Id), Times.Once);
    }

    [Fact]
    public async Task Register_ConEmailExistente_LanzaInvalidOperation()
    {
        _usuariosMock.Setup(r => r.ExisteEmailAsync("usado@test.com")).ReturnsAsync(true);

        var useCase = new RegisterUseCase(_usuariosMock.Object, _tokenMock.Object, _progresoMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.EjecutarAsync("otro", "usado@test.com", "pass123"));
        _usuariosMock.Verify(r => r.CrearAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Register_SiFallaLaRecuperacionDelUsuario_LanzaInvalidOperation()
    {
        _usuariosMock.Setup(r => r.ExisteEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        _usuariosMock.Setup(r => r.ObtenerPorEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);

        var useCase = new RegisterUseCase(_usuariosMock.Object, _tokenMock.Object, _progresoMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.EjecutarAsync("gonza", "gonza@test.com", "pass123"));
    }

    // ── CambiarPasswordUseCase ────────────────────────────────────

    [Fact]
    public async Task CambiarPassword_ConPasswordValida_DelegaAlRepositorio()
    {
        var useCase = new CambiarPasswordUseCase(_usuariosMock.Object);

        await useCase.EjecutarAsync("user-123", "vieja123", "nueva123");

        _usuariosMock.Verify(r => r.CambiarPasswordAsync("user-123", "vieja123", "nueva123"), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("corta")]
    public async Task CambiarPassword_ConPasswordInvalida_LanzaInvalidOperation(string passwordNueva)
    {
        var useCase = new CambiarPasswordUseCase(_usuariosMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.EjecutarAsync("user-123", "vieja123", passwordNueva));
        _usuariosMock.Verify(r => r.CambiarPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ── ResetPasswordUseCase ──────────────────────────────────────

    [Fact]
    public async Task ResetPassword_ConPasswordValida_DelegaAlRepositorio()
    {
        var useCase = new ResetPasswordUseCase(_usuariosMock.Object);

        await useCase.EjecutarAsync("gonza@test.com", "token-abc", "nueva123");

        _usuariosMock.Verify(r => r.ResetPasswordConTokenAsync("gonza@test.com", "token-abc", "nueva123"), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_ConPasswordCorta_LanzaInvalidOperation()
    {
        var useCase = new ResetPasswordUseCase(_usuariosMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.EjecutarAsync("gonza@test.com", "token-abc", "abc"));
    }

    // ── SolicitarResetPasswordUseCase ─────────────────────────────

    [Fact]
    public async Task SolicitarReset_GeneraTokenYEnviaEmailConLink()
    {
        _usuariosMock.Setup(r => r.GenerarTokenResetPasswordAsync("gonza@test.com"))
            .ReturnsAsync("token con espacios");

        string? cuerpoEnviado = null;
        _emailMock.Setup(e => e.EnviarAsync("gonza@test.com", It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((_, _, cuerpo) => cuerpoEnviado = cuerpo)
            .Returns(Task.CompletedTask);

        var useCase = new SolicitarResetPasswordUseCase(
            _usuariosMock.Object, _emailMock.Object, "https://front.test");

        await useCase.EjecutarAsync("gonza@test.com");

        Assert.NotNull(cuerpoEnviado);
        // El link debe apuntar al frontend con email y token URL-encodeados
        Assert.Contains("https://front.test/reset-password?email=gonza%40test.com", cuerpoEnviado);
        Assert.Contains("token%20con%20espacios", cuerpoEnviado);
        _emailMock.Verify(e => e.EnviarAsync("gonza@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
