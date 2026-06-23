using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica;

public class HabilidadesActivasTests
{
    [Fact]
    public void Activar_Mentiroso_RevelaCartaRival()
    {
        var useCase = new NuevaManoUseCase();
        var activar = new ActivarHabilidadUseCase();
        var mano = useCase.EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Mentiroso
        });

        var actualizada = activar.Ejecutar(mano.Id);

        Assert.NotNull(actualizada.VistaHabilidadesHumano?.CartaReveladaRival);
        Assert.False(actualizada.VistaHabilidadesHumano!.ActivaDisponible);
        Assert.Contains("revelaste", actualizada.UltimoMensajeHabilidad ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("(Truco", actualizada.UltimoMensajeHabilidad ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Activar_Fanfarron_MarcaBonusPendiente()
    {
        var mano = new NuevaManoUseCase().EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Fanfarron
        });
        var actualizada = new ActivarHabilidadUseCase().Ejecutar(mano.Id);

        var estado = actualizada.EstadoHabilidades.Obtener(IdJugador.Humano);

        Assert.True(estado!.FanfarronBonusPendiente);
    }

    [Fact]
    public void Activar_Manipulador_CambiaCarta()
    {
        var useCase = new NuevaManoUseCase();
        var mano = useCase.EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Manipulador
        });

        var carta = mano.Humano.Mano[0];
        var actualizada = new ActivarHabilidadUseCase().Ejecutar(mano.Id, carta.Numero, carta.Palo);

        Assert.Equal(3, actualizada.Humano.Mano.Count);
        Assert.Contains("Manipulador", actualizada.UltimoMensajeHabilidad ?? "");
    }

    [Fact]
    public void Activar_Timbero_MarcaApuestaActiva()
    {
        var mano = new NuevaManoUseCase().EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Timbero
        });

        var actualizada = new ActivarHabilidadUseCase().Ejecutar(mano.Id);
        var estado = actualizada.EstadoHabilidades.Obtener(IdJugador.Humano);

        Assert.True(estado!.TimberoApuestaActiva);
    }

    [Fact]
    public void Manipulador_ActivaNoDisponibleEnManoDos()
    {
        var useCase = new NuevaManoUseCase();
        var primera = useCase.EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Manipulador
        });

        var segunda = useCase.Ejecutar(primera.Id);

        Assert.False(segunda.VistaHabilidadesHumano!.ActivaDisponible);
    }
}
