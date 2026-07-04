using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;
using Xunit;

public class DestelloServicioTests
{
  private static ManoTruco CrearManoLuzMala(int contadorTurnos = 0, int bazasJugadas = 0)
  {
    var mano = new ManoTruco
    {
      Configuracion = new ConfiguracionPartida
      {
        Modo = ModoJuego.Historia,
        HeroeDelHumano = ClaseHeroe.Mentiroso,
        RivalDeLaMaquina = ClaseRival.LuzMala,
        RivalNivel = 4
      },
      TurnoActual = IdJugador.Humano,
      ContadorTurnosHumanoPartida = contadorTurnos,
      Humano = new Jugador
      {
        Mano =
        [
          new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
          new Carta { Numero = 7, Palo = "Oro", ValorTruco = 11 },
          new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 },
        ]
      },
      Maquina = new Jugador
      {
        Mano =
        [
          new Carta { Numero = 3, Palo = "Basto", ValorTruco = 10 },
          new Carta { Numero = 6, Palo = "Copa", ValorTruco = 3 },
          new Carta { Numero = 2, Palo = "Oro", ValorTruco = 2 },
        ]
      }
    };

    for (var i = 0; i < bazasJugadas; i++)
      mano.Bazas.Add(new Baza());

    PartidaMemoriaServicio.Guardar(mano);
    return mano;
  }

  [Fact]
  public void EvaluarTurnoHumano_CicloPendienteBazaObjetivo_ActivaDestello()
  {
    var mano = CrearManoLuzMala(contadorTurnos: 0, bazasJugadas: 1);
    mano.DestelloPendiente = true;
    mano.DestelloBazaObjetivo = 2;

    DestelloServicio.EvaluarTurnoHumano(mano);

    Assert.True(mano.DestelloBloqueando);
    Assert.Contains("Destello", mano.UltimoMensajeHabilidadRival);
  }

  [Fact]
  public void EvaluarTurnoHumano_CicloPendienteBazaIncorrecta_NoActivaDestello()
  {
    var mano = CrearManoLuzMala(contadorTurnos: 0, bazasJugadas: 1);
    mano.DestelloPendiente = true;
    mano.DestelloBazaObjetivo = 1;

    DestelloServicio.EvaluarTurnoHumano(mano);

    Assert.False(mano.DestelloBloqueando);
  }

  [Fact]
  public void EvaluarTurnoHumano_PrimerCicloBaza1_ActivaDestello()
  {
    var mano = CrearManoLuzMala(contadorTurnos: 0);
    mano.DestelloPendiente = true;
    mano.DestelloBazaObjetivo = 1;

    DestelloServicio.EvaluarTurnoHumano(mano);

    Assert.True(mano.DestelloBloqueando);
  }

  [Fact]
  public void EvaluarTurnoHumano_ContadorImpar_NoIniciaCiclo()
  {
    var mano = CrearManoLuzMala(contadorTurnos: 1);

    DestelloServicio.EvaluarTurnoHumano(mano);

    Assert.False(mano.DestelloPendiente);
    Assert.False(mano.DestelloBloqueando);
  }

  [Fact]
  public void EvaluarTurnoHumano_Baza3_NoActivaDestello()
  {
    var mano = CrearManoLuzMala(contadorTurnos: 0, bazasJugadas: 2);
    mano.DestelloPendiente = true;
    mano.DestelloBazaObjetivo = 2;

    DestelloServicio.EvaluarTurnoHumano(mano);

    Assert.False(mano.DestelloBloqueando);
  }

  [Fact]
  public void EvaluarTurnoHumano_ConEspejismoUsadoEnMano_NoActivaDestello()
  {
    var mano = CrearManoLuzMala(contadorTurnos: 0, bazasJugadas: 1);
    mano.DestelloPendiente = true;
    mano.DestelloBazaObjetivo = 2;
    mano.EspejismoUsadoEnMano = true;

    DestelloServicio.EvaluarTurnoHumano(mano);

    Assert.False(mano.DestelloBloqueando);
  }

  [Fact]
  public void ConfirmarDestello_JuegaCartaAleatoriaYLimpiaCiclo()
  {
    var mano = CrearManoLuzMala(contadorTurnos: 0);
    mano.DestelloBloqueando = true;
    mano.DestelloPendiente = true;
    mano.DestelloBazaObjetivo = 1;
    var cartasAntes = mano.Humano.Mano.Count;

    var resultado = new ConfirmarDestelloUseCase().Ejecutar(mano.Id);

    Assert.False(resultado.DestelloBloqueando);
    Assert.False(resultado.DestelloPendiente);
    Assert.Equal(0, resultado.DestelloBazaObjetivo);
    Assert.Equal(cartasAntes - 1, resultado.Humano.Mano.Count);
    Assert.Equal(1, resultado.ContadorTurnosHumanoPartida);
    Assert.Contains("confundió", resultado.UltimoMensajeHabilidadRival);
  }
}
