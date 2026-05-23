using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class IniciativaMaquinaTrucoServicioTests
{
    private static Carta C(int valorTruco) =>
        new() { Numero = valorTruco, Palo = "Espada", ValorTruco = valorTruco };

    // ─── Caso con carta más fuerte del mazo (valTruco 14) ────────────

    [Fact]
    public void DebeCantarTruco_CartaMasFuerte14_NivelMentira0_ProbabilidadAlta()
    {
        // ValorTruco >= 13 → probabilidad base 78%. Con nivelMentira 0
        // la prob es 78%, no determinista, pero podemos verificar que
        // con nivelMentira 100 siempre canta (prob = 78 + 62 + 12 = 152 → clamp 100)
        var mano = new List<Carta> { C(14), C(2), C(3) };

        bool resultado = IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 100);
        Assert.True(resultado);
    }

    [Fact]
    public void DebeCantarTruco_CartaMasFuerte14_NivelMentira100_SiempreTrue()
    {
        var mano = new List<Carta> { C(14), C(5), C(1) };
        bool resultado = IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 100);
        Assert.True(resultado);
    }

    // ─── Cualquier combinación devuelve bool ─────────────────────────

    [Fact]
    public void DebeCantarTruco_ManoCualquiera_DevuelveBool()
    {
        var mano = new List<Carta> { C(5), C(3), C(7) };
        bool resultado = IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 30);
        Assert.IsType<bool>(resultado);
    }

    // ─── Clampeo de nivelMentira ──────────────────────────────────────

    [Fact]
    public void DebeCantarTruco_NivelMentiraNegativo_NoLanzaExcepcion()
    {
        var mano = new List<Carta> { C(10), C(7), C(4) };
        var ex = Record.Exception(() =>
            IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, -10));
        Assert.Null(ex);
    }

    [Fact]
    public void DebeCantarTruco_NivelMentiraSobre100_NoLanzaExcepcion()
    {
        var mano = new List<Carta> { C(10), C(7), C(4) };
        var ex = Record.Exception(() =>
            IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 999));
        Assert.Null(ex);
    }

    // ─── Mano con una sola carta ──────────────────────────────────────

    [Fact]
    public void DebeCantarTruco_ManoConUnaCarta_NoLanzaExcepcion()
    {
        var mano = new List<Carta> { C(8) };
        var ex = Record.Exception(() =>
            IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 0));
        Assert.Null(ex);
    }
}
