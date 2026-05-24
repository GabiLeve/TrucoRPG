using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica;

public class TimberoHabilidadTests
{
    [Fact]
    public void NuevaMano_TimberoCara_SumaUnPuntoAlHumano()
    {
        AzarServicio.MonedaCaraOverride = () => true;
        try
        {
            var mano = new NuevaManoUseCase().EjecutarNuevaPartida(new ConfiguracionPartida
            {
                Modo = ModoJuego.Historia,
                HeroeDelHumano = ClaseHeroe.Timbero
            });

            Assert.Equal(1, mano.PuntosHumano);
            Assert.Contains("Cara", mano.UltimoMensajeHabilidad ?? "");
        }
        finally
        {
            AzarServicio.MonedaCaraOverride = null;
        }
    }

    [Fact]
    public void NuevaMano_TimberoCruz_NoSumaPuntos()
    {
        AzarServicio.MonedaCaraOverride = () => false;
        try
        {
            var mano = new NuevaManoUseCase().EjecutarNuevaPartida(new ConfiguracionPartida
            {
                Modo = ModoJuego.Historia,
                HeroeDelHumano = ClaseHeroe.Timbero
            });

            Assert.Equal(0, mano.PuntosHumano);
            Assert.Contains("Cruz", mano.UltimoMensajeHabilidad ?? "");
        }
        finally
        {
            AzarServicio.MonedaCaraOverride = null;
        }
    }

    [Fact]
    public void SiguienteMano_TimberoCara_SumaOtroPunto()
    {
        AzarServicio.MonedaCaraOverride = () => true;
        try
        {
            var useCase = new NuevaManoUseCase();
            var primera = useCase.EjecutarNuevaPartida(new ConfiguracionPartida
            {
                Modo = ModoJuego.Historia,
                HeroeDelHumano = ClaseHeroe.Timbero
            });
            var segunda = useCase.Ejecutar(primera.Id);

            Assert.Equal(2, segunda.PuntosHumano);
            Assert.Equal(2, segunda.NumeroDeMano);
        }
        finally
        {
            AzarServicio.MonedaCaraOverride = null;
        }
    }

    [Fact]
    public void NuevaPartida_Tradicional_NoAplicaMonedaTimbero()
    {
        AzarServicio.MonedaCaraOverride = () => true;
        try
        {
            var mano = new NuevaManoUseCase().EjecutarNuevaPartida(new ConfiguracionPartida
            {
                Modo = ModoJuego.Tradicional,
                HeroeDelHumano = ClaseHeroe.Timbero
            });

            Assert.Equal(0, mano.PuntosHumano);
        }
        finally
        {
            AzarServicio.MonedaCaraOverride = null;
        }
    }
}
