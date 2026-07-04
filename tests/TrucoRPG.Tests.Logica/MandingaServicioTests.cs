using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using Xunit;

public class MandingaServicioTests
{
  private static ManoTruco CrearManoMandinga(int numeroDeMano = 1, int puntosHumano = 0)
  {
    var mano = new ManoTruco
    {
      Configuracion = new ConfiguracionPartida
      {
        Modo = ModoJuego.Historia,
        HeroeDelHumano = ClaseHeroe.Mentiroso,
        RivalDeLaMaquina = ClaseRival.Mandinga,
        RivalNivel = 5
      },
      NumeroDeMano = numeroDeMano,
      PuntosHumano = puntosHumano,
      Humano = new Jugador
      {
        Mano =
        [
          new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
          new Carta { Numero = 7, Palo = "Oro", ValorTruco = 11 },
          new Carta { Numero = 3, Palo = "Basto", ValorTruco = 10 },
        ]
      },
      Maquina = new Jugador
      {
        Mano =
        [
          new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 },
          new Carta { Numero = 5, Palo = "Espada", ValorTruco = 2 },
          new Carta { Numero = 6, Palo = "Oro", ValorTruco = 3 },
        ]
      }
    };
    PartidaMemoriaServicio.Guardar(mano);
    return mano;
  }

  [Fact]
  public void OnManoIniciada_ManoImpar_ActivaMaldicionBloqueando()
  {
    var mano = CrearManoMandinga(numeroDeMano: 1);

    MandingaServicio.OnManoIniciada(mano);

    Assert.True(mano.MandingaMaldicionBloqueando);
  }

  [Fact]
  public void OnManoIniciada_ManoPar_NoActivaMaldicion()
  {
    var mano = CrearManoMandinga(numeroDeMano: 2);

    MandingaServicio.OnManoIniciada(mano);

    Assert.False(mano.MandingaMaldicionBloqueando);
  }

  [Fact]
  public void OnManoIniciada_Con10Puntos_ActivaEnganoYDesbloqueaFase2()
  {
    var mano = CrearManoMandinga(numeroDeMano: 8, puntosHumano: 10);

    MandingaServicio.OnManoIniciada(mano);

    Assert.True(mano.MandingaFase2Desbloqueada);
    Assert.Equal(8, mano.MandingaPrimeraManoEngano);
    Assert.True(mano.MandingaEnganoBloqueando);
  }

  [Fact]
  public void OnManoIniciada_Con20PuntosYAjugadas_AplicaEspejoPrimero()
  {
    var mano = CrearManoMandinga(numeroDeMano: 12, puntosHumano: 20);
    mano.MandingaFase2Desbloqueada = true;
    mano.MandingaFase3Desbloqueada = true;
    mano.MandingaPrimeraManoEngano = 8;
    mano.MandingaJugadasHumanoManoAnterior =
    [
      new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
      new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 },
      new Carta { Numero = 6, Palo = "Oro", ValorTruco = 3 },
    ];

    MandingaServicio.OnManoIniciada(mano);

    Assert.True(mano.MandingaEspejoBloqueando);
    Assert.Equal(1, mano.Maquina.Mano[0].Numero);
    Assert.Equal("Espada", mano.Maquina.Mano[0].Palo);
  }

  [Fact]
  public void LiquidarPuntosMaldicion_HumanoGana_RestaUnoAlTotal()
  {
    var mano = CrearManoMandinga();
    mano.MandingaMaldicionActivaEnMano = true;
    mano.GanadorMano = IdJugador.Humano;
    mano.PuntosHumanoAcumuladosMano = 3;

    MandingaServicio.LiquidarPuntosMaldicion(mano);

    Assert.Equal(2, mano.PuntosHumano);
    Assert.False(mano.MandingaMaldicionActivaEnMano);
  }

  [Fact]
  public void LiquidarPuntosMaldicion_MaquinaGana_DuplicaTotal()
  {
    var mano = CrearManoMandinga();
    mano.MandingaMaldicionActivaEnMano = true;
    mano.GanadorMano = IdJugador.Maquina;
    mano.PuntosMaquinaAcumuladosMano = 3;

    MandingaServicio.LiquidarPuntosMaldicion(mano);

    Assert.Equal(6, mano.PuntosMaquina);
  }

  [Fact]
  public void ConfirmarEngano_MezclaManoHumano()
  {
    var mano = CrearManoMandinga();
    var antes = mano.Humano.Mano.Select(c => $"{c.Numero}-{c.Palo}").OrderBy(x => x).ToList();
    mano.MandingaEnganoBloqueando = true;

    MandingaServicio.ConfirmarEngano(mano);

    var despues = mano.Humano.Mano.Select(c => $"{c.Numero}-{c.Palo}").OrderBy(x => x).ToList();
    Assert.Equal(antes, despues);
    Assert.True(mano.MandingaEnganoManoOculta);
    Assert.False(mano.MandingaEnganoBloqueando);
  }
}
