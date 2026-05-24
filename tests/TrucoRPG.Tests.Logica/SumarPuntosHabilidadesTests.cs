using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class SumarPuntosHabilidadesTests
{
    private static ManoTruco ManoConHeroe(ClaseHeroe hero) => new()
    {
        Configuracion = new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = hero
        }
    };

    [Fact]
    public void SumarPuntos_ModoTradicional_NoDisparaModificadores()
    {
        var mano = new ManoTruco
        {
            Configuracion = new ConfiguracionPartida { Modo = ModoJuego.Tradicional }
        };

        JuegoServicio.SumarPuntos(mano, IdJugador.Humano, 2);

        Assert.Equal(2, mano.PuntosHumano);
    }

    [Fact]
    public void SumarPuntos_GanadorInvalido_NoModificaPuntos()
    {
        var mano = new ManoTruco();

        JuegoServicio.SumarPuntos(mano, "Parda", 5);

        Assert.Equal(0, mano.PuntosHumano);
        Assert.Equal(0, mano.PuntosMaquina);
    }

    [Fact]
    public void SumarPuntos_DetectaFinPartidaConModificador()
    {
        var mano = ManoConHeroe(ClaseHeroe.Manipulador);
        mano.PuntosHumano = 29;

        JuegoServicio.SumarPuntos(mano, IdJugador.Humano, 1);

        Assert.True(mano.PartidaTerminada);
        Assert.Equal(IdJugador.Humano, mano.GanadorPartida);
    }
}
