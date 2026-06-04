using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica;

public class NuevaManoUseCaseHabilidadesTests
{
    [Fact]
    public void NuevaPartida_HistoriaConHeroe_ConservaConfiguracionEnSiguienteMano()
    {
        var useCase = new NuevaManoUseCase();
        var primera = useCase.EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Mentiroso
        });

        var segunda = useCase.Ejecutar(primera.Id);

        Assert.Equal(ModoJuego.Historia, segunda.Configuracion.Modo);
        Assert.Equal(ClaseHeroe.Mentiroso, segunda.Configuracion.HeroeDelHumano);
        Assert.True(segunda.Configuracion.HabilidadesActivas);
        Assert.Equal(2, segunda.NumeroDeMano);
    }

    [Fact]
    public void NuevaPartida_Tradicional_SinHabilidadesAunqueHayaHeroe()
    {
        var useCase = new NuevaManoUseCase();

        var mano = useCase.EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Tradicional,
            HeroeDelHumano = ClaseHeroe.Fanfarron
        });

        Assert.False(mano.Configuracion.HabilidadesActivas);
        Assert.False(mano.VistaHabilidadesHumano!.HabilidadesActivasEnPartida);
    }
}
