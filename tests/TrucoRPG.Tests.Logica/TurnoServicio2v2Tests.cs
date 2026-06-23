using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class TurnoServicio2v2Tests
{
    // ── Helpers ──────────────────────────────────────────────────────

    private static ManoTruco2v2 CrearMano(string jugadorMano = "J1")
    {
        var j1 = new Jugador { Id = "J1" };
        var j2 = new Jugador { Id = "J2" };
        var j3 = new Jugador { Id = "J3" };
        var j4 = new Jugador { Id = "J4" };

        return new ManoTruco2v2
        {
            Posicion1    = j1,
            Posicion2    = j2,
            Posicion3    = j3,
            Posicion4    = j4,
            JugadorMano  = jugadorMano,
            TurnoActual  = jugadorMano,
            EquipoMano   = jugadorMano is "J1" or "J3" ? "EquipoA" : "EquipoB",
            EquipoA = new Equipo2v2 { Id = "EquipoA", Jugador1 = j1, Jugador2 = j3 },
            EquipoB = new Equipo2v2 { Id = "EquipoB", Jugador1 = j2, Jugador2 = j4 },
        };
    }

    // ── ObtenerOrdenTurno ─────────────────────────────────────────────

    [Fact]
    public void ObtenerOrdenTurno_ManoEsJ1_OrdenEsJ1J2J3J4()
    {
        // Do
        var mano = CrearMano("J1");

        // To
        var orden = TurnoServicio2v2.ObtenerOrdenTurno(mano);

        // Where
        Assert.Equal(new[] { "J1", "J2", "J3", "J4" }, orden);
    }

    [Fact]
    public void ObtenerOrdenTurno_ManoEsJ2_OrdenEsJ2J3J4J1()
    {
        // Do
        var mano = CrearMano("J2");

        // To
        var orden = TurnoServicio2v2.ObtenerOrdenTurno(mano);

        // Where
        Assert.Equal(new[] { "J2", "J3", "J4", "J1" }, orden);
    }

    [Fact]
    public void ObtenerOrdenTurno_ManoEsJ3_OrdenEsJ3J4J1J2()
    {
        // Do
        var mano = CrearMano("J3");

        // To
        var orden = TurnoServicio2v2.ObtenerOrdenTurno(mano);

        // Where
        Assert.Equal(new[] { "J3", "J4", "J1", "J2" }, orden);
    }

    [Fact]
    public void ObtenerOrdenTurno_ManoEsJ4_OrdenEsJ4J1J2J3()
    {
        // Do
        var mano = CrearMano("J4");

        // To
        var orden = TurnoServicio2v2.ObtenerOrdenTurno(mano);

        // Where
        Assert.Equal(new[] { "J4", "J1", "J2", "J3" }, orden);
    }

    // ── SiguienteJugador ──────────────────────────────────────────────

    [Fact]
    public void SiguienteJugador_ManoJ1_DespuesDeJ1_EsJ2()
    {
        // Do
        var mano = CrearMano("J1");

        // To
        var siguiente = TurnoServicio2v2.SiguienteJugador(mano, "J1");

        // Where
        Assert.Equal("J2", siguiente);
    }

    [Fact]
    public void SiguienteJugador_ManoJ1_DespuesDeJ4_EsNull()
    {
        // Do - J4 es el último en el orden J1→J2→J3→J4
        var mano = CrearMano("J1");

        // To
        var siguiente = TurnoServicio2v2.SiguienteJugador(mano, "J4");

        // Where
        Assert.Null(siguiente);
    }

    [Fact]
    public void SiguienteJugador_ManoJ2_DespuesDeJ2_EsJ3()
    {
        // Do
        var mano = CrearMano("J2");

        // To
        var siguiente = TurnoServicio2v2.SiguienteJugador(mano, "J2");

        // Where
        Assert.Equal("J3", siguiente);
    }

    // ── ObtenerPrimeroDeVueltaSiguiente ───────────────────────────────

    [Fact]
    public void ObtenerPrimeroDeVueltaSiguiente_GanadorEquipoA_ManoJ1_RetornaJ1()
    {
        // Do - EquipoA = J1,J3. El primero de A en el orden J1→J2→J3→J4 es J1.
        var mano = CrearMano("J1");

        // To
        var primero = TurnoServicio2v2.ObtenerPrimeroDeVueltaSiguiente(mano, "EquipoA");

        // Where
        Assert.Equal("J1", primero);
    }

    [Fact]
    public void ObtenerPrimeroDeVueltaSiguiente_GanadorEquipoB_ManoJ1_RetornaJ2()
    {
        // Do - EquipoB = J2,J4. El primero de B en el orden J1→J2→J3→J4 es J2.
        var mano = CrearMano("J1");

        // To
        var primero = TurnoServicio2v2.ObtenerPrimeroDeVueltaSiguiente(mano, "EquipoB");

        // Where
        Assert.Equal("J2", primero);
    }

    [Fact]
    public void ObtenerPrimeroDeVueltaSiguiente_Parda_RetornaJugadorMano()
    {
        // Do
        var mano = CrearMano("J2");

        // To
        var primero = TurnoServicio2v2.ObtenerPrimeroDeVueltaSiguiente(mano, "Parda");

        // Where
        Assert.Equal("J2", primero);
    }

    [Fact]
    public void ObtenerPrimeroDeVueltaSiguiente_GanadorNull_RetornaJugadorMano()
    {
        // Do
        var mano = CrearMano("J3");

        // To
        var primero = TurnoServicio2v2.ObtenerPrimeroDeVueltaSiguiente(mano, null);

        // Where
        Assert.Equal("J3", primero);
    }

    // ── ObtenerUltimoDelEquipoEnTurno ─────────────────────────────────

    [Fact]
    public void ObtenerUltimoDelEquipoEnTurno_ManoJ1_UltimoDeEquipoAEsJ3()
    {
        // Do - Orden J1→J2→J3→J4. EquipoA = J1,J3. Último de A = J3.
        var mano = CrearMano("J1");

        // To
        var ultimo = TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoA");

        // Where
        Assert.Equal("J3", ultimo);
    }

    [Fact]
    public void ObtenerUltimoDelEquipoEnTurno_ManoJ1_UltimoDeEquipoBEsJ4()
    {
        // Do - Orden J1→J2→J3→J4. EquipoB = J2,J4. Último de B = J4.
        var mano = CrearMano("J1");

        // To
        var ultimo = TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoB");

        // Where
        Assert.Equal("J4", ultimo);
    }

    [Fact]
    public void ObtenerUltimoDelEquipoEnTurno_ManoJ2_UltimoDeEquipoAEsJ1()
    {
        // Do - Orden J2→J3→J4→J1. EquipoA = J1,J3. Último de A = J1.
        var mano = CrearMano("J2");

        // To
        var ultimo = TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoA");

        // Where
        Assert.Equal("J1", ultimo);
    }

    [Fact]
    public void ObtenerUltimoDelEquipoEnTurno_ManoJ2_UltimoDeEquipoBEsJ2()
    {
        // Do - Orden J2→J3→J4→J1. EquipoB = J2,J4. Posiciones: J2=idx0, J4=idx2. Último = J4.
        // Wait: J2 idx0, J3 idx1, J4 idx2, J1 idx3. EquipoB: J2(idx0) y J4(idx2). Último = J4.
        var mano = CrearMano("J2");

        // To
        var ultimo = TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoB");

        // Where - J4 aparece más tarde que J2 en el orden J2→J3→J4→J1
        Assert.Equal("J4", ultimo);
    }

    // ── PuedeEscalarTruco ─────────────────────────────────────────────

    [Fact]
    public void PuedeEscalarTruco_EquipoACanto_UltimoDeEquipoB_J4_PuedeEscalar()
    {
        // Do - EquipoA cantó truco. Orden J1→J2→J3→J4. Último de B = J4.
        var mano = CrearMano("J1");
        mano.TrucoCantado      = true;
        mano.EquipoCantorTruco = "EquipoA";

        // To
        var puede = TurnoServicio2v2.PuedeEscalarTruco(mano, "J4");

        // Where
        Assert.True(puede);
    }

    [Fact]
    public void PuedeEscalarTruco_EquipoACanto_J2NoPuedeEscalar_NoEsUltimo()
    {
        // Do - EquipoA cantó truco. J2 es del EquipoB pero NO es el último (J4 es el último).
        var mano = CrearMano("J1");
        mano.TrucoCantado      = true;
        mano.EquipoCantorTruco = "EquipoA";

        // To
        var puede = TurnoServicio2v2.PuedeEscalarTruco(mano, "J2");

        // Where
        Assert.False(puede);
    }

    [Fact]
    public void PuedeEscalarTruco_EquipoACanto_J1NoPuedeEscalar_EsDelMismoEquipo()
    {
        // Do - EquipoA cantó truco. J1 es del EquipoA → no puede escalar (es el cantor).
        var mano = CrearMano("J1");
        mano.TrucoCantado      = true;
        mano.EquipoCantorTruco = "EquipoA";

        // To
        var puede = TurnoServicio2v2.PuedeEscalarTruco(mano, "J1");

        // Where
        Assert.False(puede);
    }

    [Fact]
    public void PuedeEscalarTruco_SinTrucoCantado_NadiePuedeEscalar()
    {
        // Do
        var mano = CrearMano("J1");
        mano.TrucoCantado = false;

        // To
        var puede = TurnoServicio2v2.PuedeEscalarTruco(mano, "J4");

        // Where
        Assert.False(puede);
    }

    // ── ObtenerAbreSiguienteVuelta ────────────────────────────────────

    [Fact]
    public void ObtenerAbreSiguienteVuelta_Parda_AbreElMano()
    {
        // Tras una parda, la vuelta siguiente la abre el jugador MANO de la ronda
        // (p. ej. si el compañero es mano, juega primero su carta).
        var mano = CrearMano("J3"); // el compañero (J3) es mano
        var vuelta = new Vuelta2v2();

        var abre = TurnoServicio2v2.ObtenerAbreSiguienteVuelta(mano, vuelta, "Parda");

        Assert.Equal("J3", abre);
    }

    [Fact]
    public void ObtenerAbreSiguienteVuelta_GanoEquipoA_AbreElDeLaCartaMasAlta()
    {
        // Si gana un equipo, abre el jugador de ese equipo que tiró la carta más alta.
        var mano = CrearMano("J1");
        var vuelta = new Vuelta2v2();
        vuelta.CartasJugadas["J1"] = new Carta { Numero = 4, Palo = "Oro",   ValorTruco = 5 };
        vuelta.CartasJugadas["J3"] = new Carta { Numero = 3, Palo = "Espada", ValorTruco = 10 };

        var abre = TurnoServicio2v2.ObtenerAbreSiguienteVuelta(mano, vuelta, "EquipoA");

        Assert.Equal("J3", abre); // J3 jugó la más alta del equipo
    }

    // ── ObtenerOrdenDeclaracionEnvido ─────────────────────────────────

    [Fact]
    public void ObtenerOrdenDeclaracionEnvido_ManoJ1_EmpiezaPorElMano()
    {
        // Do - El conteo arranca por el mano (J1) y sigue el orden de la mesa.
        var mano = CrearMano("J1");

        // To
        var orden = TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano);

        // Where
        Assert.Equal(new[] { "J1", "J2", "J3", "J4" }, orden);
    }

    [Fact]
    public void ObtenerOrdenDeclaracionEnvido_ManoJ2_EmpiezaPorElMano()
    {
        // Do - J2 es mano → el conteo arranca por J2.
        var mano = CrearMano("J2");
        mano.EquipoMano = "EquipoB";

        // To
        var orden = TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano);

        // Where
        Assert.Equal(new[] { "J2", "J3", "J4", "J1" }, orden);
    }

    // ── ObtenerResponsableTruco ───────────────────────────────────────

    [Fact]
    public void ObtenerResponsableTruco_EquipoACanto_ManoJ1_PrimerDelRivalEsJ2()
    {
        // Do - EquipoA cantó. Orden J1→J2→J3→J4. Primer jugador del EquipoB = J2.
        var mano = CrearMano("J1");

        // To
        var responsable = TurnoServicio2v2.ObtenerResponsableTruco(mano, "EquipoA");

        // Where
        Assert.Equal("J2", responsable);
    }

    [Fact]
    public void ObtenerResponsableTruco_EquipoBCanto_ManoJ1_PrimerDelRivalEsJ1()
    {
        // Do - EquipoB cantó. Orden J1→J2→J3→J4. Primer jugador del EquipoA = J1.
        var mano = CrearMano("J1");

        // To
        var responsable = TurnoServicio2v2.ObtenerResponsableTruco(mano, "EquipoB");

        // Where
        Assert.Equal("J1", responsable);
    }
}
