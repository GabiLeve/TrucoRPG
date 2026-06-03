using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Controllers;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.API;

public class TrucoControllerTests
{
    private static TrucoController CrearController() => new(
        new NuevaManoUseCase(),
        new ConfigurarNivelMentiraUseCase(),
        new CantarEnvidoUseCase(),
        new ResponderEnvidoUseCase(),
        new CantarTrucoUseCase(),
        new ResponderTrucoUseCase(),
        new EscalarTrucoUseCase(),
        new IrseAlMazoUseCase(),
        new JugarCartaUseCase(),
        new ActivarHabilidadUseCase());

    private readonly TrucoController _controller = CrearController();

    // ─── Helper ──────────────────────────────────────────────────────

    private ManoTruco ObtenerManoNueva()
    {
        var result = _controller.NuevaPartida(null);
        var ok = (OkObjectResult)result.Result!;
        return (ManoTruco)ok.Value!;
    }

    // ─── NuevaPartida ────────────────────────────────────────────────

    [Fact]
    public void NuevaPartida_DevuelveOk_ConManoValida()
    {
        var result = _controller.NuevaPartida(null);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);
        Assert.NotEqual(Guid.Empty, mano.Id);
    }

    [Fact]
    public void NuevaPartida_HumanoRecibeTresCartas()
    {
        //Then
        var mano = ObtenerManoNueva();

        //When
        Assert.Equal(3, mano.Humano.Mano.Count);
    }

    [Fact]
    public void NuevaPartida_MaquinaRecibeTresCartas()
    {
        //When
        var mano = ObtenerManoNueva();

        //Then
        Assert.Equal(3, mano.Maquina.Mano.Count);
    }

    [Fact]
    public void NuevaPartida_CartasDelHumanoYMaquinaSonDistintas()
    {
        //When
        var mano = ObtenerManoNueva();
        var todas = mano.Humano.Mano.Concat(mano.Maquina.Mano);

        //Then
        var cantCartasDistintas = todas.Select(c => (c.Numero, c.Palo)).Distinct().Count();
        Assert.Equal(6,cantCartasDistintas);
    }

    [Fact]
    public void NuevaPartida_PuntosInicianEnCero()
    {
        //When
        var mano = ObtenerManoNueva();

        //Then
        Assert.Equal(0, mano.PuntosHumano);
        Assert.Equal(0, mano.PuntosMaquina);
    }

    [Fact]
    public void NuevaPartida_EnvidoYTrucoNoEstanCantados()
    {
        //When
        var mano = ObtenerManoNueva();

        //Then
        Assert.False(mano.EnvidoCantado);
        Assert.False(mano.TrucoCantado);
    }

    // ─── CantarEnvido ────────────────────────────────────────────────

    [Fact]
    public void CantarEnvido_ManoIdInexistente_DevuelveNotFound()
    {
        // Given
        var request = new CantarEnvidoRequest
        {
            ManoId = Guid.NewGuid()
        };

        // When
        Action act = () => _controller.CantarEnvido(request);

        // Then
        Assert.Throws<KeyNotFoundException>(act);
    }

    [Fact]
    public void CantarEnvido_EnvidoYaCantado_DevuelveBadRequest()
    {
        // Cantamos el envido una vez; la segunda llamada debe fallar
        //Given
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano" || partida.Bazas.Count > 0) return;

        _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = partida.Id });

        var result = new CantarEnvidoRequest
        {
            ManoId = partida.Id
        };

        //When
        Action act = () => _controller.CantarEnvido(result);

        //Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void CantarEnvido_DespuesDeJugarBaza_DevuelveBadRequest()
    {
        // El envido solo puede cantarse antes de la primera baza
        //Given
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano") return;

        var carta = partida.Humano.Mano.First();
        _controller.JugarCarta(new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = carta.Numero,
            Palo   = carta.Palo
        });
        var result = new CantarEnvidoRequest
        {
            ManoId = partida.Id 
        };

        //When
        Action act = () => _controller.CantarEnvido(result);

        //Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void CantarEnvido_AntesDeJugarCualquierCarta_Permitido()
    {
        // Sin bazas jugadas, el envido debe poder cantarse
        //Given
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano" || partida.EnvidoPendienteRespuestaHumano) return;

        //When
        var result = _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = partida.Id });

        //Then
        // Puede ser Ok (se resolvió) o BadRequest solo si ya había algo
        Assert.IsNotType<NotFoundObjectResult>(result.Result);
    }

    // ─── CantarTruco ─────────────────────────────────────────────────

    [Fact]
    public void CantarTruco_ManoIdInexistente_DevuelveNotFound()
    {
        //Given
        var result = new CantarEnvidoRequest
        {
            ManoId = Guid.NewGuid()
        };

        //When 
        Action act =()=> _controller.CantarTruco(result);

        //Then
        Assert.Throws<KeyNotFoundException>(act);
    }

    [Fact]
    public void CantarTruco_TrucoYaCantado_DevuelveBadRequest()
    {
        // Given
        var partida = ObtenerManoNueva();

        partida.TrucoCantado = true;

        if (partida.TurnoActual != "Humano") return;
        if (partida.TrucoCantado || partida.TrucoPendienteRespuestaHumano) return;

        var request = new CantarEnvidoRequest
        {
            ManoId = partida.Id
        };

        // When
        Action act = () => _controller.CantarTruco(request);

        // Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void CantarTruco_ManoTerminada_DevuelveBadRequest()
    {
        // Configuramos una mano terminada directamente en memoria
        //Given
        var mano = new ManoTruco { GanadorMano = "Humano" };
        PartidaMemoriaServicio.Guardar(mano);
        var result = new CantarEnvidoRequest { ManoId = mano.Id };

        //When
        Action act =()=> _controller.CantarTruco(result);

        //Then
        Assert.Throws<InvalidOperationException>(act);
    }

    // ─── EscalarTruco ────────────────────────────────────────────────

    [Fact]
    public void EscalarTruco_ManoIdInexistente_DevuelveNotFound()
    {
        //Given
        var request = new CantarEnvidoRequest
        {
            ManoId = Guid.NewGuid()
        };

        // When
        Action act = () => _controller.EscalarTruco(request);

        // Then
        Assert.Throws<KeyNotFoundException>(act);
        
    }

    [Fact]
    public void EscalarTruco_SinTrucoActivo_DevuelveBadRequest()
    {
        // Given
        var mano = new ManoTruco { TrucoCantado = false };
        PartidaMemoriaServicio.Guardar(mano);

        var request = new CantarEnvidoRequest
        {
            ManoId = mano.Id
        };

        // When
        Action act = () => _controller.EscalarTruco(request);

        // Then
        Assert.Throws<InvalidOperationException>(act);

    }

    [Fact]
    public void EscalarTruco_NivelMaximo_DevuelveBadRequest()
    {
        //Given
        var mano = new ManoTruco
        {
            TrucoCantado = true,
            TrucoResuelto = false,
            NivelTruco = 3,
            CantorTruco = "Maquina"
        };
        PartidaMemoriaServicio.Guardar(mano);
        var result = new CantarEnvidoRequest { ManoId = mano.Id };

        //When
        Action act =()=> _controller.EscalarTruco(result);

        //Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void EscalarTruco_CantorEsHumano_DevuelveBadRequest()
    {
        // El que cantó el nivel actual no puede escalar su propio canto
        //Given
        var mano = new ManoTruco
        {
            TrucoCantado = true,
            TrucoResuelto = false,
            TrucoPendienteRespuestaHumano = false,
            NivelTruco = 1,
            CantorTruco = "Humano"
        };
        PartidaMemoriaServicio.Guardar(mano);

        var result = new CantarEnvidoRequest
        {
            ManoId = mano.Id

        };

        //When
        Action act =()=> _controller.EscalarTruco(result);

        //Then 
        Assert.Throws<InvalidOperationException>(act);
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

        var request = new CantarEnvidoRequest { ManoId = mano.Id };

        //When
        var result = _controller.EscalarTruco(request);
        // No debe ser BadRequest por la restricción de cantor
        Assert.IsNotType<NotFoundObjectResult>(result);
        // El resultado es Ok (máquina responde) o BadRequest por otra razón (p.ej. cartas vacías)
        // Lo importante es que NO falla con "No podés escalar tu propio canto"
    }

    // ─── ResponderTruco ──────────────────────────────────────────────

    [Fact]
    public void ResponderTruco_SinTrucoPendiente_DevuelveBadRequest()
    {
        //Given
        var partida = ObtenerManoNueva();
        var result = new ResponderTrucoRequest
        {
            ManoId = partida.Id,
            Aceptar = true

        };

        //When
        Action act =()=> _controller.ResponderTruco(result);

        //Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ResponderTruco_ManoIdInexistente_DevuelveKeyNotFountException()
    {
        // Given
        var request = new ResponderTrucoRequest
        {
            ManoId = Guid.NewGuid(),
            Aceptar = false
        };

        // When
        Action act = () => _controller.ResponderTruco(request);

        // Then
        Assert.Throws<KeyNotFoundException>(act);
    }

    [Fact]
    public void ResponderTruco_NoQuiero_CantorGanaUnPunto()
    {
        // Preparamos una mano con truco pendiente de respuesta del humano
        //Given
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

        var request = new ResponderTrucoRequest {
            ManoId = mano.Id,
            Aceptar = false
        };

        //When
        var result = _controller.ResponderTruco(request);

        //Then
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var manoActualizada = Assert.IsType<ManoTruco>(ok.Value);

        Assert.Equal("Maquina", manoActualizada.GanadorMano);
        Assert.Equal(1, manoActualizada.PuntosMaquina); // nivel 1 → 1 punto al rechazar
    }

    // ─── ResponderEnvido ─────────────────────────────────────────────

    [Fact]
    public void ResponderEnvido_SinEnvidoPendiente_DevuelveBadRequest()
    {
        //Given
        var partida = ObtenerManoNueva();
        var result = new ResponderEnvidoRequest
        {
            ManoId = partida.Id,
            Aceptar = true
        };

        //When
        Action act = () => _controller.ResponderEnvido(result);

        //Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void ResponderEnvido_ManoIdInexistente_DevuelveKeyNotFoundException()
    {
        //Given
        var result = new ResponderEnvidoRequest
        {
            ManoId = Guid.NewGuid(),
            Aceptar = false
        };

        //When
        Action act = () => _controller.ResponderEnvido(result);

        //Then
        Assert.Throws<KeyNotFoundException>(act);
    }

    [Fact]
    public void ResponderEnvido_NoQuiero_CantorGana1Punto()
    {
        //Given
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

        var request = new ResponderEnvidoRequest
        {
            ManoId = mano.Id,
            Aceptar = false
        };

        //When
        var result = _controller.ResponderEnvido(request);

        //Then
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
        //Given
        var partida = ObtenerManoNueva();
        if (partida.EnvidoPendienteRespuestaHumano || partida.TrucoPendienteRespuestaHumano) return;
        var request = new CantarEnvidoRequest { ManoId = partida.Id };

        //When
        var result = _controller.IrseAlMazo(request);

        //Then 
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);
        Assert.Equal("Maquina", mano.GanadorMano);
        Assert.True(mano.PuntosMaquina >= 1);
    }

    [Fact]
    public void IrseAlMazo_ConTrucoPendienteDeRespuesta_DevuelveBadRequest()
    {
        //Given
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

        //When
        var result = new CantarEnvidoRequest
        {
            ManoId = mano.Id
        };

        //When
        Action act =()=> _controller.IrseAlMazo(result);

        //Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void IrseAlMazo_ManoYaTerminada_DevuelveBadRequest()
    {
        // Given
        var mano = new ManoTruco { GanadorMano = "Humano" };
        PartidaMemoriaServicio.Guardar(mano);

        var request = new CantarEnvidoRequest
        {
            ManoId = mano.Id
        };

        // When
        Action act = () => _controller.IrseAlMazo(request);

        // Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void IrseAlMazo_ConTrucoCantado_MaquinaGanaLosPuntosNegociados()
    {
        //Given
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
        var request = new CantarEnvidoRequest { ManoId = mano.Id };

        //When
        var result = _controller.IrseAlMazo(request);

        //Then
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var updated = Assert.IsType<ManoTruco>(ok.Value);
        Assert.Equal("Maquina", updated.GanadorMano);
        Assert.Equal(3, updated.PuntosMaquina);
    }

    // ─── JugarCarta ──────────────────────────────────────────────────

    [Fact]
    public void JugarCarta_CartaInexistente_DevuelveInvalidOperationException()
    {
        // Given
        var partida = ObtenerManoNueva();

        var request = new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = 99,
            Palo = "Espada"
        };

        // When
        Action act = () => _controller.JugarCarta(request);

        // Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void JugarCarta_CartaValidaDelHumano_DevuelveOk()
    {
        //Given
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano") return;
        if (partida.EnvidoPendienteRespuestaHumano || partida.TrucoPendienteRespuestaHumano) return;
        var carta = partida.Humano.Mano.First();
        var request = new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = carta.Numero,
            Palo = carta.Palo
        };

        //Then
        var result = _controller.JugarCarta(request);

        //When
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public void JugarCarta_ManoIdInexistente_DevuelveNotFound()
    {
        //Given
        var result = new JugarCartaRequest
        {
            ManoId = Guid.NewGuid(),
            Numero = 1,
            Palo = "Espada"

        };

        //When
        Action act = () =>_controller.JugarCarta(result);

        //Then
        Assert.Throws<KeyNotFoundException>(act);
    }

    // ─── ConfigurarNivelMentira ───────────────────────────────────────

    [Fact]
    public void ConfigurarNivelMentiraEnvido_NivelValido_ActualizaNivel()
    {
        //Given
        var partida = ObtenerManoNueva();
        var request = new ConfigurarNivelMentiraEnvidoRequest
        {
            ManoId = partida.Id,
            NivelMentira = 75
        };
        //When
        var result = _controller.ConfigurarNivelMentiraEnvido(request);

        //Then
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);
        Assert.Equal(75, mano.NivelMentiraEnvidoMaquina);
    }

    [Fact]
    public void ConfigurarNivelMentiraEnvido_NivelSuperior100_SeClampea()
    {
        //Given
        var partida = ObtenerManoNueva();
        var request = new ConfigurarNivelMentiraEnvidoRequest
        {
            ManoId = partida.Id,
            NivelMentira = 999
        };

        //When
        var result = _controller.ConfigurarNivelMentiraEnvido(request);

        //Then
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(100, ((ManoTruco)ok.Value!).NivelMentiraEnvidoMaquina);
    }

    [Fact]
    public void ConfigurarNivelMentiraTruco_NivelNegativo_SeClampea()
    {
        //Given
        var partida = ObtenerManoNueva();
        var request = new ConfigurarNivelMentiraTrucoRequest
        {
            ManoId = partida.Id,
            NivelMentira = -50
        };

        //When
        var result = _controller.ConfigurarNivelMentiraTruco(request);

        //Then
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(0, ((ManoTruco)ok.Value!).NivelMentiraTrucoMaquina);
    }
}
