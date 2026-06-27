using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica;

public class RivalPasivasHistoriaTests
{
    private static ConfiguracionPartida ConfigHistoria(ClaseRival rival) => new()
    {
        Modo = ModoJuego.Historia,
        HeroeDelHumano = ClaseHeroe.Mentiroso,
        RivalDeLaMaquina = rival,
        RivalNivel = 1
    };

    [Fact]
    public void Remolino_CuandoProbabilidadExitosa_CambiaPaloDeCartaEnMesa()
    {
        //Given
        AzarServicio.TirarProbabilidadOverride = _ => true;
        var mano = PartidaServicio.CrearManoNueva(configuracion: ConfigHistoria(ClaseRival.Nahuelito));
        var carta = new Carta { Numero = 7, Palo = "Espada", ValorTruco = 10 };
        var paloOriginal = carta.Palo;

        try
        {
            //When
            var activo = RemolinoServicio.IntentarEnPrimeraBaza(mano, carta);

            //Then
            Assert.True(activo);
            Assert.NotEqual(paloOriginal, carta.Palo);
            Assert.Contains("Remolino", mano.UltimoMensajeHabilidadRival);
        }
        finally
        {
            AzarServicio.TirarProbabilidadOverride = null;
        }
    }

    [Fact]
    public void Remolino_CuandoProbabilidadFalla_NoCambiaCarta()
    {
        //Given
        AzarServicio.TirarProbabilidadOverride = _ => false;
        var mano = PartidaServicio.CrearManoNueva(configuracion: ConfigHistoria(ClaseRival.Nahuelito));
        var carta = new Carta { Numero = 7, Palo = "Espada", ValorTruco = 10 };

        try
        {
            //When
            var activo = RemolinoServicio.IntentarEnPrimeraBaza(mano, carta);

            //Then
            Assert.False(activo);
            Assert.Equal("Espada", carta.Palo);
        }
        finally
        {
            AzarServicio.TirarProbabilidadOverride = null;
        }
    }

    [Fact]
    public void Pomberito_ManoSilenciosaHumanoGana_SumaUnPuntoExtraAMaquina()
    {
        //Given
        var mano = PartidaServicio.CrearManoNueva(configuracion: ConfigHistoria(ClaseRival.Pomberito));
        mano.GanadorMano = IdJugador.Humano;
        mano.EnvidoCantado = false;
        mano.TrucoCantado = false;

        //When
        JuegoServicio.SumarPuntos(mano, IdJugador.Humano, 1, OrigenPuntos.TrucoMano);

        //Then
        Assert.Equal(1, mano.PuntosHumano);
        Assert.Equal(1, mano.PuntosMaquina);
    }

    [Fact]
    public void Pomberito_ManoConTrucoCantado_NoSumaPuntoExtra()
    {
        //Given
        var mano = PartidaServicio.CrearManoNueva(configuracion: ConfigHistoria(ClaseRival.Pomberito));
        mano.GanadorMano = IdJugador.Humano;
        mano.TrucoCantado = true;

        //When
        JuegoServicio.SumarPuntos(mano, IdJugador.Humano, 2, OrigenPuntos.TrucoMano);

        //Then
        Assert.Equal(2, mano.PuntosHumano);
        Assert.Equal(0, mano.PuntosMaquina);
    }

    [Fact]
    public void LunaLlena_HumanoAceptaTrucoMaquina_DebilitaCartaUnaVezPorMano()
    {
        //Given
        var mano = PartidaServicio.CrearManoNueva(configuracion: new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Mentiroso,
            RivalDeLaMaquina = ClaseRival.Lobizon,
            RivalNivel = 3
        });
        mano.Humano.Mano =
        [
            new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
            new Carta { Numero = 3, Palo = "Copa", ValorTruco = 10 },
            new Carta { Numero = 4, Palo = "Oro", ValorTruco = 1 }
        ];
        var sumaAntes = mano.Humano.Mano.Sum(c => c.ValorTruco);

        //When
        LunaLlenaServicio.IntentarAlAceptarTrucoMaquina(mano, IdJugador.Maquina);
        LunaLlenaServicio.IntentarAlAceptarTrucoMaquina(mano, IdJugador.Maquina);

        //Then
        Assert.True(mano.LunaLlenaUsadaEnMano);
        Assert.Equal(sumaAntes - 1, mano.Humano.Mano.Sum(c => c.ValorTruco));
        Assert.Contains("Luna llena", mano.UltimoMensajeHabilidadRival);
    }

    [Fact]
    public void Aullido_PrimeraBazaHumanaYProbabilidadExitosa_BloqueaHastaConfirmar()
    {
        //Given
        AzarServicio.TirarProbabilidadOverride = _ => true;
        var mano = PartidaServicio.CrearManoNueva(configuracion: new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Mentiroso,
            RivalDeLaMaquina = ClaseRival.Lobizon,
            RivalNivel = 3
        });
        mano.Bazas.Add(new Baza
        {
            CartaJugador = new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
            CartaMaquina = new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 },
            Ganador = IdJugador.Humano
        });

        try
        {
            //When
            var activo = AullidoServicio.IntentarTrasPrimeraBaza(mano, IdJugador.Humano);

            //Then
            Assert.True(activo);
            Assert.True(mano.AullidoBloqueando);
        }
        finally
        {
            AzarServicio.TirarProbabilidadOverride = null;
        }
    }

    [Fact]
    public void ConfirmarAullido_FuerzaIrseAlMazo_MaquinaSumaPuntos()
    {
        //Given
        var mano = PartidaServicio.CrearManoNueva(configuracion: new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Mentiroso,
            RivalDeLaMaquina = ClaseRival.Lobizon,
            RivalNivel = 3
        });
        mano.AullidoBloqueando = true;
        mano.AullidoUsadoEnMano = true;
        PartidaMemoriaServicio.Guardar(mano);

        //When
        var resultado = new ConfirmarAullidoUseCase().Ejecutar(mano.Id);

        //Then
        Assert.False(resultado.AullidoBloqueando);
        Assert.Equal(IdJugador.Maquina, resultado.GanadorMano);
        Assert.Equal(1, resultado.PuntosMaquina);
    }
}
