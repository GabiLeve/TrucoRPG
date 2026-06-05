using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class JuegoServicio2v2Tests
{
    // ── Helpers ──────────────────────────────────────────────────────

    private static Equipo2v2 EquipoA() => new()
    {
        Id = "EquipoA",
        Jugador1 = new Jugador { Id = "J1" },
        Jugador2 = new Jugador { Id = "J3" }
    };

    private static Equipo2v2 EquipoB() => new()
    {
        Id = "EquipoB",
        Jugador1 = new Jugador { Id = "J2" },
        Jugador2 = new Jugador { Id = "J4" }
    };

    private static Carta C(int valor) =>
        new() { Numero = 1, Palo = "Espada", ValorTruco = valor };

    private static Vuelta2v2 VueltaCon(
        int valorJ1, int valorJ2, int valorJ3, int valorJ4) =>
        new()
        {
            CartasJugadas = new Dictionary<string, Carta>
            {
                ["J1"] = C(valorJ1),
                ["J2"] = C(valorJ2),
                ["J3"] = C(valorJ3),
                ["J4"] = C(valorJ4)
            }
        };

    // ── ResolverVuelta ────────────────────────────────────────────────

    [Fact]
    public void ResolverVuelta_EquipoATieneMejorCarta_GanaEquipoA()
    {
        // Do
        var vuelta = VueltaCon(valorJ1: 14, valorJ2: 5, valorJ3: 3, valorJ4: 4);
        var equipoA = EquipoA();
        var equipoB = EquipoB();

        // To
        var resultado = JuegoServicio2v2.ResolverVuelta(vuelta, equipoA, equipoB);

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    [Fact]
    public void ResolverVuelta_EquipoBTieneMejorCarta_GanaEquipoB()
    {
        // Do
        var vuelta = VueltaCon(valorJ1: 3, valorJ2: 14, valorJ3: 4, valorJ4: 5);
        var equipoA = EquipoA();
        var equipoB = EquipoB();

        // To
        var resultado = JuegoServicio2v2.ResolverVuelta(vuelta, equipoA, equipoB);

        // Where
        Assert.Equal("EquipoB", resultado);
    }

    [Fact]
    public void ResolverVuelta_MejorCartaIgual_EsParda()
    {
        // Do - EquipoA mejor = 10 (J1), EquipoB mejor = 10 (J2)
        var vuelta = VueltaCon(valorJ1: 10, valorJ2: 10, valorJ3: 5, valorJ4: 5);
        var equipoA = EquipoA();
        var equipoB = EquipoB();

        // To
        var resultado = JuegoServicio2v2.ResolverVuelta(vuelta, equipoA, equipoB);

        // Where
        Assert.Equal("Parda", resultado);
    }

    [Fact]
    public void ResolverVuelta_J3TieneLaMejorDelEquipoA_GanaEquipoA()
    {
        // Do - J1=5, J3=12 → mejor de A = 12; J2=10, J4=9 → mejor de B = 10
        var vuelta = VueltaCon(valorJ1: 5, valorJ2: 10, valorJ3: 12, valorJ4: 9);
        var equipoA = EquipoA();
        var equipoB = EquipoB();

        // To
        var resultado = JuegoServicio2v2.ResolverVuelta(vuelta, equipoA, equipoB);

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    [Fact]
    public void ResolverVuelta_J4TieneLaMejorDelEquipoB_GanaEquipoB()
    {
        // Do - J1=8, J3=7 → mejor de A = 8; J2=6, J4=14 → mejor de B = 14
        var vuelta = VueltaCon(valorJ1: 8, valorJ2: 6, valorJ3: 7, valorJ4: 14);
        var equipoA = EquipoA();
        var equipoB = EquipoB();

        // To
        var resultado = JuegoServicio2v2.ResolverVuelta(vuelta, equipoA, equipoB);

        // Where
        Assert.Equal("EquipoB", resultado);
    }

    [Fact]
    public void ResolverVuelta_SetMejorCartasEnVuelta_EquipoA()
    {
        // Do
        var vuelta = VueltaCon(14, 13, 3, 4);
        var equipoA = EquipoA();
        var equipoB = EquipoB();

        // To
        JuegoServicio2v2.ResolverVuelta(vuelta, equipoA, equipoB);

        // Where - verifica que el helper seteó las mejores cartas
        Assert.NotNull(vuelta.MejorCartaEquipoA);
        Assert.Equal(14, vuelta.MejorCartaEquipoA!.ValorTruco);
        Assert.NotNull(vuelta.MejorCartaEquipoB);
        Assert.Equal(13, vuelta.MejorCartaEquipoB!.ValorTruco);
    }

    // ── ResolverGanadorMano ───────────────────────────────────────────

    [Fact]
    public void ResolverGanadorMano_SinVueltas_RetornaNull()
    {
        // Do
        var ganadoresVueltas = new List<string>();

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Null(resultado);
    }

    [Fact]
    public void ResolverGanadorMano_SoloUnaVuelta_RetornaNull()
    {
        // Do
        var ganadoresVueltas = new List<string> { "EquipoA" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Null(resultado);
    }

    [Fact]
    public void ResolverGanadorMano_EquipoAGanaDosVueltas_GanaEquipoA()
    {
        // Do
        var ganadoresVueltas = new List<string> { "EquipoA", "EquipoA" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_EquipoBGanaDosVueltas_GanaEquipoB()
    {
        // Do
        var ganadoresVueltas = new List<string> { "EquipoB", "EquipoB" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoB", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_UnoUnoSinTercera_RetornaNull()
    {
        // Do
        var ganadoresVueltas = new List<string> { "EquipoA", "EquipoB" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Null(resultado);
    }

    [Fact]
    public void ResolverGanadorMano_UnoUnoEquipoAEnTercera_GanaEquipoA()
    {
        // Do
        var ganadoresVueltas = new List<string> { "EquipoA", "EquipoB", "EquipoA" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_UnoUnoEquipoBEnTercera_GanaEquipoB()
    {
        // Do
        var ganadoresVueltas = new List<string> { "EquipoA", "EquipoB", "EquipoB" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoB", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_EquipoAGanaPrimera_PardaSegunda_GanaEquipoA()
    {
        // Do - Regla: ganó 1ra + parda 2da → gana quien ganó 1ra
        var ganadoresVueltas = new List<string> { "EquipoA", "Parda" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_EquipoBGanaPrimera_PardaSegunda_GanaEquipoB()
    {
        // Do
        var ganadoresVueltas = new List<string> { "EquipoB", "Parda" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoB", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_EquipoAGanaSegunda_GanaEquipoA()
    {
        // Do - Parda 1ra → decide la 2da
        var ganadoresVueltas = new List<string> { "Parda", "EquipoA" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_EquipoBGanaSegunda_GanaEquipoB()
    {
        // Do
        var ganadoresVueltas = new List<string> { "Parda", "EquipoB" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoB", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_PardaSegunda_SinTercera_RetornaNull()
    {
        // Do
        var ganadoresVueltas = new List<string> { "Parda", "Parda" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Null(resultado);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPardaEquipoAEnTercera_GanaEquipoA()
    {
        // Do
        var ganadoresVueltas = new List<string> { "Parda", "Parda", "EquipoA" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoB");

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPardaParda_EquipoManoEsA_GanaEquipoA()
    {
        // Do - Todas pardas → gana el equipo mano
        var ganadoresVueltas = new List<string> { "Parda", "Parda", "Parda" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoA");

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPardaParda_EquipoManoEsB_GanaEquipoB()
    {
        // Do
        var ganadoresVueltas = new List<string> { "Parda", "Parda", "Parda" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoB");

        // Where
        Assert.Equal("EquipoB", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_EquipoAGanaPrimera_EquipoBGanaSegunda_PardaTercera_GanaEquipoA()
    {
        // Do - 1ra y 2da distintas, parda en 3ra → gana quien ganó 1ra
        var ganadoresVueltas = new List<string> { "EquipoA", "EquipoB", "Parda" };

        // To
        var resultado = JuegoServicio2v2.ResolverGanadorMano(ganadoresVueltas, "EquipoB");

        // Where
        Assert.Equal("EquipoA", resultado);
    }

    // ── SumarPuntos ───────────────────────────────────────────────────

    [Fact]
    public void SumarPuntos_EquipoA_IncrementaPuntosEquipoA()
    {
        // Do
        var mano = new ManoTruco2v2 { PuntosEquipoA = 5, PuntosEquipoB = 3 };

        // To
        JuegoServicio2v2.SumarPuntos(mano, "EquipoA", 4);

        // Where
        Assert.Equal(9, mano.PuntosEquipoA);
        Assert.Equal(3, mano.PuntosEquipoB);
    }

    [Fact]
    public void SumarPuntos_EquipoB_IncrementaPuntosEquipoB()
    {
        // Do
        var mano = new ManoTruco2v2 { PuntosEquipoA = 5, PuntosEquipoB = 3 };

        // To
        JuegoServicio2v2.SumarPuntos(mano, "EquipoB", 2);

        // Where
        Assert.Equal(5, mano.PuntosEquipoA);
        Assert.Equal(5, mano.PuntosEquipoB);
    }

    [Fact]
    public void SumarPuntos_EquipoALlegaA30_TerminaPartidaConGanadorA()
    {
        // Do
        var mano = new ManoTruco2v2 { PuntosEquipoA = 28, PuntosEquipoB = 15 };

        // To
        JuegoServicio2v2.SumarPuntos(mano, "EquipoA", 3);

        // Where
        Assert.True(mano.PartidaTerminada);
        Assert.Equal("EquipoA", mano.GanadorPartida);
    }

    [Fact]
    public void SumarPuntos_EquipoBLlegaA30_TerminaPartidaConGanadorB()
    {
        // Do
        var mano = new ManoTruco2v2 { PuntosEquipoA = 10, PuntosEquipoB = 29 };

        // To
        JuegoServicio2v2.SumarPuntos(mano, "EquipoB", 2);

        // Where
        Assert.True(mano.PartidaTerminada);
        Assert.Equal("EquipoB", mano.GanadorPartida);
    }

    [Fact]
    public void SumarPuntos_CeroPuntos_NoModificaEstado()
    {
        // Do
        var mano = new ManoTruco2v2 { PuntosEquipoA = 5, PuntosEquipoB = 3 };

        // To
        JuegoServicio2v2.SumarPuntos(mano, "EquipoA", 0);

        // Where
        Assert.Equal(5, mano.PuntosEquipoA);
        Assert.False(mano.PartidaTerminada);
    }
}
