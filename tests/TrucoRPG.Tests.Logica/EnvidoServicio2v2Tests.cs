using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class EnvidoServicio2v2Tests
{
    // ── Helpers ──────────────────────────────────────────────────────

    private static Carta C(int numero, string palo) =>
        new() { Numero = numero, Palo = palo, ValorTruco = 0 };

    private static ManoTruco2v2 CrearMano(string jugadorMano = "J1")
    {
        var j1 = new Jugador { Id = "J1", Mano = new List<Carta>() };
        var j2 = new Jugador { Id = "J2", Mano = new List<Carta>() };
        var j3 = new Jugador { Id = "J3", Mano = new List<Carta>() };
        var j4 = new Jugador { Id = "J4", Mano = new List<Carta>() };

        return new ManoTruco2v2
        {
            Posicion1   = j1,
            Posicion2   = j2,
            Posicion3   = j3,
            Posicion4   = j4,
            JugadorMano = jugadorMano,
            TurnoActual = jugadorMano,
            EquipoMano  = jugadorMano is "J1" or "J3" ? "EquipoA" : "EquipoB",
            EquipoA = new Equipo2v2 { Id = "EquipoA", Jugador1 = j1, Jugador2 = j3 },
            EquipoB = new Equipo2v2 { Id = "EquipoB", Jugador1 = j2, Jugador2 = j4 },
        };
    }

    // ── CalcularTantoEquipo ───────────────────────────────────────────

    [Fact]
    public void CalcularTantoEquipo_UsaElMaximoDeLosDosjugadores()
    {
        // Do
        var mano = CrearMano();
        // J1: 7+6 espada = 33; J3: 1+2 basto = 23 → máximo = 33
        mano.EquipoA.Jugador1.Mano = new List<Carta> { C(7, "Espada"), C(6, "Espada"), C(1, "Basto") };
        mano.EquipoA.Jugador2.Mano = new List<Carta> { C(1, "Basto"), C(2, "Basto"), C(3, "Oro") };

        // To
        var tanto = EnvidoServicio2v2.CalcularTantoEquipo(mano.EquipoA);

        // Where
        Assert.Equal(33, tanto);
    }

    [Fact]
    public void CalcularTantoEquipo_AmbosConTantoBajo_UsaElMayor()
    {
        // Do
        var mano = CrearMano();
        // J1: 5+3 = 28; J3: 4+6 = 30
        mano.EquipoA.Jugador1.Mano = new List<Carta> { C(5, "Copa"), C(3, "Copa"), C(1, "Espada") };
        mano.EquipoA.Jugador2.Mano = new List<Carta> { C(4, "Basto"), C(6, "Basto"), C(7, "Espada") };

        // To
        var tanto = EnvidoServicio2v2.CalcularTantoEquipo(mano.EquipoA);

        // Where
        Assert.Equal(30, tanto); // 4+6+20=30
    }

    // ── IniciarDeclaracionTantos ──────────────────────────────────────

    [Fact]
    public void IniciarDeclaracionTantos_FaseEsDeclarandoTantos()
    {
        // Do
        var mano = CrearMano("J1");
        mano.EquipoA.Jugador1.Mano = new List<Carta> { C(1, "Espada"), C(2, "Espada"), C(3, "Basto") };
        mano.EquipoA.Jugador2.Mano = new List<Carta> { C(4, "Copa"), C(5, "Copa"), C(6, "Oro") };
        mano.EquipoB.Jugador1.Mano = new List<Carta> { C(1, "Oro"), C(2, "Oro"), C(3, "Copa") };
        mano.EquipoB.Jugador2.Mano = new List<Carta> { C(7, "Basto"), C(6, "Basto"), C(1, "Copa") };

        // To
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        // Where
        Assert.Equal("declarando_tantos", mano.FaseEnvido);
        Assert.Equal(0, mano.IndiceDeclaracionTanto);
    }

    [Fact]
    public void IniciarDeclaracionTantos_EmpiezaPorElMano()
    {
        // Do - ManoJ1 → el conteo arranca por el mano (J1). Orden: J1, J2, J3, J4.
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);

        // To
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        // Where
        Assert.Equal("J1", mano.EnvidoPendienteRespuestaDe);
    }

    // ── ProcesarDeclaracion ───────────────────────────────────────────

    [Fact]
    public void ProcesarDeclaracion_JugadorDeclaroTanto_AvanzaAlSiguiente()
    {
        // Do
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);
        // El mano (J1) declara primero

        // To
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 25, sonBuenas: false);

        // Where
        Assert.False(terminado);
        Assert.Equal("J2", mano.EnvidoPendienteRespuestaDe);
        Assert.Equal(25, mano.TantosDeclarados["J1"]);
    }

    [Fact]
    public void ProcesarDeclaracion_GanaElEquipoConMayorTantoDeclarado()
    {
        // Do - Orden: J1, J2, J3, J4. J2 (EquipoB) muestra el mejor (30) → gana B.
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 28, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 30, sonBuenas: false);
        // J4 (EquipoB) se saltea: su equipo ya va ganando con 30.
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J3", 25, sonBuenas: false);

        // Where
        Assert.True(terminado);
        Assert.True(mano.EnvidoResuelto);
        Assert.Equal("EquipoB", mano.GanadorEnvido);
    }

    [Fact]
    public void ProcesarDeclaracion_TantosIguales_GanaEquipoMano()
    {
        // Do - Empate → gana el equipo mano (EquipoA, J1 es mano). Orden: J1, J2, J3, J4.
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        // J1 (mano) muestra 28; J2 iguala con 28 pero no supera (el mano gana empates),
        // así que J3 (EquipoA, ya líder) se saltea y declara J4.
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 28, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 28, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J4", 28, sonBuenas: false);

        // Where - empate a 28 → gana EquipoA (mano)
        Assert.True(mano.EnvidoResuelto);
        Assert.Equal("EquipoA", mano.GanadorEnvido);
    }

    // ── Son Buenas en 2v2 ─────────────────────────────────────────────

    [Fact]
    public void ProcesarDeclaracion_ManoGana_RivalesConceden_GanaElMano()
    {
        // El mano (J1) muestra 30; los rivales conceden ("son buenas"). Gana EquipoA.
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 30, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", null, sonBuenas: true);
        // J3 (EquipoA, ya líder) se saltea → declara/concede J4.
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J4", null, sonBuenas: true);

        Assert.True(terminado);
        Assert.True(mano.SonBuenasDeclarado);
        Assert.Equal("EquipoA", mano.GanadorEnvido);
        Assert.Equal(2, mano.PuntosEquipoA);
    }

    [Fact]
    public void ProcesarDeclaracion_ManoGana_SumaLosPuntos()
    {
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 4; // EnvidoEnvido
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 30, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", null, sonBuenas: true);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J4", null, sonBuenas: true);

        Assert.Equal(4, mano.PuntosEquipoA);
    }

    [Fact]
    public void ProcesarDeclaracion_ManoConcede_PuedeGanarElRival()
    {
        // El mano (J1) concede; un rival muestra el mejor tanto → gana EquipoB.
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", null, sonBuenas: true);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 30, sonBuenas: false);
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J3", null, sonBuenas: true);

        Assert.True(terminado);
        Assert.Equal("EquipoB", mano.GanadorEnvido);
        Assert.Equal(2, mano.PuntosEquipoB);
    }

    [Fact]
    public void ProcesarDeclaracion_CompaneroConcede_SinTantoGanador_GanaElRival()
    {
        // El mano muestra poco (20), el rival supera (31) y el compañero concede → gana B.
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 20, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 31, sonBuenas: false);
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J3", null, sonBuenas: true);

        Assert.True(terminado);
        Assert.Equal("EquipoB", mano.GanadorEnvido);
        Assert.Equal(2, mano.PuntosEquipoB);
    }

    [Fact]
    public void ProcesarDeclaracion_ManoYaGana_CompaneroNoNecesitaCantar()
    {
        // Pedido del usuario: si el mano (J1) ya muestra un tanto ganador, su compañero
        // (J3) NO necesita cantar; tras J2 el pendiente pasa directo a J4.
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 33, sonBuenas: false);
        var siguePendiente = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 27, sonBuenas: false);

        // J3 (compañero) salteado → el pendiente es J4, no J3.
        Assert.False(siguePendiente);
        Assert.Equal("J4", mano.EnvidoPendienteRespuestaDe);

        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J4", 20, sonBuenas: false);
        Assert.True(terminado);
        Assert.Equal("EquipoA", mano.GanadorEnvido);
        Assert.Equal(2, mano.PuntosEquipoA);
    }

    [Fact]
    public void ProcesarDeclaracion_CompaneroYaGana_ElOtroNoNecesitaCantar()
    {
        // Pedido del usuario: "si mi compañero ya le gana, no hace falta que cante el otro".
        // Mano = J2 (EquipoB). Orden: J2, J3, J4, J1. J3 (EquipoA) muestra 31 → gana A
        // y J1 (su compañero) ni siquiera llega a cantar.
        var mano = CrearMano("J2");
        mano.EquipoMano = "EquipoB";
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 20, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J3", 31, sonBuenas: false);
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J4", 25, sonBuenas: false);

        Assert.True(terminado);
        Assert.Equal("EquipoA", mano.GanadorEnvido);
        Assert.Null(mano.EnvidoPendienteRespuestaDe); // J1 salteado
    }

    // ── ResolverNoQuiero ──────────────────────────────────────────────

    [Fact]
    public void ResolverNoQuiero_CantorJ1_GanaEquipoA()
    {
        // Do
        var mano = CrearMano("J1");
        mano.CantorEnvido = "J1";
        mano.PuntosEnvido = 1;

        // To
        EnvidoServicio2v2.ResolverNoQuiero(mano);

        // Where
        Assert.Equal("EquipoA", mano.GanadorEnvido);
        Assert.True(mano.EnvidoResuelto);
        Assert.Equal(1, mano.PuntosEquipoA);
    }

    [Fact]
    public void ResolverNoQuiero_CantorJ2_GanaEquipoB()
    {
        // Do
        var mano = CrearMano("J1");
        mano.CantorEnvido = "J2";
        mano.PuntosEnvido = 1;

        // To
        EnvidoServicio2v2.ResolverNoQuiero(mano);

        // Where
        Assert.Equal("EquipoB", mano.GanadorEnvido);
        Assert.Equal(1, mano.PuntosEquipoB);
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static void AsignarCartasBasicas(ManoTruco2v2 mano)
    {
        mano.EquipoA.Jugador1.Mano = new List<Carta> { C(1, "Espada"), C(2, "Basto"), C(3, "Copa") };
        mano.EquipoA.Jugador2.Mano = new List<Carta> { C(4, "Espada"), C(5, "Basto"), C(6, "Copa") };
        mano.EquipoB.Jugador1.Mano = new List<Carta> { C(7, "Copa"), C(1, "Basto"), C(2, "Espada") };
        mano.EquipoB.Jugador2.Mano = new List<Carta> { C(3, "Basto"), C(4, "Oro"), C(5, "Espada") };
    }
}
