using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class RasgunoServicioTests
    {
        private static ManoTruco ManoConCartas(params (int numero, string palo, int valor)[] cartas) =>
            new()
            {
                Humano = new Jugador
                {
                    Mano = cartas.Select(c => new Carta
                    {
                        Numero = c.numero,
                        Palo = c.palo,
                        ValorTruco = c.valor
                    }).ToList()
                }
            };

        [Fact]
        public void DebilitarCartaAleatoria_UnEspada_PasaAValor13()
        {
            var mano = ManoConCartas((1, "Espada", 14));

            RasgunoServicio.DebilitarCartaAleatoria(mano);

            var carta = Assert.Single(mano.Humano.Mano);
            Assert.Equal(13, carta.ValorTruco);
            Assert.Equal(1, carta.Numero);
            Assert.Equal("Basto", carta.Palo);
        }

        [Fact]
        public void DebilitarCartaAleatoria_Tres_PasaAValor9()
        {
            var mano = ManoConCartas((3, "Copa", 10));

            RasgunoServicio.DebilitarCartaAleatoria(mano);

            var carta = Assert.Single(mano.Humano.Mano);
            Assert.Equal(9, carta.ValorTruco);
            Assert.Equal(2, carta.Numero);
        }

        [Fact]
        public void DebilitarCartaAleatoria_SoloCartasMinimas_NoCambiaNada()
        {
            var mano = ManoConCartas((4, "Oro", 1), (4, "Copa", 1), (4, "Basto", 1));

            RasgunoServicio.DebilitarCartaAleatoria(mano);

            Assert.All(mano.Humano.Mano, c => Assert.Equal(1, c.ValorTruco));
        }

        [Fact]
        public void DebilitarCartaAleatoria_NuncaDuplicaCartaYaPresenteEnMano()
        {
            //Given
            var plantilla = new (int numero, string palo, int valor)[]
            {
                (3, "Copa", 10),
                (2, "Copa", 9),
                (7, "Oro", 11),
            };

            for (var intento = 0; intento < 500; intento++)
            {
                var mano = ManoConCartas(plantilla);

                //When
                RasgunoServicio.DebilitarCartaAleatoria(mano);

                //Then
                var claves = mano.Humano.Mano.Select(c => (c.Numero, c.Palo)).ToList();
                Assert.Equal(claves.Count, claves.Distinct().Count());
            }
        }

        [Fact]
        public void DebilitarCartaAleatoria_NoDuplicaCartaEnMesaNiEnManoDelRival()
        {
            var tresEspadaMesa = new Carta { Numero = 3, Palo = "Espada", ValorTruco = 10 };
            var dosCopaRival = new Carta { Numero = 2, Palo = "Copa", ValorTruco = 9 };
            var plantillaHumano = new (int numero, string palo, int valor)[]
            {
                (3, "Copa", 10),
                (7, "Oro", 11),
                (4, "Basto", 1),
            };

            for (var intento = 0; intento < 200; intento++)
            {
                var mano = ManoConCartas(plantillaHumano);
                mano.Maquina.Mano = [dosCopaRival];
                mano.Bazas =
                [
                    new Baza { CartaJugador = tresEspadaMesa, CartaMaquina = new Carta { Numero = 5, Palo = "Oro", ValorTruco = 2 }, Ganador = IdJugador.Humano }
                ];

                RasgunoServicio.DebilitarCartaAleatoria(mano);

                var enJuego = mano.Humano.Mano
                    .Select(c => (c.Numero, c.Palo))
                    .Concat(mano.Maquina.Mano.Select(c => (c.Numero, c.Palo)))
                    .Concat(mano.Bazas.SelectMany(b => new[] { b.CartaJugador, b.CartaMaquina })
                        .Where(c => c != null)
                        .Select(c => (c!.Numero, c.Palo)))
                    .ToList();

                Assert.Equal(enJuego.Count, enJuego.DistinctBy(c => (c.Numero, c.Palo.ToLowerInvariant())).Count());
                Assert.DoesNotContain(
                    mano.Humano.Mano,
                    c => c.Numero == 3 && c.Palo.Equals("Espada", StringComparison.OrdinalIgnoreCase));
                Assert.DoesNotContain(
                    mano.Humano.Mano,
                    c => c.Numero == 2 && c.Palo.Equals("Copa", StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
