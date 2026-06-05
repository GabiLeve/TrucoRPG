using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica;

public class SonBuenas1v1Tests
{
    // ── Helpers ──────────────────────────────────────────────────────

    private static Carta C(int numero, string palo) =>
        new() { Numero = numero, Palo = palo, ValorTruco = 0 };

    private static ManoTruco CrearManoConEnvidoCantadoPorMaquina()
    {
        var mano = new ManoTruco
        {
            Id = Guid.NewGuid(),
            Humano = new Jugador
            {
                Id    = "Humano",
                Mano  = new List<Carta> { C(3, "Copa"), C(5, "Copa"), C(7, "Espada") }
            },
            Maquina = new Jugador
            {
                Id    = "Maquina",
                Mano  = new List<Carta> { C(1, "Espada"), C(7, "Basto"), C(6, "Basto") }
            },
            EnvidoCantado                = true,
            CantorEnvido                 = "Maquina",
            TipoEnvidoCantado            = "Envido",
            PuntosEnvido                 = 2,
            EnvidoPendienteRespuestaHumano = false, // ya aceptó el quiero
            EnvidoResuelto               = false
        };
        PartidaMemoriaServicio.Guardar(mano);
        return mano;
    }

    // ── Son Buenas básico ─────────────────────────────────────────────

    [Fact]
    public void EjecutarSonBuenas_MaquinaCanto_HumanoDeclaraSonBuenas_MaquinaGana()
    {
        // Do
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        var useCase = new ResponderEnvidoUseCase();

        // To
        var resultado = useCase.EjecutarSonBuenas(mano.Id);

        // Where
        Assert.Equal("Maquina", resultado.GanadorEnvido);
        Assert.True(resultado.EnvidoResuelto);
        Assert.True(resultado.SonBuenasDeclarado);
    }

    [Fact]
    public void EjecutarSonBuenas_MaquinaCanto_SumaLosPuntosAlGanador()
    {
        // Do
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        mano.PuntosEnvido = 2;
        PartidaMemoriaServicio.Actualizar(mano);
        var useCase = new ResponderEnvidoUseCase();

        // To
        var resultado = useCase.EjecutarSonBuenas(mano.Id);

        // Where
        Assert.Equal(2, resultado.PuntosMaquina);
    }

    [Fact]
    public void EjecutarSonBuenas_MaquinaCanto_FaseEnvidoEsResuelto()
    {
        // Do
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        var useCase = new ResponderEnvidoUseCase();

        // To
        var resultado = useCase.EjecutarSonBuenas(mano.Id);

        // Where
        Assert.Equal("resuelto", resultado.FaseEnvido);
    }

    [Fact]
    public void EjecutarSonBuenas_MaquinaCanto_EstadoMentionaSonBuenas()
    {
        // Do
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        var useCase = new ResponderEnvidoUseCase();

        // To
        var resultado = useCase.EjecutarSonBuenas(mano.Id);

        // Where
        Assert.NotNull(resultado.EstadoEnvido);
        Assert.Contains("buenas", resultado.EstadoEnvido!, StringComparison.OrdinalIgnoreCase);
    }

    // ── Son Buenas con RealEnvido ──────────────────────────────────────

    [Fact]
    public void EjecutarSonBuenas_ConRealEnvido_MaquinaGana3Puntos()
    {
        // Do
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        mano.TipoEnvidoCantado = "RealEnvido";
        mano.PuntosEnvido      = 3;
        PartidaMemoriaServicio.Actualizar(mano);
        var useCase = new ResponderEnvidoUseCase();

        // To
        var resultado = useCase.EjecutarSonBuenas(mano.Id);

        // Where
        Assert.Equal(3, resultado.PuntosEnvido);
        Assert.Equal(3, resultado.PuntosMaquina);
    }

    // ── Errores esperados ─────────────────────────────────────────────

    [Fact]
    public void EjecutarSonBuenas_PartidaTerminada_LanzaExcepcion()
    {
        // Do
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        mano.PartidaTerminada = true;
        PartidaMemoriaServicio.Actualizar(mano);
        var useCase = new ResponderEnvidoUseCase();

        // To & Where
        Assert.Throws<InvalidOperationException>(() => useCase.EjecutarSonBuenas(mano.Id));
    }

    [Fact]
    public void EjecutarSonBuenas_EnvidoNoExiste_LanzaExcepcion()
    {
        // Do
        var mano = new ManoTruco { Id = Guid.NewGuid(), EnvidoCantado = false };
        PartidaMemoriaServicio.Guardar(mano);
        var useCase = new ResponderEnvidoUseCase();

        // To & Where
        Assert.Throws<InvalidOperationException>(() => useCase.EjecutarSonBuenas(mano.Id));
    }

    [Fact]
    public void EjecutarSonBuenas_EnvidoYaResuelto_LanzaExcepcion()
    {
        // Do
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        mano.EnvidoResuelto = true;
        PartidaMemoriaServicio.Actualizar(mano);
        var useCase = new ResponderEnvidoUseCase();

        // To & Where
        Assert.Throws<InvalidOperationException>(() => useCase.EjecutarSonBuenas(mano.Id));
    }

    [Fact]
    public void EjecutarSonBuenas_HumanoCanto_NoSePuedeDeclararSonBuenas()
    {
        // Do - "Son buenas" solo aplica cuando la máquina cantó
        var mano = new ManoTruco
        {
            Id = Guid.NewGuid(),
            Humano  = new Jugador { Id = "Humano",  Mano = new List<Carta> { C(1, "Espada"), C(2, "Basto"), C(3, "Copa") } },
            Maquina = new Jugador { Id = "Maquina", Mano = new List<Carta> { C(4, "Espada"), C(5, "Basto"), C(6, "Copa") } },
            EnvidoCantado   = true,
            CantorEnvido    = "Humano",  // Humano cantó
            TipoEnvidoCantado = "Envido",
            PuntosEnvido    = 2,
            EnvidoResuelto  = false,
            EnvidoPendienteRespuestaHumano = false
        };
        PartidaMemoriaServicio.Guardar(mano);
        var useCase = new ResponderEnvidoUseCase();

        // To & Where
        Assert.Throws<InvalidOperationException>(() => useCase.EjecutarSonBuenas(mano.Id));
    }

    [Fact]
    public void EjecutarSonBuenas_EnvidoPendienteRespuesta_LanzaExcepcion()
    {
        // Do - El humano aún no aceptó el envido
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        mano.EnvidoPendienteRespuestaHumano = true; // todavía pendiente
        PartidaMemoriaServicio.Actualizar(mano);
        var useCase = new ResponderEnvidoUseCase();

        // To & Where
        Assert.Throws<InvalidOperationException>(() => useCase.EjecutarSonBuenas(mano.Id));
    }

    // ── Son Buenas no revela tanto del humano ─────────────────────────

    [Fact]
    public void EjecutarSonBuenas_NoRevelaElTantoDelHumano()
    {
        // Do
        var mano = CrearManoConEnvidoCantadoPorMaquina();
        var useCase = new ResponderEnvidoUseCase();

        // To
        var resultado = useCase.EjecutarSonBuenas(mano.Id);

        // Where - TantoHumano no debe ser calculado / revelado
        Assert.Null(resultado.TantoHumano);
    }

    // ── ID no existente ───────────────────────────────────────────────

    [Fact]
    public void EjecutarSonBuenas_ManoNoExiste_LanzaKeyNotFoundException()
    {
        // Do
        var useCase = new ResponderEnvidoUseCase();

        // To & Where
        Assert.Throws<KeyNotFoundException>(() => useCase.EjecutarSonBuenas(Guid.NewGuid()));
    }
}
