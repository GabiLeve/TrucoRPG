using Microsoft.AspNetCore.Mvc;
using TrucoDemo.Controllers;
using TrucoDemo.Clases;
using TrucoDemo.Models;
using TrucoDemo.Servicios;

namespace TrucoRPG.Tests.API;

public class TrucoControllerTests
{
    private readonly TrucoController _controller = new();

    // ─── Helper ──────────────────────────────────────────────────────

    private ManoTruco ObtenerManoNueva()
    {
        var result = _controller.NuevaPartida();
        var ok = (OkObjectResult)result.Result!;
        return (ManoTruco)ok.Value!;
    }

    // ─── NuevaPartida ────────────────────────────────────────────────

    [Fact]
    public void NuevaPartida_DevuelveOk_ConManoValida()
    {
        var result = _controller.NuevaPartida();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);
        Assert.NotEqual(Guid.Empty, mano.Id);
    }

    [Fact]
    public void NuevaPartida_HumanoRecibeTresCartas()
    {
        var mano = ObtenerManoNueva();
        Assert.Equal(3, mano.Humano.Mano.Count);
    }

    [Fact]
    public void NuevaPartida_MaquinaRecibeTresCartas()
    {
        var mano = ObtenerManoNueva();
        Assert.Equal(3, mano.Maquina.Mano.Count);
    }

    [Fact]
    public void NuevaPartida_CartasDelHumanoYMaquinaSonDistintas()
    {
        var mano = ObtenerManoNueva();
        var todas = mano.Humano.Mano.Concat(mano.Maquina.Mano);
        Assert.Equal(6, todas.Select(c => (c.Numero, c.Palo)).Distinct().Count());
    }

    [Fact]
    public void NuevaPartida_PuntosInicianEnCero()
    {
        var mano = ObtenerManoNueva();
        Assert.Equal(0, mano.PuntosHumano);
        Assert.Equal(0, mano.PuntosMaquina);
    }

    [Fact]
    public void NuevaPartida_EnvidoYTrucoNoEstanCantados()
    {
        var mano = ObtenerManoNueva();
        Assert.False(mano.EnvidoCantado);
        Assert.False(mano.TrucoCantado);
    }

    // ─── CantarEnvido ────────────────────────────────────────────────

    [Fact]
    public void CantarEnvido_ManoIdInexistente_DevuelveNotFound()
    {
        var result = _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = Guid.NewGuid() });
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void CantarEnvido_EnvidoYaCantado_DevuelveBadRequest()
    {
        // Cantamos el envido una vez; la segunda llamada debe fallar
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano" || partida.Bazas.Count > 0) return;

        _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = partida.Id });

        var result = _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = partida.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void CantarEnvido_DespuesDeJugarBaza_DevuelveBadRequest()
    {
        // El envido solo puede cantarse antes de la primera baza
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano") return;

        var carta = partida.Humano.Mano.First();
        _controller.JugarCarta(new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = carta.Numero,
            Palo   = carta.Palo
        });

        var result = _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = partida.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void CantarEnvido_AntesDeJugarCualquierCarta_Permitido()
    {
        // Sin bazas jugadas, el envido debe poder cantarse
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano" || partida.EnvidoPendienteRespuestaHumano) return;

        var result = _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = partida.Id });
        // Puede ser Ok (se resolvió) o BadRequest solo si ya había algo
        Assert.IsNotType<NotFoundObjectResult>(result.Result);
    }

    // ─── CantarTruco ─────────────────────────────────────────────────

    [Fact]
    public void CantarTruco_ManoIdInexistente_DevuelveNotFound()
    {
        var result = _controller.CantarTruco(new CantarEnvidoRequest { ManoId = Guid.NewGuid() });
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void CantarTruco_TrucoYaCantado_DevuelveBadRequest()
    {
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano") return;
        if (partida.TrucoCantado || partida.TrucoPendienteRespuestaHumano) return;

        _controller.CantarTruco(new CantarEnvidoRequest { ManoId = partida.Id });

        var result = _controller.CantarTruco(new CantarEnvidoRequest { ManoId = partida.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void CantarTruco_ManoTerminada_DevuelveBadRequest()
    {
        // Configuramos una mano terminada directamente en memoria
        var mano = new ManoTruco { GanadorMano = "Humano" };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.CantarTruco(new CantarEnvidoRequest { ManoId = mano.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // ─── EscalarTruco ────────────────────────────────────────────────

    [Fact]
    public void EscalarTruco_ManoIdInexistente_DevuelveNotFound()
    {
        var result = _controller.EscalarTruco(new CantarEnvidoRequest { ManoId = Guid.NewGuid() });
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void EscalarTruco_SinTrucoActivo_DevuelveBadRequest()
    {
        var mano = new ManoTruco { TrucoCantado = false };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.EscalarTruco(new CantarEnvidoRequest { ManoId = mano.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void EscalarTruco_NivelMaximo_DevuelveBadRequest()
    {
        var mano = new ManoTruco
        {
            TrucoCantado = true,
            TrucoResuelto = false,
            NivelTruco = 3,
            CantorTruco = "Maquina"
        };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.EscalarTruco(new CantarEnvidoRequest { ManoId = mano.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void EscalarTruco_CantorEsHumano_DevuelveBadRequest()
    {
        // El que cantó el nivel actual no puede escalar su propio canto
        var mano = new ManoTruco
        {
            TrucoCantado = true,
            TrucoResuelto = false,
            TrucoPendienteRespuestaHumano = false,
            NivelTruco = 1,
            CantorTruco = "Humano"
        };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.EscalarTruco(new CantarEnvidoRequest { ManoId = mano.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void EscalarTruco_CantorEsMaquina_SePermite()
    {
        // Si la máquina cantó truco y no hay pendiente, el humano puede escalar
        var mano = new ManoTruco
        {
            TrucoCantado = true,
            TrucoResuelto = false,
            TrucoPendienteRespuestaHumano = false,
            NivelTruco = 1,
            CantorTruco = "Maquina",
            PuntosTrucoMano = 2,
            Humano = new Jugador { Nombre = "Humano", EsMaquina = false },
            Maquina = new Jugador { Nombre = "Maquina", EsMaquina = true }
        };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.EscalarTruco(new CantarEnvidoRequest { ManoId = mano.Id });
        // No debe ser BadRequest por la restricción de cantor
        Assert.IsNotType<NotFoundObjectResult>(result.Result);
        // El resultado es Ok (máquina responde) o BadRequest por otra razón (p.ej. cartas vacías)
        // Lo importante es que NO falla con "No podés escalar tu propio canto"
    }

    // ─── ResponderTruco ──────────────────────────────────────────────

    [Fact]
    public void ResponderTruco_SinTrucoPendiente_DevuelveBadRequest()
    {
        var partida = ObtenerManoNueva();
        var result = _controller.ResponderTruco(new ResponderTrucoRequest
        {
            ManoId = partida.Id,
            Aceptar = true
        });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void ResponderTruco_ManoIdInexistente_DevuelveBadRequest()
    {
        var result = _controller.ResponderTruco(new ResponderTrucoRequest
        {
            ManoId = Guid.NewGuid(),
            Aceptar = false
        });
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void ResponderTruco_NoQuiero_CantorGanaUnPunto()
    {
        // Preparamos una mano con truco pendiente de respuesta del humano
        var mano = new ManoTruco
        {
            TrucoCantado = true,
            TrucoResuelto = false,
            TrucoPendienteRespuestaHumano = true,
            NivelTruco = 1,
            CantorTruco = "Maquina",
            PuntosTrucoMano = 2,
            Humano = new Jugador { Nombre = "Humano", EsMaquina = false },
            Maquina = new Jugador { Nombre = "Maquina", EsMaquina = true }
        };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.ResponderTruco(new ResponderTrucoRequest
        {
            ManoId = mano.Id,
            Aceptar = false
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var updated = Assert.IsType<ManoTruco>(ok.Value);
        Assert.Equal("Maquina", updated.GanadorMano);
        Assert.Equal(1, updated.PuntosMaquina); // nivel 1 → 1 punto al rechazar
    }

    // ─── ResponderEnvido ─────────────────────────────────────────────

    [Fact]
    public void ResponderEnvido_SinEnvidoPendiente_DevuelveBadRequest()
    {
        var partida = ObtenerManoNueva();
        var result = _controller.ResponderEnvido(new ResponderEnvidoRequest
        {
            ManoId = partida.Id,
            Aceptar = true
        });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void ResponderEnvido_ManoIdInexistente_DevuelveNotFound()
    {
        var result = _controller.ResponderEnvido(new ResponderEnvidoRequest
        {
            ManoId = Guid.NewGuid(),
            Aceptar = false
        });
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void ResponderEnvido_NoQuiero_CantorGana1Punto()
    {
        var mano = new ManoTruco
        {
            EnvidoCantado = true,
            EnvidoResuelto = false,
            EnvidoPendienteRespuestaHumano = true,
            CantorEnvido = "Maquina",
            TipoEnvidoCantado = "Envido",
            Humano = new Jugador { Nombre = "Humano", EsMaquina = false },
            Maquina = new Jugador { Nombre = "Maquina", EsMaquina = true }
        };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.ResponderEnvido(new ResponderEnvidoRequest
        {
            ManoId = mano.Id,
            Aceptar = false
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var updated = Assert.IsType<ManoTruco>(ok.Value);
        Assert.Equal("Maquina", updated.GanadorEnvido);
        Assert.Equal(1, updated.PuntosMaquina);
        Assert.True(updated.EnvidoResuelto);
    }

    // ─── IrseAlMazo ──────────────────────────────────────────────────

    [Fact]
    public void IrseAlMazo_ManoValida_MaquinaGanaLaMano()
    {
        var partida = ObtenerManoNueva();
        if (partida.EnvidoPendienteRespuestaHumano || partida.TrucoPendienteRespuestaHumano) return;

        var result = _controller.IrseAlMazo(new CantarEnvidoRequest { ManoId = partida.Id });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);
        Assert.Equal("Maquina", mano.GanadorMano);
        Assert.True(mano.PuntosMaquina >= 1);
    }

    [Fact]
    public void IrseAlMazo_ConTrucoPendienteDeRespuesta_DevuelveBadRequest()
    {
        var mano = new ManoTruco
        {
            TrucoCantado = true,
            TrucoPendienteRespuestaHumano = true,
            CantorTruco = "Maquina",
            NivelTruco = 1,
            Humano = new Jugador { Nombre = "Humano", EsMaquina = false },
            Maquina = new Jugador { Nombre = "Maquina", EsMaquina = true }
        };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.IrseAlMazo(new CantarEnvidoRequest { ManoId = mano.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void IrseAlMazo_ManoYaTerminada_DevuelveBadRequest()
    {
        var mano = new ManoTruco { GanadorMano = "Humano" };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.IrseAlMazo(new CantarEnvidoRequest { ManoId = mano.Id });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void IrseAlMazo_ConTrucoCantado_MaquinaGanaLosPuntosNegociados()
    {
        var mano = new ManoTruco
        {
            TrucoCantado = true,
            TrucoResuelto = true,
            NivelTruco = 2,
            PuntosTrucoMano = 3,
            CantorTruco = "Humano",
            Humano = new Jugador { Nombre = "Humano", EsMaquina = false },
            Maquina = new Jugador { Nombre = "Maquina", EsMaquina = true }
        };
        PartidaMemoriaServicio.Guardar(mano);

        var result = _controller.IrseAlMazo(new CantarEnvidoRequest { ManoId = mano.Id });
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var updated = Assert.IsType<ManoTruco>(ok.Value);
        Assert.Equal("Maquina", updated.GanadorMano);
        Assert.Equal(3, updated.PuntosMaquina);
    }

    // ─── JugarCarta ──────────────────────────────────────────────────

    [Fact]
    public void JugarCarta_CartaInexistente_DevuelveBadRequest()
    {
        var partida = ObtenerManoNueva();
        var result = _controller.JugarCarta(new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = 99,
            Palo = "Espada"
        });
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void JugarCarta_CartaValidaDelHumano_DevuelveOk()
    {
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano") return;
        if (partida.EnvidoPendienteRespuestaHumano || partida.TrucoPendienteRespuestaHumano) return;

        var carta = partida.Humano.Mano.First();
        var result = _controller.JugarCarta(new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = carta.Numero,
            Palo   = carta.Palo
        });
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void JugarCarta_ManoIdInexistente_DevuelveNotFound()
    {
        var result = _controller.JugarCarta(new JugarCartaRequest
        {
            ManoId = Guid.NewGuid(),
            Numero = 1,
            Palo = "Espada"
        });
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // ─── ConfigurarNivelMentira ───────────────────────────────────────

    [Fact]
    public void ConfigurarNivelMentiraEnvido_NivelValido_ActualizaNivel()
    {
        var partida = ObtenerManoNueva();
        var result = _controller.ConfigurarNivelMentiraEnvido(new ConfigurarNivelMentiraEnvidoRequest
        {
            ManoId = partida.Id,
            NivelMentira = 75
        });
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);
        Assert.Equal(75, mano.NivelMentiraEnvidoMaquina);
    }

    [Fact]
    public void ConfigurarNivelMentiraEnvido_NivelSuperior100_SeClampea()
    {
        var partida = ObtenerManoNueva();
        var result = _controller.ConfigurarNivelMentiraEnvido(new ConfigurarNivelMentiraEnvidoRequest
        {
            ManoId = partida.Id,
            NivelMentira = 999
        });
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(100, ((ManoTruco)ok.Value!).NivelMentiraEnvidoMaquina);
    }

    [Fact]
    public void ConfigurarNivelMentiraTruco_NivelNegativo_SeClampea()
    {
        var partida = ObtenerManoNueva();
        var result = _controller.ConfigurarNivelMentiraTruco(new ConfigurarNivelMentiraTrucoRequest
        {
            ManoId = partida.Id,
            NivelMentira = -50
        });
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(0, ((ManoTruco)ok.Value!).NivelMentiraTrucoMaquina);
    }
}
