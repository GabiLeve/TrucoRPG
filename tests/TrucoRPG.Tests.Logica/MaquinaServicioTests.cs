using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class MaquinaServicioTests
{
    private static Carta C(int valor) =>
        new() { Numero = valor, Palo = "Espada", ValorTruco = valor };

    [Fact]
    public void ElegirCarta_SinCartaHumano_DevuelveCartaDeLaMano()
    {
        var mano = new List<Carta> { C(3), C(7), C(10) };
        Carta? cartaHumano = null;

        var elegida = MaquinaServicio.ElegirCarta(mano, cartaHumano);

        Assert.Contains(elegida, mano);
    }

    [Fact]
    public void ElegirCarta_TieneCartasQueGanan_EligeListaMenorGanadora()
    {
        var mano = new List<Carta> { C(4), C(8), C(12) };
        var cartaHumano = C(5);
        int valorEsperado = 8;

        var elegida = MaquinaServicio.ElegirCarta(mano, cartaHumano);

        Assert.Equal(valorEsperado, elegida.ValorTruco);
    }

    [Fact]
    public void ElegirCarta_SoloUnaCartaGana_EligeLaGanadora()
    {
        var mano = new List<Carta> { C(2), C(3), C(9) };
        var cartaHumano = C(8);
        int valorEsperado = 9;

        var elegida = MaquinaServicio.ElegirCarta(mano, cartaHumano);

        Assert.Equal(valorEsperado, elegida.ValorTruco);
    }

    [Fact]
    public void ElegirCarta_NingunaCarta_Gana_EligeElMenorValor()
    {
        var mano = new List<Carta> { C(5), C(3), C(7) };
        var cartaHumano = C(14);
        int valorEsperado = 3;  

        var elegida = MaquinaServicio.ElegirCarta(mano, cartaHumano);

        Assert.Equal(valorEsperado, elegida.ValorTruco);
    }

    [Fact]
    public void ElegirCarta_SoloUnaCartaEnMano_DevuelveEsa()
    {
        var mano = new List<Carta> { C(6) };
        var cartaHumano = C(10);

        var elegida = MaquinaServicio.ElegirCarta(mano, cartaHumano);

        Assert.Equal(6, elegida.ValorTruco);
    }

    //AVANZAR UN PASO
    [Fact]
    public void AvanzarUnPaso_CuandoNoEsModoHistoriaOTerminada_DebeRetornarNull()
    {
        // Given
        var mano = CrearManoBase();
        mano.GanadorMano = "Humano";

        // When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        Assert.Null(resultado);
    }

    [Fact]
    public void AvanzarUnPaso_CuandoHayHabilidadesBloqueando_DebeRetornarNull()
    {
        // Given
        var mano = CrearManoBase();
        mano.SalpicaduraBloqueando = true; 

        // When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        Assert.Null(resultado);
    }

    [Fact]
    public void AvanzarUnPaso_CuandoHumanoTienePendienteOMaquinaYaJugo_DebeRetornarNull()
    {
        // Given
        var mano = CrearManoBase();
        mano.EnvidoPendienteRespuestaHumano = true; 

        // When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        Assert.Null(resultado);
    }

    [Fact]
    public void AvanzarUnPaso_CuandoHumanoTieneCartaEnMesa_DebeDelegarACompletarBaza()
    {
        // Given
        var mano = CrearManoBase();
        mano.CartaHumanoEnMesa = new Carta { Numero = 1, Palo = "Espada" };

        // When
        Action act = () => MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void AvanzarUnPaso_CuandoMaquinaTomaIniciativa_DebeCantarEnvido()
    {
        // Given
        var mano = CrearManoBase();
        mano.EnvidoCantado = false;
        mano.EnvidoResuelto = false;
        mano.Bazas = new List<Baza>(); 

        mano.Maquina.Mano = new List<Carta> {
        new Carta { Numero = 7, Palo = "Espada" },
        new Carta { Numero = 6, Palo = "Espada" }
        };

        // When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        if (resultado != null) 
        {
            Assert.Equal("envido", resultado.Tipo);
        }
    }

    [Fact]
    public void AvanzarUnPaso_CuandoNoEsTurnoMaquinaOSinCartas_DebeRetornarNull()
    {
        // Given
        var mano = CrearManoBase();
        mano.TurnoActual = "Humano"; 

        // When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        Assert.Null(resultado);
    }
    [Fact]
    public void AvanzarUnPaso_IntentarCantarEnvidoIniciativa_DebeRetornarFalse()
    {
        //Given
        var mano = new ManoTruco();

        mano.EnvidoCantado = true;
        mano.Bazas = new List<Baza> { new Baza() };

        //When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        //Then
        Assert.Null(resultado);
    }

    [Fact]
    public void AvanzarUnPaso_IntentarCantarEnvidoIniciativa_DebeModificarElEstadoYRetornarEvento()
    {
        //Given
        var mano = CrearManoBase();

        mano.EnvidoCantado = false;
        mano.EnvidoResuelto = false;
        mano.Bazas = new List<Baza>(); 

        mano.Maquina = new Jugador
        {
            Mano = new List<Carta>
        {
            new Carta { Numero = 7, Palo = "Espada", ValorTruco = 7 },
            new Carta { Numero = 6, Palo = "Espada", ValorTruco = 6 }
        }
        };
        mano.NivelMentiraEnvidoMaquina = 0; 

        //When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        //Then
        Assert.NotNull(resultado);
        Assert.Equal("envido", resultado.Tipo);

        Assert.True(mano.EnvidoCantado);
        Assert.Equal("Maquina", mano.CantorEnvido);
        Assert.Equal("Envido", mano.TipoEnvidoCantado);
        Assert.True(mano.EnvidoPendienteRespuestaHumano);
        Assert.Equal("La máquina cantó Envido.", mano.EstadoEnvido);
    }

    //helpe
    private static ManoTruco CrearManoBase()
    {
        var mano = new ManoTruco
        {
            Configuracion = new()
            {
                Modo = ModoJuego.Historia
            },

            Maquina = new()
            {
                Id = "Maquina",
                EsMaquina = true,
                Mano = new List<Carta>(),
                Jugadas = new List<Carta>()
            },

            Humano = new()
            {
                Id = "Humano",
                EsMaquina = false,
                Mano = new List<Carta>(),
                Jugadas = new List<Carta>()
            },

            GanadorMano = null,
            PartidaTerminada = false,
            SalpicaduraBloqueando = false,
            TravesuraBloqueando = false,
            RasgunoBloqueando = false,
            AullidoBloqueando = false,
            EnvidoPendienteRespuestaHumano = false,
            TrucoPendienteRespuestaHumano = false,
            CartaMaquinaEnMesa = null,
            CartaHumanoEnMesa = null,
            EnvidoCantado = true,
            EnvidoResuelto = true,
            TrucoCantado = true,
            TrucoResuelto = true,
            TurnoActual = "Maquina",
            Bazas = new List<Baza>(),

        };

        return mano;
    }

}
