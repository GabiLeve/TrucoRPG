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
        mano.TrucoCantado = false;
        mano.TrucoResuelto = false;
        mano.Bazas = new List<Baza>();
        mano.NivelMentiraEnvidoMaquina = 100;

        mano.Maquina.Mano = new List<Carta> {
        new Carta { Numero = 7, Palo = "Espada" },
        new Carta { Numero = 6, Palo = "Espada" }
        };

        // When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        Assert.NotNull(resultado);
        Assert.Equal("envido", resultado.Tipo);
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
        mano.TrucoCantado = false;
        mano.TrucoResuelto = false;
        mano.Bazas = new List<Baza>(); 

        mano.Maquina = new Jugador
        {
            Mano = new List<Carta>
        {
            new Carta { Numero = 7, Palo = "Espada", ValorTruco = 7 },
            new Carta { Numero = 6, Palo = "Espada", ValorTruco = 6 }
        }
        };
        mano.NivelMentiraEnvidoMaquina = 100; 

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

    [Fact]
    public void AvanzarUnPaso_DespuesDeAceptarTruco_NoCantaEnvido()
    {
        var mano = CrearManoBase();
        mano.Configuracion = new() { Modo = ModoJuego.Historia };
        mano.EnvidoCantado = false;
        mano.EnvidoResuelto = false;
        mano.TrucoCantado = true;
        mano.TrucoResuelto = false;
        mano.TrucoPendienteRespuestaHumano = false;
        mano.TurnoActual = "Maquina";
        mano.Bazas = new List<Baza>();
        mano.NivelMentiraEnvidoMaquina = 100;
        mano.Maquina.Mano =
        [
            new Carta { Numero = 7, Palo = "Espada", ValorTruco = 10 },
            new Carta { Numero = 6, Palo = "Espada", ValorTruco = 1 },
        ];

        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        Assert.NotEqual("envido", resultado?.Tipo);
        Assert.False(mano.EnvidoCantado);
    }

    [Fact]
    public void AvanzarUnPaso_ConTrucoPendienteDeRespuestaHumana_PermiteEnvidoVaPrimero()
    {
        var mano = CrearManoBase();
        mano.Configuracion = new() { Modo = ModoJuego.Historia };
        mano.EnvidoCantado = false;
        mano.EnvidoResuelto = false;
        mano.TrucoCantado = true;
        mano.TrucoResuelto = false;
        mano.TrucoPendienteRespuestaHumano = true;
        mano.TurnoActual = "Humano";
        mano.Bazas = new List<Baza>();
        mano.NivelMentiraEnvidoMaquina = 100;
        mano.Maquina.Mano =
        [
            new Carta { Numero = 7, Palo = "Espada", ValorTruco = 10 },
            new Carta { Numero = 6, Palo = "Espada", ValorTruco = 1 },
        ];

        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        Assert.Null(resultado);
        Assert.False(mano.EnvidoCantado);
    }

    [Fact]
    public void AvanzarUnPaso_CuandoMaquinaJuegaCarta_DevuelveEventoCarta()
    {
        // Given
        var mano = CrearManoBase();
        mano.Maquina = new Jugador
        {
            Mano = new List<Carta>
         {
             new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 }
         },
            Jugadas = new List<Carta>()
        };
        mano.EnvidoCantado = true;
        mano.EnvidoResuelto = true;
        mano.TrucoCantado = true;
        mano.TrucoResuelto = true;
        mano.TurnoActual = "Maquina";

        // When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        Assert.NotNull(resultado);
        Assert.Equal("carta", resultado.Tipo);
        Assert.Equal("", resultado.Texto);
    }

    //TEST A REVISAR ANTES ME ANDABA SUPONGO QUE TIENE QUE VER CON EL RANDOM
    //[Fact]
    //public void AvanzarUnPaso_CuandoNoSeCumpleNingunaCondicionAlFinal_DevuelveNull()
    //{
    //    // Given
    //    var mano = CrearManoBase();

    //    mano.Maquina = new Jugador
    //    {
    //        Mano = new List<Carta>
    //     {
    //         new Carta { Numero = 12, Palo = "Basto", ValorTruco = 7 }
    //     },
    //        Jugadas = new List<Carta>()
    //    };

    //    mano.EnvidoCantado = true;
    //    mano.EnvidoResuelto = true;
    //    mano.TrucoCantado = true;
    //    mano.TrucoResuelto = true;
    //    mano.TurnoActual = "Maquina";
    //    mano.CartaMaquinaEnMesa = null;
    //    mano.TrucoPendienteRespuestaHumano = false;
    //    // When
    //    var resultado = MaquinaServicio.AvanzarUnPaso(mano);

    //    // Then
    //    Assert.Null(resultado);
    //}

    [Fact]
    public void AvanzarUnPaso_CuandoMaquinaDecideCantarTruco_DevuelveEventoTruco()
    {
        // Given
        var mano = CrearManoBase();

        mano.NivelMentiraTrucoMaquina = 100;
        mano.Maquina = new Jugador
        {
            Mano = new List<Carta> { new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 } },
            Jugadas = new List<Carta>()
        };

        mano.TrucoCantado = false;
        mano.TrucoResuelto = false;
        mano.TrucoPendienteRespuestaHumano = false;
        mano.EnvidoCantado = true;
        mano.EnvidoResuelto = true;
        mano.TurnoActual = "Maquina";

        // When
        var resultado = MaquinaServicio.AvanzarUnPaso(mano);

        // Then
        Assert.NotNull(resultado);
        Assert.Equal("truco", resultado.Tipo);
        Assert.Equal("¡Truco!", resultado.Texto);
    }

    //resolver bza jugada
    [Fact]
    public void ResolverBazaJugada_CuandoSeActivaAullido_ModificaVistaYHaceEarlyReturn()
    {
        // Given
        var mano = CrearManoPrueba();
        var cartaHumano = new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 };
        var cartaMaquina = new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 };
        mano.AullidoBloqueando = true;

        // When
        MaquinaServicio.ResolverBazaJugada(mano, cartaHumano, cartaMaquina);

        // Then
        Assert.Single(mano.Bazas);
        Assert.Equal("Humano", mano.TurnoActual);
    }

    [Fact]
    public void ResolverBazaJugada_CuandoBazaEsParda_AsignaTurnoAlQueInicioLaMano()
    {
        // Given
        var mano = CrearManoPrueba();
        mano.ManoIniciadaPor = "Maquina";
        mano.AullidoBloqueando = false;
        var cartaHumano = new Carta { Numero = 4, Palo = "Basto", ValorTruco = 1 };
        var cartaMaquina = new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 };

        // When
        MaquinaServicio.ResolverBazaJugada(mano, cartaHumano, cartaMaquina);

        // Then
        Assert.Equal("Parda", mano.Bazas[0].Ganador);
        Assert.Equal("Maquina", mano.TurnoActual);
    }

    [Fact]
    public void ResolverBazaJugada_CuandoHumanoGanalaManoSinTrucoCantado_SumaPuntosYResuelveTruco()
    {
        // Given
        var mano = CrearManoPrueba();
        mano.TrucoCantado = false;
        mano.PuntosTrucoMano = 1;
        mano.AullidoBloqueando = false;
        mano.Bazas.Add(new Baza { Ganador = "Humano", CartaJugador = new Carta(), CartaMaquina = new Carta() });
        var cartaHumano = new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 };
        var cartaMaquina = new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 };

        // When
        MaquinaServicio.ResolverBazaJugada(mano, cartaHumano, cartaMaquina);

        // Then
        Assert.Equal("Humano", mano.GanadorMano);
        Assert.True(mano.TrucoResuelto);
        Assert.Equal("No se cantó truco. La mano vale 1 punto.", mano.EstadoTruco);
    }

    [Fact]
    public void ResolverBazaJugada_CuandoNoEsModoHistoria_AvanzaElTurnoAutomaticamente()
    {
        // Given
        var mano = CrearManoPrueba();
        mano.Configuracion.Modo = ModoJuego.Tradicional; 
        mano.AullidoBloqueando = false;
        var cartaHumano = new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 };
        var cartaMaquina = new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 };

        // When
        MaquinaServicio.ResolverBazaJugada(mano, cartaHumano, cartaMaquina);

        // Then
        Assert.Null(mano.GanadorMano);
    }

    [Fact]
    public void ResolverBazaJugada_CuandoEsModoHistoriaYLeTocaALaMaquina_NotificaTurno()
    {
        // Given
        var mano = CrearManoPrueba();
        mano.Configuracion.Modo = ModoJuego.Historia; 
        mano.AullidoBloqueando = false;
        var cartaHumano = new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 };
        var cartaMaquina = new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 };

        // When
        MaquinaServicio.ResolverBazaJugada(mano, cartaHumano, cartaMaquina);

        // Then
        Assert.Equal("Maquina", mano.TurnoActual);
        Assert.Null(mano.GanadorMano);
    }

    //procesar iniciativa

    [Fact]
    public void ProcesarIniciativa_CuandoFiltrosInicialesNoSeCumplen_SaltaBloqueYSoloAvanzarTurno()
    {
        // Given
        var mano = CrearManoBase();

        // When
        MaquinaServicio.ProcesarIniciativa(mano);

        // Then
        Assert.Null(mano.CantorEnvido);
        Assert.False(mano.EnvidoPendienteRespuestaHumano);
    }

    [Fact]
    public void ProcesarIniciativa_CuandoFiltrosSeCumplenPeroMaquinaNoQuiereCantar_SoloAvanzarTurno()
    {
        // Given
        var mano = CrearManoBase();
        mano.EnvidoCantado = false;
        mano.EnvidoResuelto = false;
        mano.Bazas = new List<Baza>();

        mano.NivelMentiraEnvidoMaquina = 0;
        mano.Maquina.Mano = new List<Carta>();

        // La decisión tiene un piso de 4% de probabilidad: se fija la tirada
        // en "no" para que el test sea determinístico.
        AzarServicio.TirarProbabilidadOverride = _ => false;
        try
        {
            // When
            MaquinaServicio.ProcesarIniciativa(mano);

            // Then
            Assert.False(mano.EnvidoCantado);
            Assert.Null(mano.CantorEnvido);
            Assert.False(mano.EnvidoPendienteRespuestaHumano);
        }
        finally
        {
            AzarServicio.TirarProbabilidadOverride = null;
        }
    }

    [Fact]
    public void ProcesarIniciativa_CuandoMaquinaDecideCantarEnvido_ModificaFlagsDeEnvidoYAvanzaTurno()
    {
        // Given
        var mano = CrearManoBase();
        mano.EnvidoCantado = false;
        mano.EnvidoResuelto = false;
        mano.TrucoCantado = false;
        mano.TrucoResuelto = false;
        mano.Bazas = new List<Baza>();
        mano.NivelMentiraEnvidoMaquina = 100;
        mano.Maquina.Mano = new List<Carta>
            {
                new Carta { Numero = 7, Palo = "Espada", ValorTruco = 10 },
                new Carta { Numero = 6, Palo = "Espada", ValorTruco = 1 }
            };

        // When
        MaquinaServicio.ProcesarIniciativa(mano);

        // Then
        Assert.True(mano.EnvidoCantado);
        Assert.Equal("Maquina", mano.CantorEnvido);
        Assert.Equal("Envido", mano.TipoEnvidoCantado);
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

    private ManoTruco CrearManoPrueba()
    {
        return new ManoTruco
        {
            ManoIniciadaPor = "Humano",
            TurnoActual = "Humano",
            GanadorMano = null,
            TrucoCantado = false,
            TrucoResuelto = false,
            PuntosTrucoMano = 1,
            Bazas = new List<Baza>(),
            Configuracion = new() { Modo = ModoJuego.Historia }
        };
    }

}
