using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class HabilidadesOrquestadorTests
{
    [Fact]
    public void Disparar_ModoTradicional_NoActivaHabilidades()
    {
        var mano = PartidaServicio.CrearManoNueva(configuracion: new ConfiguracionPartida
        {
            Modo = ModoJuego.Tradicional,
            HeroeDelHumano = ClaseHeroe.Fanfarron
        });

        HabilidadesOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

        Assert.False(mano.Configuracion.HabilidadesActivas);
        Assert.NotNull(mano.VistaHabilidadesHumano);
        Assert.False(mano.VistaHabilidadesHumano!.HabilidadesActivasEnPartida);
    }

    [Fact]
    public void Disparar_ModoHistoriaConHeroe_InicializaEstadoYVista()
    {
        var mano = PartidaServicio.CrearManoNueva(configuracion: new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Timbero
        });

        HabilidadesOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

        Assert.True(mano.Configuracion.HabilidadesActivas);
        var estado = mano.EstadoHabilidades.Obtener(IdJugador.Humano);
        Assert.NotNull(estado);
        Assert.Equal(ClaseHeroe.Timbero, estado!.ClaseHeroe);
        Assert.NotNull(mano.VistaHabilidadesHumano);
        Assert.Equal(ClaseHeroe.Timbero, mano.VistaHabilidadesHumano!.ClaseHeroe);
    }

    [Fact]
    public void Activar_ModoTradicional_DevuelveError()
    {
        var mano = PartidaServicio.CrearManoNueva();
        var resultado = HabilidadesOrquestador.Activar(mano, new SolicitudActivarHabilidad
        {
            IdJugador = IdJugador.Humano
        });

        Assert.False(resultado.Exito);
    }
}
