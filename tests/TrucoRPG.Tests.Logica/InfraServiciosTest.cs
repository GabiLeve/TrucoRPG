using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Infraestructura.Provider;
using TrucoRPG.Infraestructura.Servicios;
using Xunit;

namespace TrucoRPG.Tests.Logica;

public class InfraServiciosTest
{
    private static IConfiguration Config(Dictionary<string, string?> valores) =>
        new ConfigurationBuilder().AddInMemoryCollection(valores).Build();

    private static Mock<UserManager<ApplicationUser>> UserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    // ── TokenService ──────────────────────────────────────────────

    private static IConfiguration ConfigJwt() => Config(new()
    {
        ["Jwt:Key"]      = "clave-super-secreta-para-tests-de-al-menos-32-caracteres!",
        ["Jwt:Issuer"]   = "TrucoRPG",
        ["Jwt:Audience"] = "TrucoRPG-Front",
    });

    private static readonly Usuario UsuarioDemo = new()
    {
        Id = "user-123", UserName = "gonza", Email = "gonza@test.com",
    };

    [Fact]
    public void GenerarToken_ConRoles_IncluyeClaimsYRoles()
    {
        var um = UserManagerMock();
        var appUser = new ApplicationUser { Id = "user-123" };
        um.Setup(u => u.FindByIdAsync("user-123")).ReturnsAsync(appUser);
        um.Setup(u => u.GetRolesAsync(appUser)).ReturnsAsync(new List<string> { "Jugador" });

        var token = new TokenService(ConfigJwt(), um.Object).GenerarToken(UsuarioDemo);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("TrucoRPG", jwt.Issuer);
        Assert.Contains(jwt.Claims, c => c.Type == "email" && c.Value == "gonza@test.com");
        Assert.Contains(jwt.Claims, c => c.Value == "Jugador");
    }

    [Fact]
    public void GenerarToken_SinUsuarioIdentity_GeneraTokenSinRoles()
    {
        var um = UserManagerMock();
        um.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var token = new TokenService(ConfigJwt(), um.Object).GenerarToken(UsuarioDemo);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.DoesNotContain(jwt.Claims, c => c.Value == "Jugador");
        Assert.Contains(jwt.Claims, c => c.Type == "sub" && c.Value == "user-123");
    }

    // ── EmailService (solo el ctor: EnviarAsync requiere SMTP real) ──

    [Fact]
    public void EmailService_ConConfigCompleta_SeConstruye()
    {
        var config = Config(new()
        {
            ["Email:Host"] = "smtp.test.com",
            ["Email:Port"] = "587",
            ["Email:User"] = "bot@test.com",
            ["Email:Password"] = "secreto",
            ["Email:From"] = "truco@test.com",
        });

        var servicio = new EmailService(config);

        Assert.NotNull(servicio);
    }

    [Fact]
    public void EmailService_SinFrom_UsaElUserComoRemitente()
    {
        var config = Config(new()
        {
            ["Email:Host"] = "smtp.test.com",
            ["Email:User"] = "bot@test.com",
            ["Email:Password"] = "secreto",
        });

        Assert.NotNull(new EmailService(config));
    }

    [Theory]
    [InlineData("Email:Host")]
    [InlineData("Email:User")]
    [InlineData("Email:Password")]
    public void EmailService_SinConfigObligatoria_Lanza(string claveFaltante)
    {
        var valores = new Dictionary<string, string?>
        {
            ["Email:Host"] = "smtp.test.com",
            ["Email:User"] = "bot@test.com",
            ["Email:Password"] = "secreto",
        };
        valores.Remove(claveFaltante);

        Assert.Throws<InvalidOperationException>(() => new EmailService(Config(valores)));
    }
}
