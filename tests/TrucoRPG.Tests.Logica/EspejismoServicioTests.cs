using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;
using Xunit;

public class EspejismoServicioTests
{
  private static ManoTruco CrearManoLuzMalaMano()
  {
    var carta = new Carta { Numero = 3, Palo = "Oro", ValorTruco = 10 };
    var mano = new ManoTruco
    {
      Configuracion = new ConfiguracionPartida
      {
        Modo = ModoJuego.Historia,
        HeroeDelHumano = ClaseHeroe.Mentiroso,
        RivalDeLaMaquina = ClaseRival.LuzMala,
        RivalNivel = 4
      },
      ManoIniciadaPor = IdJugador.Maquina,
      TurnoActual = IdJugador.Maquina,
      CartaMaquinaEnMesa = carta,
      Humano = new Jugador { Mano = [new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 }] },
      Maquina = new Jugador { Mano = [new Carta { Numero = 7, Palo = "Copa", ValorTruco = 4 }] }
    };
    PartidaMemoriaServicio.Guardar(mano);
    return mano;
  }

  [Fact]
  public void Intentar_ConProbabilidadSegura_ActivaEspejismoYBloquea()
  {
    AzarServicio.TirarProbabilidadOverride = _ => true;
    AzarServicio.MonedaCaraOverride = () => true;
    try
    {
      var mano = CrearManoLuzMalaMano();

      var activo = EspejismoServicio.IntentarAlJugarPrimeraCarta(mano);

      Assert.True(activo);
      Assert.True(mano.EspejismoActivo);
      Assert.True(mano.EspejismoBloqueando);
      Assert.NotNull(mano.EspejismoCartaFalsa);
      Assert.False(EsMismaCarta(mano.CartaMaquinaEnMesa!, mano.EspejismoCartaFalsa));
      Assert.False(mano.Humano.Mano.Any(c => EsMismaCarta(c, mano.EspejismoCartaFalsa)));
      Assert.False(mano.Maquina.Mano.Any(c => EsMismaCarta(c, mano.EspejismoCartaFalsa)));
      Assert.True(MazoServicio.CrearMazo().Any(c => EsMismaCarta(c, mano.EspejismoCartaFalsa)));
      Assert.True(mano.EspejismoUsadoEnMano);
      Assert.False(mano.DestelloPendiente);
      Assert.False(mano.DestelloBloqueando);
    }
    finally
    {
      AzarServicio.TirarProbabilidadOverride = null;
      AzarServicio.MonedaCaraOverride = null;
    }
  }

  [Fact]
  public void Intentar_CartaFalsa_NuncaEsCartaInvalidaDelMazo()
  {
    AzarServicio.TirarProbabilidadOverride = _ => true;
    var mazo = MazoServicio.CrearMazo();
    try
    {
      for (var intento = 0; intento < 30; intento++)
      {
        var mano = CrearManoLuzMalaMano();
        mano.Humano.Mano =
        [
          new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
          new Carta { Numero = 7, Palo = "Basto", ValorTruco = 4 },
          new Carta { Numero = 12, Palo = "Copa", ValorTruco = 7 },
        ];
        mano.Maquina.Mano =
        [
          new Carta { Numero = 2, Palo = "Oro", ValorTruco = 9 },
          new Carta { Numero = 6, Palo = "Espada", ValorTruco = 3 },
        ];
        mano.CartaMaquinaEnMesa = new Carta { Numero = 3, Palo = "Oro", ValorTruco = 10 };

        EspejismoServicio.IntentarAlJugarPrimeraCarta(mano);

        Assert.NotNull(mano.EspejismoCartaFalsa);
        Assert.True(mazo.Any(c => EsMismaCarta(c, mano.EspejismoCartaFalsa!)));
        Assert.False(mano.Humano.Mano.Any(c => EsMismaCarta(c, mano.EspejismoCartaFalsa!)));
        Assert.False(mano.Maquina.Mano.Any(c => EsMismaCarta(c, mano.EspejismoCartaFalsa!)));
        Assert.False(EsMismaCarta(mano.CartaMaquinaEnMesa, mano.EspejismoCartaFalsa));
      }
    }
    finally
    {
      AzarServicio.TirarProbabilidadOverride = null;
    }
  }

  private static bool EsMismaCarta(Carta a, Carta b) =>
    a.Numero == b.Numero
    && a.Palo.Equals(b.Palo, StringComparison.OrdinalIgnoreCase);

  [Fact]
  public void ConfirmarOverlay_HabilitaAlternancia()
  {
    var mano = CrearManoLuzMalaMano();
    mano.EspejismoActivo = true;
    mano.EspejismoBloqueando = true;
    mano.EspejismoCartaFalsa = new Carta { Numero = 5, Palo = "Copa", ValorTruco = 2 };

    EspejismoServicio.ConfirmarOverlay(mano);

    Assert.False(mano.EspejismoBloqueando);
    Assert.True(mano.EspejismoAlternando);
  }

  [Fact]
  public void Finalizar_LimpiaEstadoEspejismo()
  {
    var mano = CrearManoLuzMalaMano();
    mano.EspejismoActivo = true;
    mano.EspejismoAlternando = true;
    mano.EspejismoCartaFalsa = new Carta { Numero = 5, Palo = "Copa", ValorTruco = 2 };

    EspejismoServicio.Finalizar(mano);

    Assert.False(mano.EspejismoActivo);
    Assert.False(mano.EspejismoAlternando);
    Assert.Null(mano.EspejismoCartaFalsa);
  }

  [Fact]
  public void JugarCarta_ConEspejismoActivo_FinalizaEspejismo()
  {
    AzarServicio.TirarProbabilidadOverride = _ => true;
    try
    {
      var mano = CrearManoLuzMalaMano();
      mano.ContadorTurnosHumanoPartida = 1;
      EspejismoServicio.IntentarAlJugarPrimeraCarta(mano);
      new ConfirmarEspejismoUseCase().Ejecutar(mano.Id);

      var resultado = new JugarCartaUseCase().Ejecutar(mano.Id, 1, "Espada");

      Assert.False(resultado.EspejismoActivo);
      Assert.False(resultado.EspejismoAlternando);
    }
    finally
    {
      AzarServicio.TirarProbabilidadOverride = null;
    }
  }
}
