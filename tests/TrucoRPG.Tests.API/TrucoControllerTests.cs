using Microsoft.AspNetCore.Mvc;
using TrucoDemo.Controllers;
using TrucoDemo.Clases;
using TrucoDemo.Models;

namespace TrucoRPG.Tests.API;

public class TrucoControllerTests
{
    private readonly TrucoController _controller = new();

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
        var result = _controller.NuevaPartida();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);

        Assert.Equal(3, mano.Humano.Mano.Count);
    }

    [Fact]
    public void NuevaPartida_MaquinaRecibeTresCartas()
    {
        var result = _controller.NuevaPartida();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);

        Assert.Equal(3, mano.Maquina.Mano.Count);
    }

    [Fact]
    public void NuevaPartida_CartasDelHumanoYMaquinaSonDistintas()
    {
        var result = _controller.NuevaPartida();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);

        var todasLasCartas = mano.Humano.Mano.Concat(mano.Maquina.Mano).ToList();
        var distintas = todasLasCartas.Select(c => (c.Numero, c.Palo)).Distinct().Count();

        Assert.Equal(6, distintas);
    }

    [Fact]
    public void NuevaPartida_PuntosInicianEnCero()
    {
        var result = _controller.NuevaPartida();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);

        Assert.Equal(0, mano.PuntosHumano);
        Assert.Equal(0, mano.PuntosMaquina);
    }

    [Fact]
    public void NuevaPartida_EnvidoYTrucoNoEstanCantados()
    {
        var result = _controller.NuevaPartida();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);

        Assert.False(mano.EnvidoCantado);
        Assert.False(mano.TrucoCantado);
    }

    [Fact]
    public void JugarCarta_CartaInexistente_DevuelveBadRequest()
    {
        var partida = ObtenerManoNueva();

        var request = new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = 99,
            Palo = "Espada"
        };

        var result = _controller.JugarCarta(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void JugarCarta_CartaValida_DevuelveOkYReduceManoDe3A2()
    {
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano")
            return;

        var carta = partida.Humano.Mano.First();
        var request = new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = carta.Numero,
            Palo = carta.Palo
        };

        var result = _controller.JugarCarta(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var manoActualizada = Assert.IsType<ManoTruco>(ok.Value);

        Assert.Equal(2, manoActualizada.Humano.Mano.Count);
        Assert.Single(manoActualizada.Humano.Jugadas);
    }

    [Fact]
    public void CantarEnvido_DespuesDeJugarBaza_DevuelveBadRequest()
    {
        var partida = ObtenerManoNueva();
        if (partida.TurnoActual != "Humano") return;

        var carta = partida.Humano.Mano.First();
        _controller.JugarCarta(new JugarCartaRequest
        {
            ManoId = partida.Id,
            Numero = carta.Numero,
            Palo = carta.Palo
        });

        var result = _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = partida.Id });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void CantarEnvido_ManoIdInexistente_DevuelveNotFound()
    {
        var result = _controller.CantarEnvido(new CantarEnvidoRequest { ManoId = Guid.NewGuid() });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

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
        var mano = Assert.IsType<ManoTruco>(ok.Value);

        Assert.Equal(100, mano.NivelMentiraEnvidoMaquina);
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
        var mano = Assert.IsType<ManoTruco>(ok.Value);

        Assert.Equal(0, mano.NivelMentiraTrucoMaquina);
    }

    [Fact]
    public void IrseAlMazo_ManoValida_MaquinaGanaLaMano()
    {
        var partida = ObtenerManoNueva();
        if (partida.EnvidoPendienteRespuestaHumano || partida.TrucoPendienteRespuestaHumano)
            return;

        var result = _controller.IrseAlMazo(new CantarEnvidoRequest { ManoId = partida.Id });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mano = Assert.IsType<ManoTruco>(ok.Value);

        Assert.Equal("Maquina", mano.GanadorMano);
        Assert.True(mano.PuntosMaquina >= 1);
    }

    private ManoTruco ObtenerManoNueva()
    {
        var result = _controller.NuevaPartida();
        var ok = (OkObjectResult)result.Result!;
        return (ManoTruco)ok.Value!;
    }
}
