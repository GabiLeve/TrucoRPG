using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Habilidades.Rivales;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica;

public class HabilidadesRivalTests
{
    private static ConfiguracionPartida ConfigHistoria(ClaseRival rival) => new()
    {
        Modo = ModoJuego.Historia,
        HeroeDelHumano = ClaseHeroe.Mentiroso,
        RivalDeLaMaquina = rival,
        RivalNivel = 1,
    };

    private static ManoTruco CrearManoGuardada(ClaseRival rival, int numeroDeMano = 1, int puntosHumano = 0)
    {
        var mano = PartidaServicio.CrearManoNueva(numeroDeMano, puntosHumano, 0, ConfigHistoria(rival));
        PartidaMemoriaServicio.Guardar(mano);
        return mano;
    }

    [Theory]
    [InlineData(ClaseRival.Nahuelito, typeof(SalpicaduraHabilidad))]
    [InlineData(ClaseRival.Pomberito, typeof(TravesuraHabilidad))]
    [InlineData(ClaseRival.Lobizon, typeof(RasgunoHabilidad))]
    [InlineData(ClaseRival.LuzMala, typeof(DestelloHabilidad))]
    [InlineData(ClaseRival.Mandinga, typeof(MandingaHabilidad))]
    public void RivalHabilidadFactory_CrearDesdeRival_DevuelveHabilidadCorrecta(ClaseRival rival, Type tipoEsperado)
    {
        var habilidad = RivalHabilidadFactory.CrearDesdeRival(rival);
        Assert.IsType(tipoEsperado, habilidad);
    }

    [Theory]
    [InlineData(TipoHabilidadRival.Salpicadura)]
    [InlineData(TipoHabilidadRival.Travesura)]
    [InlineData(TipoHabilidadRival.Rasguno)]
    [InlineData(TipoHabilidadRival.Destello)]
    [InlineData(TipoHabilidadRival.MandingaFases)]
    [InlineData(TipoHabilidadRival.Ninguna)]
    public void RivalHabilidadFactory_CrearPorTipo_NoLanza(TipoHabilidadRival tipo)
    {
        var habilidad = RivalHabilidadFactory.Crear(tipo);
        Assert.Equal(tipo, habilidad.Tipo);
    }

    [Fact]
    public void HabilidadesRivalOrquestador_ManoImparNahuelito_ActivaSalpicadura()
    {
        var mano = CrearManoGuardada(ClaseRival.Nahuelito, numeroDeMano: 1);

        HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

        Assert.True(mano.SalpicaduraActiva);
        Assert.True(mano.SalpicaduraBloqueando);
        Assert.Contains("Salpicadura", mano.UltimoMensajeHabilidadRival);
        Assert.NotNull(mano.VistaHabilidadesRival);
    }

    [Fact]
    public void HabilidadesRivalOrquestador_ManoParNahuelito_NoActivaSalpicadura()
    {
        var mano = CrearManoGuardada(ClaseRival.Nahuelito, numeroDeMano: 2);

        HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

        Assert.False(mano.SalpicaduraBloqueando);
    }

    [Fact]
    public void HabilidadesRivalOrquestador_ManoImparPomberito_ActivaTravesura()
    {
        var mano = CrearManoGuardada(ClaseRival.Pomberito, numeroDeMano: 3);

        HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

        Assert.True(mano.TravesuraActiva);
        Assert.True(mano.TravesuraBloqueando);
        Assert.Contains("Travesura", mano.UltimoMensajeHabilidadRival);
    }

    [Fact]
    public void HabilidadesRivalOrquestador_ManoImparLobizon_ActivaRasguno()
    {
        var mano = CrearManoGuardada(ClaseRival.Lobizon, numeroDeMano: 1);

        HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

        Assert.True(mano.RasgunoActivo);
        Assert.True(mano.RasgunoBloqueando);
    }

    [Fact]
    public void HabilidadesRivalOrquestador_Mandinga_DelegaEnMandingaServicio()
    {
        var mano = CrearManoGuardada(ClaseRival.Mandinga, numeroDeMano: 1);

        HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

        Assert.True(mano.MandingaMaldicionBloqueando);
    }

    [Fact]
    public void SalpicaduraServicio_ReemplazarCartasHumano_CambiaPalos()
    {
        var mano = CrearManoGuardada(ClaseRival.Nahuelito);
        var snapshot = mano.Humano.Mano.Select(c => c.Palo).ToList();

        SalpicaduraServicio.ReemplazarCartasHumano(mano);

        Assert.Equal(snapshot.Count, mano.Humano.Mano.Count);
        Assert.Contains(
            mano.Humano.Mano.Select((c, i) => c.Palo != snapshot[i]),
            cambio => cambio);
    }

    [Fact]
    public void SalpicaduraServicio_CambiarPaloCarta_ActualizaValorTruco()
    {
        var carta = new Carta { Numero = 7, Palo = "Espada", ValorTruco = 10 };
        var paloAntes = carta.Palo;
        var mano = new ManoTruco
        {
            Humano = new Jugador { Mano = [carta] }
        };

        SalpicaduraServicio.CambiarPaloCarta(carta, mano);

        Assert.NotEqual(paloAntes, carta.Palo);
        Assert.Equal(MazoServicio.ObtenerValorTruco(carta.Numero, carta.Palo), carta.ValorTruco);
    }

    [Fact]
    public void SalpicaduraServicio_ReemplazarCartasHumano_NoDuplicaCartasEnJuego()
    {
        var tresEspada = new Carta { Numero = 3, Palo = "Espada", ValorTruco = 10 };
        var tresCopa = new Carta { Numero = 3, Palo = "Copa", ValorTruco = 10 };
        var sieteOro = new Carta { Numero = 7, Palo = "Oro", ValorTruco = 4 };
        var mano = new ManoTruco
        {
            Humano = new Jugador { Mano = [tresCopa, sieteOro, new Carta { Numero = 4, Palo = "Basto", ValorTruco = 1 }] },
            Maquina = new Jugador { Mano = [new Carta { Numero = 5, Palo = "Copa", ValorTruco = 2 }] },
            Bazas =
            [
                new Baza { CartaJugador = tresEspada, CartaMaquina = new Carta { Numero = 2, Palo = "Oro", ValorTruco = 9 }, Ganador = IdJugador.Humano }
            ]
        };

        for (var intento = 0; intento < 50; intento++)
        {
            tresCopa.Palo = "Copa";
            tresCopa.ValorTruco = 10;
            sieteOro.Palo = "Oro";
            sieteOro.ValorTruco = 4;

            SalpicaduraServicio.ReemplazarCartasHumano(mano);

            var cartasEnJuego = mano.Humano.Mano
                .Select(c => (c.Numero, c.Palo))
                .Concat(mano.Maquina.Mano.Select(c => (c.Numero, c.Palo)))
                .Concat(mano.Bazas.SelectMany(b => new[] { b.CartaJugador, b.CartaMaquina })
                    .Where(c => c != null)
                    .Select(c => (c!.Numero, c.Palo)))
                .ToList();

            Assert.Equal(cartasEnJuego.Count, cartasEnJuego.DistinctBy(c => (c.Numero, c.Palo.ToLowerInvariant())).Count());
            Assert.DoesNotContain(
                mano.Humano.Mano,
                c => c.Numero == 3 && c.Palo.Equals("Espada", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void SalpicaduraServicio_CambiarPaloCarta_NoDuplicaCartaEnMesa()
    {
        var tresEspadaMesa = new Carta { Numero = 3, Palo = "Espada", ValorTruco = 10 };
        var tresCopaMano = new Carta { Numero = 3, Palo = "Copa", ValorTruco = 10 };
        var mano = new ManoTruco
        {
            Humano = new Jugador { Mano = [tresCopaMano] },
            Bazas = [new Baza { CartaJugador = tresEspadaMesa, Ganador = IdJugador.Humano }]
        };

        for (var intento = 0; intento < 20; intento++)
        {
            tresCopaMano.Palo = "Copa";
            tresCopaMano.ValorTruco = 10;

            SalpicaduraServicio.CambiarPaloCarta(tresCopaMano, mano);

            Assert.False(
                tresCopaMano.Numero == 3
                && tresCopaMano.Palo.Equals("Espada", StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void TravesuraServicio_OcultarCartasHumano_OcultaDosCartas()
    {
        var mano = CrearManoGuardada(ClaseRival.Pomberito);
        mano.TravesuraBloqueando = true;

        TravesuraServicio.OcultarCartasHumano(mano);

        Assert.Equal(2, mano.CartasOcultasTravesura.Count);
    }

    [Fact]
    public void ConfirmarSalpicadura_LimpiaBloqueoYCambiaCartas()
    {
        var mano = CrearManoGuardada(ClaseRival.Nahuelito);
        mano.SalpicaduraBloqueando = true;
        PartidaMemoriaServicio.Actualizar(mano);

        var resultado = new ConfirmarSalpicaduraUseCase().Ejecutar(mano.Id);

        Assert.False(resultado.SalpicaduraBloqueando);
        Assert.Contains("Salpicadura", resultado.UltimoMensajeHabilidadRival);
    }

    [Fact]
    public void ConfirmarTravesura_OcultaCartasYLimpiaBloqueo()
    {
        var mano = CrearManoGuardada(ClaseRival.Pomberito);
        mano.TravesuraBloqueando = true;
        PartidaMemoriaServicio.Actualizar(mano);

        var resultado = new ConfirmarTravesuraUseCase().Ejecutar(mano.Id);

        Assert.False(resultado.TravesuraBloqueando);
        Assert.Equal(2, resultado.CartasOcultasTravesura.Count);
    }

    [Fact]
    public void ConfirmarRasguno_DebilitaCartaYLimpiaBloqueo()
    {
        var mano = CrearManoGuardada(ClaseRival.Lobizon);
        mano.RasgunoBloqueando = true;
        var sumaAntes = mano.Humano.Mano.Sum(c => c.ValorTruco);
        PartidaMemoriaServicio.Actualizar(mano);

        var resultado = new ConfirmarRasgunoUseCase().Ejecutar(mano.Id);

        Assert.False(resultado.RasgunoBloqueando);
        Assert.True(resultado.Humano.Mano.Sum(c => c.ValorTruco) <= sumaAntes);
    }

    [Fact]
    public void ConfirmarMandingaEspejo_LimpiaBloqueoYEncadenaEngano()
    {
        var mano = CrearManoGuardada(ClaseRival.Mandinga, numeroDeMano: 8, puntosHumano: 10);
        mano.MandingaEspejoBloqueando = true;
        mano.MandingaEnganoProgramadoEstaMano = true;
        PartidaMemoriaServicio.Actualizar(mano);

        var resultado = new ConfirmarMandingaEspejoUseCase().Ejecutar(mano.Id);

        Assert.False(resultado.MandingaEspejoBloqueando);
        Assert.True(resultado.MandingaEnganoBloqueando);
    }

    [Fact]
    public void ConfirmarMandingaMaldicion_ActivaMaldicionEnMano()
    {
        var mano = CrearManoGuardada(ClaseRival.Mandinga);
        mano.MandingaMaldicionBloqueando = true;
        PartidaMemoriaServicio.Actualizar(mano);

        var resultado = new ConfirmarMandingaMaldicionUseCase().Ejecutar(mano.Id);

        Assert.False(resultado.MandingaMaldicionBloqueando);
        Assert.True(resultado.MandingaMaldicionActivaEnMano);
    }

    [Fact]
    public void ConfirmarMandingaEngano_MezclaManoYLimpiaBloqueo()
    {
        var mano = CrearManoGuardada(ClaseRival.Mandinga);
        mano.MandingaEnganoBloqueando = true;
        PartidaMemoriaServicio.Actualizar(mano);

        var resultado = new ConfirmarMandingaEnganoUseCase().Ejecutar(mano.Id);

        Assert.False(resultado.MandingaEnganoBloqueando);
        Assert.True(resultado.MandingaEnganoManoOculta);
    }

    [Fact]
    public void SalpicaduraBloqueoServicio_ConMandingaEngano_LanzaExcepcion()
    {
        var mano = CrearManoGuardada(ClaseRival.Mandinga);
        mano.MandingaEnganoBloqueando = true;

        Assert.Throws<InvalidOperationException>(() => SalpicaduraBloqueoServicio.ValidarNoBloqueado(mano));
    }

    public static IEnumerable<object[]> BloqueosRival =>
    [
        [(Action<ManoTruco>)(m => m.SalpicaduraBloqueando = true), "Salpicadura"],
        [(Action<ManoTruco>)(m => m.TravesuraBloqueando = true), "Travesura"],
        [(Action<ManoTruco>)(m => m.RasgunoBloqueando = true), "Rasguño"],
        [(Action<ManoTruco>)(m => m.AullidoBloqueando = true), "Aullido"],
        [(Action<ManoTruco>)(m => m.DestelloBloqueando = true), "Destello"],
        [(Action<ManoTruco>)(m => m.EspejismoBloqueando = true), "Espejismo"],
        [(Action<ManoTruco>)(m => m.MandingaEspejoBloqueando = true), "Espejo"],
        [(Action<ManoTruco>)(m => m.MandingaMaldicionBloqueando = true), "Pacto"],
    ];

    [Theory]
    [MemberData(nameof(BloqueosRival))]
    public void SalpicaduraBloqueoServicio_BloqueosLanzanExcepcion(Action<ManoTruco> activar, string fragmento)
    {
        var mano = CrearManoGuardada(ClaseRival.Nahuelito);
        activar(mano);

        var ex = Assert.Throws<InvalidOperationException>(() => SalpicaduraBloqueoServicio.ValidarNoBloqueado(mano));
        Assert.Contains(fragmento, ex.Message);
    }
}
