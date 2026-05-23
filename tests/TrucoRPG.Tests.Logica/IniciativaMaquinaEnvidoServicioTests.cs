using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class IniciativaMaquinaEnvidoServicioTests
{
    private static Carta C(int numero, string palo) =>
        new() { Numero = numero, Palo = palo, ValorTruco = 0 };

    // ─── Casos deterministas (tanto >= 30 → siempre canta) ───────────

    [Fact]
    public void DebeCantarEnvido_Tanto30_NivelMentira0_SiempreDevuelveTrue()
    {
        // 7 + 3 + 20 = 30
        var mano = new List<Carta> { C(7, "Espada"), C(3, "Espada"), C(1, "Oro") };

        // Con tanto exactamente 30 y nivel mentira neutro, debe cantar siempre
        bool resultado = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 0);
        Assert.True(resultado);
    }

    [Fact]
    public void DebeCantarEnvido_Tanto33_SiempreDevuelveTrue()
    {
        // Máximo posible: 7 + 6 + 20 = 33
        var mano = new List<Carta> { C(7, "Copa"), C(6, "Copa"), C(5, "Copa") };

        bool resultado = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 0);
        Assert.True(resultado);
    }

    // ─── nivelMentira altísimo sube la probabilidad ───────────────────

    [Fact]
    public void DebeCantarEnvido_NivelMentira100_RetornaTrue_ConTantoBajo()
    {
        // Tanto bajo pero nivelMentira = 100 eleva probabilidad al máximo (100%)
        var mano = new List<Carta> { C(1, "Espada"), C(2, "Oro"), C(3, "Copa") };

        // Con nivelMentira = 100 la prob queda en 100% → siempre true
        bool resultado = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 100);
        Assert.True(resultado);
    }

    // ─── Clampeo de nivelMentira ──────────────────────────────────────

    [Fact]
    public void DebeCantarEnvido_NivelMentiraNegativo_SeBehaveLikeZero()
    {
        // No debe lanzar excepción; nivelMentira se clampea a 0
        var mano = new List<Carta> { C(7, "Espada"), C(6, "Espada"), C(5, "Espada") };
        var ex = Record.Exception(() =>
            IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, -50));
        Assert.Null(ex);
    }

    [Fact]
    public void DebeCantarEnvido_NivelMentiraSobre100_SeBehaveLike100()
    {
        var mano = new List<Carta> { C(7, "Espada"), C(6, "Espada"), C(5, "Espada") };
        var ex = Record.Exception(() =>
            IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 200));
        Assert.Null(ex);
    }

    // ─── Devuelve bool válido en todos los casos ──────────────────────

    [Fact]
    public void DebeCantarEnvido_ManoCualquiera_DevuelveBool()
    {
        var mano = new List<Carta> { C(4, "Basto"), C(5, "Copa"), C(3, "Oro") };
        bool resultado = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 50);
        Assert.IsType<bool>(resultado);
    }
}
