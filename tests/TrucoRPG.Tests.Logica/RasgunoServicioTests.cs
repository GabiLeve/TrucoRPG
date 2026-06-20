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
    }
}
