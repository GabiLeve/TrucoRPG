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
    public void IniciarDeclaracionTantos_RivalDeclaraPrimero_EquipoManoUltimo()
    {
        // Do - ManoJ1 → EquipoA es mano. Rival (B) primero, Mano (A) último.
        // Orden declaración: J2, J4, J1, J3
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);

        // To
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        // Where
        Assert.Equal("J2", mano.EnvidoPendienteRespuestaDe);
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
        // Ahora J2 debe declarar

        // To
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 25, sonBuenas: false);

        // Where
        Assert.False(terminado);
        Assert.Equal("J4", mano.EnvidoPendienteRespuestaDe);
        Assert.Equal(25, mano.TantosDeclarados["J2"]);
    }

    [Fact]
    public void ProcesarDeclaracion_TodosDeclararon_ResuelvePorTantos()
    {
        // Do
        var mano = CrearMano("J1");
        // EquipoA: J1=28, J3=25 → tanto equipo = 28
        // EquipoB: J2=30, J4=22 → tanto equipo = 30 → gana B
        mano.EquipoA.Jugador1.Mano = new List<Carta> { C(7, "Espada"), C(1, "Espada"), C(3, "Basto") };
        mano.EquipoA.Jugador2.Mano = new List<Carta> { C(5, "Oro"), C(1, "Oro"), C(3, "Copa") };
        mano.EquipoB.Jugador1.Mano = new List<Carta> { C(7, "Copa"), C(3, "Copa"), C(6, "Basto") };
        mano.EquipoB.Jugador2.Mano = new List<Carta> { C(2, "Espada"), C(1, "Basto"), C(4, "Oro") };
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        // Orden: J2, J4, J1, J3
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 30, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J4", 22, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 28, sonBuenas: false);
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J3", 25, sonBuenas: false);

        // Where
        Assert.True(terminado);
        Assert.True(mano.EnvidoResuelto);
        Assert.Equal("EquipoB", mano.GanadorEnvido);
    }

    [Fact]
    public void ProcesarDeclaracion_TantosIguales_GanaEquipoMano()
    {
        // Do - Empate → gana el equipo mano (EquipoA tiene J1 como mano)
        var mano = CrearMano("J1");
        // EquipoA tanto = 28, EquipoB tanto = 28 → empate → gana EquipoA (mano)
        mano.EquipoA.Jugador1.Mano = new List<Carta> { C(7, "Espada"), C(1, "Espada"), C(3, "Oro") };
        mano.EquipoA.Jugador2.Mano = new List<Carta> { C(3, "Copa"), C(1, "Copa"), C(5, "Basto") };
        mano.EquipoB.Jugador1.Mano = new List<Carta> { C(7, "Basto"), C(1, "Basto"), C(4, "Espada") };
        mano.EquipoB.Jugador2.Mano = new List<Carta> { C(5, "Basto"), C(2, "Basto"), C(4, "Copa") };
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        // Orden: J2, J4, J1, J3
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 28, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J4", 22, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", 28, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J3", 25, sonBuenas: false);

        // Where - empate a 28 → gana EquipoA (mano)
        Assert.True(mano.EnvidoResuelto);
        Assert.Equal("EquipoA", mano.GanadorEnvido);
    }

    // ── Son Buenas en 2v2 ─────────────────────────────────────────────

    [Fact]
    public void ProcesarDeclaracion_SonBuenas_ResuelveFavor_EquipoContrario()
    {
        // Do - J2 declara "son buenas" → EquipoB pierde → EquipoA gana
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);
        // Ahora es turno de J2

        // To
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", null, sonBuenas: true);

        // Where
        Assert.True(terminado);
        Assert.True(mano.SonBuenasDeclarado);
        Assert.Equal("J2", mano.JugadorQueDijoSonBuenas);
        Assert.Equal("EquipoA", mano.GanadorEnvido);
        Assert.True(mano.EnvidoResuelto);
    }

    [Fact]
    public void ProcesarDeclaracion_SonBuenas_SumaLosPuntos()
    {
        // Do
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 4; // EnvidoEnvido
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);

        // To
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", null, sonBuenas: true);

        // Where
        Assert.Equal(4, mano.PuntosEquipoA);
    }

    [Fact]
    public void ProcesarDeclaracion_SonBuenas_J1DeEquipoA_GanaEquipoB()
    {
        // Do - J1 es del EquipoA y dice son buenas → EquipoA pierde → EquipoB gana
        var mano = CrearMano("J1");
        AsignarCartasBasicas(mano);
        mano.PuntosEnvido = 2;
        EnvidoServicio2v2.IniciarDeclaracionTantos(mano);
        // Primero J2 declara (del rival)
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J2", 30, sonBuenas: false);
        EnvidoServicio2v2.ProcesarDeclaracion(mano, "J4", 25, sonBuenas: false);
        // Ahora J1 declara

        // To
        var terminado = EnvidoServicio2v2.ProcesarDeclaracion(mano, "J1", null, sonBuenas: true);

        // Where
        Assert.True(terminado);
        Assert.Equal("EquipoB", mano.GanadorEnvido);
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
