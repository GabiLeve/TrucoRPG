using Microsoft.AspNetCore.Identity;
using Moq;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Infraestructura.Repositorios;
using Xunit;

namespace TrucoRPG.Tests.Logica;

public class UsuarioRepositorioTest
{
    private readonly Mock<UserManager<ApplicationUser>> _umMock;
    private readonly UsuarioRepositorio _repo;

    private static readonly ApplicationUser AppUserDemo = new()
    {
        Id = "user-123", UserName = "gonza", Email = "gonza@test.com", Monedas = 50,
    };

    public UsuarioRepositorioTest()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _umMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _repo = new UsuarioRepositorio(_umMock.Object);
    }

    // ── Obtener / existe ──────────────────────────────────────────

    [Fact]
    public async Task ObtenerPorEmail_UsuarioExistente_MapeaAUsuario()
    {
        _umMock.Setup(u => u.FindByEmailAsync("gonza@test.com")).ReturnsAsync(AppUserDemo);

        var usuario = await _repo.ObtenerPorEmailAsync("gonza@test.com");

        Assert.NotNull(usuario);
        Assert.Equal("user-123", usuario!.Id);
        Assert.Equal(50, usuario.Monedas);
    }

    [Fact]
    public async Task ObtenerPorEmail_Inexistente_DevuelveNull()
    {
        _umMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        Assert.Null(await _repo.ObtenerPorEmailAsync("nadie@test.com"));
        Assert.False(await _repo.ExisteEmailAsync("nadie@test.com"));
    }

    [Fact]
    public async Task ObtenerPorId_UsuarioExistente_MapeaAUsuario()
    {
        _umMock.Setup(u => u.FindByIdAsync("user-123")).ReturnsAsync(AppUserDemo);

        var usuario = await _repo.ObtenerPorIdAsync("user-123");

        Assert.Equal("gonza", usuario!.UserName);
    }

    // ── Crear ─────────────────────────────────────────────────────

    [Fact]
    public async Task Crear_Exitoso_AsignaRolJugador()
    {
        _umMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), "pass123"))
            .ReturnsAsync(IdentityResult.Success);
        _umMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Jugador"))
            .ReturnsAsync(IdentityResult.Success);

        await _repo.CrearAsync("gonza", "gonza@test.com", "pass123");

        _umMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Jugador"), Times.Once);
    }

    [Fact]
    public async Task Crear_SiIdentityFalla_LanzaConLosErrores()
    {
        _umMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password débil" }));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.CrearAsync("gonza", "gonza@test.com", "123"));
        Assert.Contains("Password débil", ex.Message);
    }

    [Fact]
    public async Task Crear_SiFallaElRol_Lanza()
    {
        _umMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _umMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Sin rol" }));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.CrearAsync("gonza", "gonza@test.com", "pass123"));
    }

    // ── Password ──────────────────────────────────────────────────

    [Fact]
    public async Task ValidarPassword_UsuarioInexistente_DevuelveFalse()
    {
        _umMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        Assert.False(await _repo.ValidarPasswordAsync("nadie@test.com", "pass"));
    }

    [Fact]
    public async Task ValidarPassword_ConPasswordCorrecta_DevuelveTrue()
    {
        _umMock.Setup(u => u.FindByEmailAsync("gonza@test.com")).ReturnsAsync(AppUserDemo);
        _umMock.Setup(u => u.CheckPasswordAsync(AppUserDemo, "pass")).ReturnsAsync(true);

        Assert.True(await _repo.ValidarPasswordAsync("gonza@test.com", "pass"));
    }

    [Fact]
    public async Task CambiarPassword_Exitoso_NoLanza()
    {
        _umMock.Setup(u => u.FindByIdAsync("user-123")).ReturnsAsync(AppUserDemo);
        _umMock.Setup(u => u.ChangePasswordAsync(AppUserDemo, "vieja", "nueva"))
            .ReturnsAsync(IdentityResult.Success);

        await _repo.CambiarPasswordAsync("user-123", "vieja", "nueva");

        _umMock.Verify(u => u.ChangePasswordAsync(AppUserDemo, "vieja", "nueva"), Times.Once);
    }

    [Fact]
    public async Task CambiarPassword_SiFalla_LanzaConElError()
    {
        _umMock.Setup(u => u.FindByIdAsync("user-123")).ReturnsAsync(AppUserDemo);
        _umMock.Setup(u => u.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "No coincide" }));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.CambiarPasswordAsync("user-123", "mala", "nueva"));
        Assert.Contains("No coincide", ex.Message);
    }

    [Fact]
    public async Task GenerarTokenReset_UsuarioExistente_DevuelveToken()
    {
        _umMock.Setup(u => u.FindByEmailAsync("gonza@test.com")).ReturnsAsync(AppUserDemo);
        _umMock.Setup(u => u.GeneratePasswordResetTokenAsync(AppUserDemo)).ReturnsAsync("token-abc");

        Assert.Equal("token-abc", await _repo.GenerarTokenResetPasswordAsync("gonza@test.com"));
    }

    [Fact]
    public async Task GenerarTokenReset_UsuarioInexistente_Lanza()
    {
        _umMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.GenerarTokenResetPasswordAsync("nadie@test.com"));
    }

    [Fact]
    public async Task ResetPasswordConToken_Exitoso_NoLanza()
    {
        _umMock.Setup(u => u.FindByEmailAsync("gonza@test.com")).ReturnsAsync(AppUserDemo);
        _umMock.Setup(u => u.ResetPasswordAsync(AppUserDemo, "token", "nueva123"))
            .ReturnsAsync(IdentityResult.Success);

        await _repo.ResetPasswordConTokenAsync("gonza@test.com", "token", "nueva123");

        _umMock.Verify(u => u.ResetPasswordAsync(AppUserDemo, "token", "nueva123"), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordConToken_TokenInvalido_Lanza()
    {
        _umMock.Setup(u => u.FindByEmailAsync("gonza@test.com")).ReturnsAsync(AppUserDemo);
        _umMock.Setup(u => u.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Token expirado" }));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.ResetPasswordConTokenAsync("gonza@test.com", "viejo", "nueva123"));
    }

    // ── Personaje ─────────────────────────────────────────────────

    [Fact]
    public async Task CrearPersonaje_UsuarioSinPersonaje_ActualizaAlUsuario()
    {
        var sinPersonaje = new ApplicationUser { Id = "user-123" };
        _umMock.Setup(u => u.FindByIdAsync("user-123")).ReturnsAsync(sinPersonaje);
        _umMock.Setup(u => u.UpdateAsync(sinPersonaje)).ReturnsAsync(IdentityResult.Success);

        var heroeId = Guid.NewGuid();
        await _repo.CrearPersonaje("user-123", "gaucho", heroeId);

        Assert.Equal("gaucho", sinPersonaje.SpriteKey);
        Assert.Equal(heroeId, sinPersonaje.HeroeSeleccionadoId);
    }

    [Fact]
    public async Task CrearPersonaje_SiYaTiene_Lanza()
    {
        var conPersonaje = new ApplicationUser { Id = "user-123", SpriteKey = "gaucho" };
        _umMock.Setup(u => u.FindByIdAsync("user-123")).ReturnsAsync(conPersonaje);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.CrearPersonaje("user-123", "otro", Guid.NewGuid()));
    }

    [Fact]
    public async Task PersonajeExistente_DetectaAmbosCasos()
    {
        var con = new ApplicationUser { Id = "a", SpriteKey = "gaucho", HeroeSeleccionadoId = Guid.NewGuid() };
        var sin = new ApplicationUser { Id = "b" };
        _umMock.Setup(u => u.FindByIdAsync("a")).ReturnsAsync(con);
        _umMock.Setup(u => u.FindByIdAsync("b")).ReturnsAsync(sin);

        Assert.True(await _repo.PersonajeExistente("a"));
        Assert.False(await _repo.PersonajeExistente("b"));
    }

    [Fact]
    public async Task ObtenerPersonaje_ConPersonaje_LoDevuelve()
    {
        var heroeId = Guid.NewGuid();
        var con = new ApplicationUser { Id = "a", SpriteKey = "gaucho", HeroeSeleccionadoId = heroeId };
        _umMock.Setup(u => u.FindByIdAsync("a")).ReturnsAsync(con);

        var personaje = await _repo.ObtenerPersonajeDelUsuario("a");

        Assert.Equal("gaucho", personaje.SpriteKey);
        Assert.Equal(heroeId, personaje.HeroeId);
    }

    [Fact]
    public async Task ObtenerPersonaje_SinPersonaje_Lanza()
    {
        _umMock.Setup(u => u.FindByIdAsync("b")).ReturnsAsync(new ApplicationUser { Id = "b" });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repo.ObtenerPersonajeDelUsuario("b"));
    }

    // ── Monedas ───────────────────────────────────────────────────

    [Fact]
    public async Task ActualizarMonedas_UsuarioExistente_ActualizaYDevuelveTrue()
    {
        var user = new ApplicationUser { Id = "user-123", Monedas = 50 };
        _umMock.Setup(u => u.FindByIdAsync("user-123")).ReturnsAsync(user);
        _umMock.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var ok = await _repo.ActualizarMonedasAsync("user-123", 80);

        Assert.True(ok);
        Assert.Equal(80, user.Monedas);
    }

    [Fact]
    public async Task ActualizarMonedas_UsuarioInexistente_DevuelveFalse()
    {
        _umMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        Assert.False(await _repo.ActualizarMonedasAsync("nadie", 80));
    }
}
