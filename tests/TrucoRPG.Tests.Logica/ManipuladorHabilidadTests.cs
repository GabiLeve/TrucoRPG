using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class ManipuladorHabilidadTests
{
    [Fact]
    public void CrearManoNueva_Manipulador_Guarda34CartasEnMazoRestante()
    {
        var mano = PartidaServicio.CrearManoNueva(configuracion: new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Manipulador
        });

        Assert.Equal(34, mano.CartasRestantesMazo.Count);
        Assert.Null(mano.RepartoContext);
    }

    [Fact]
    public void CrearManoNueva_Manipulador_SumaValorTrucoHumanoMayorEnPromedio()
    {
        const int repeticiones = 400;
        long sumaConManipulador = 0;
        long sumaTradicional = 0;

        for (int i = 0; i < repeticiones; i++)
        {
            var conHeroe = PartidaServicio.CrearManoNueva(configuracion: new ConfiguracionPartida
            {
                Modo = ModoJuego.Historia,
                HeroeDelHumano = ClaseHeroe.Manipulador
            });
            sumaConManipulador += conHeroe.Humano.Mano.Sum(c => c.ValorTruco);

            var tradicional = PartidaServicio.CrearManoNueva();
            sumaTradicional += tradicional.Humano.Mano.Sum(c => c.ValorTruco);
        }

        Assert.True(sumaConManipulador > sumaTradicional);
    }

    [Fact]
    public void Repartir_ProbabilidadTotalMejora_AumentaValorRespectoAlRepartoNeutral()
    {
        const int repeticiones = 200;
        long sumaConMejora = 0;
        long sumaNeutral = 0;

        for (int i = 0; i < repeticiones; i++)
        {
            var conMejora = new ManoTruco
            {
                Humano = new Jugador { Id = IdJugador.Humano },
                Maquina = new Jugador { Id = IdJugador.Maquina },
                RepartoContext = new RepartoContext
                {
                    Random = new Random(i),
                    ProbMejorarCartaPorJugador = { [IdJugador.Humano] = 1.0 }
                }
            };
            RepartoServicio.Repartir(conMejora);
            sumaConMejora += conMejora.Humano.Mano.Sum(c => c.ValorTruco);

            var neutral = new ManoTruco
            {
                Humano = new Jugador { Id = IdJugador.Humano },
                Maquina = new Jugador { Id = IdJugador.Maquina },
                RepartoContext = new RepartoContext { Random = new Random(i) }
            };
            RepartoServicio.Repartir(neutral);
            sumaNeutral += neutral.Humano.Mano.Sum(c => c.ValorTruco);
        }

        Assert.True(sumaConMejora > sumaNeutral);
    }
}
