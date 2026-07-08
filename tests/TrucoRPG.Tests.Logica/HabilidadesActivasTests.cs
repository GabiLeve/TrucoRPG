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

        // Fijamos una carta baja para descartar: con una carta al azar del reparto
        // el test era intermitente (si tocaba una muy alta, el mazo no tiene
        // ninguna de valor igual o mayor y la habilidad falla).
        var cartaBaja = new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 };
        mano.Humano.Mano[0] = cartaBaja;

        var actualizada = new ActivarHabilidadUseCase().Ejecutar(mano.Id, cartaBaja.Numero, cartaBaja.Palo);

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
    public void Manipulador_ActivaSigueDisponibleEnManoDos_SiNoSeUso()
    {
        // Si el jugador no usa la activa, debe seguir disponible cada mano
        // (el cooldown recién empieza a contar al usarla).
        var useCase = new NuevaManoUseCase();
        var primera = useCase.EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Manipulador
        });

        var segunda = useCase.Ejecutar(primera.Id);

        Assert.True(segunda.VistaHabilidadesHumano!.ActivaDisponible);
    }

    [Fact]
    public void Manipulador_TrasUsar_NoDisponiblePorTresManos_LuegoVuelve()
    {
        var useCase = new NuevaManoUseCase();

        // La activa exige que quede en el mazo una carta de ValorTruco igual o
        // mayor a la descartada. Se descarta la más baja de la mano y, en el
        // caso raro de que ni así haya reemplazo (las cartas altas repartidas),
        // se reparte una partida nueva.
        ManoTruco mano1;
        Carta carta;
        do
        {
            mano1 = useCase.EjecutarNuevaPartida(new ConfiguracionPartida
            {
                Modo = ModoJuego.Historia,
                HeroeDelHumano = ClaseHeroe.Manipulador
            });
            carta = mano1.Humano.Mano.OrderBy(c => c.ValorTruco).First();
        }
        while (!mano1.CartasRestantesMazo.Any(c => c.ValorTruco >= carta.ValorTruco));

        // Usar la activa en la mano 1.
        new ActivarHabilidadUseCase().Ejecutar(mano1.Id, carta.Numero, carta.Palo);

        var mano2 = useCase.Ejecutar(mano1.Id);
        var mano3 = useCase.Ejecutar(mano2.Id);
        var mano4 = useCase.Ejecutar(mano3.Id);

        Assert.False(mano2.VistaHabilidadesHumano!.ActivaDisponible);
        Assert.False(mano3.VistaHabilidadesHumano!.ActivaDisponible);
        Assert.True(mano4.VistaHabilidadesHumano!.ActivaDisponible);
    }
}
