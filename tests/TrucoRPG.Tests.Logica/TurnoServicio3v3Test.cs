using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class TurnoServicio3v3Test
    {

        //obtener abre siguiente vuelta
        [Fact]
        public void ObtenerAbreSiguienteVuelta_CuandoGanadorEsNullOParda_DevuelveJugadorMano()
        {
            // Given
            var mano = CrearManoBase3v3();
            var vuelta = new Vuelta3v3();
            string? ganadorVuelta = "Parda"; 

            // When
            var resultado = TurnoServicio3v3.ObtenerAbreSiguienteVuelta(mano, vuelta, ganadorVuelta);

            // Then
            Assert.Equal("J1", resultado);
        }

        [Fact]
        public void ObtenerAbreSiguienteVuelta_CuandoHayGanador_DevuelveAlJugadorDelEquipoConLaCartaMasAlta()
        {
            // Given
            var mano = CrearManoBase3v3();
            var vuelta = new Vuelta3v3();
            string ganadorVuelta = "EquipoA";
            vuelta.CartasJugadas = new Dictionary<string, Carta>
            {
                { "J1", new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 } },
                { "J3", new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 } },
                { "J5", new Carta { Numero = 7, Palo = "Basto", ValorTruco = 10 } }
            };

            // When
            var resultado = TurnoServicio3v3.ObtenerAbreSiguienteVuelta(mano, vuelta, ganadorVuelta);

            // Then
            Assert.Equal("J3", resultado);
        }

        [Fact]
        public void ObtenerAbreSiguienteVuelta_CuandoHayGanadorPeroNoHayCartasRegistradas_DevuelveJugadorMano()
        {
            // Given
            var mano = CrearManoBase3v3();
            var vuelta = new Vuelta3v3();
            string ganadorVuelta = "EquipoB";
            vuelta.CartasJugadas = new Dictionary<string, Carta>();

            // When
            var resultado = TurnoServicio3v3.ObtenerAbreSiguienteVuelta(mano, vuelta, ganadorVuelta);

            // Then
            Assert.Equal("J1", resultado);
        }

        //obtener primero siguiente vuelta 
        [Fact]
        public void ObtenerPrimeroDeVueltaSiguiente_CuandoGanadorEsNull_DevuelveJugadorMano()
        {
            // Given
            var mano = CrearManoBase3v3();
            string? ganadorVuelta = null;

            // When
            var resultado = TurnoServicio3v3.ObtenerPrimeroDeVueltaSiguiente(mano, ganadorVuelta);

            // Then
            Assert.Equal("J1", resultado);
        }

        [Fact]
        public void ObtenerPrimeroDeVueltaSiguiente_CuandoGanadorEsParda_DevuelveJugadorMano()
        {
            // Given
            var mano = CrearManoBase3v3();
            string ganadorVuelta = "Parda";

            // When
            var resultado = TurnoServicio3v3.ObtenerPrimeroDeVueltaSiguiente(mano, ganadorVuelta);

            // Then
            Assert.Equal("J1", resultado);
        }

        [Fact]
        public void ObtenerPrimeroDeVueltaSiguiente_CuandoHayUnGanador_DevuelvePrimerJugadorDeEseEquipoSegunOrden()
        {
            // Given
            var mano = CrearManoBase3v3();
            string ganadorVuelta = "J4";

            // When
            var resultado = TurnoServicio3v3.ObtenerPrimeroDeVueltaSiguiente(mano, ganadorVuelta);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("J2", resultado);
        }

        //obtener responsable canto
        [Fact]
        public void ObtenerResponsableCanto_CuandoCantorExisteEnOrden_BuscaAlSiguienteJugadorDelEquipoContrario()
        {
            // Given
            var mano = CrearManoBase3v3();
            string cantorId = "J1";

            // When
            var resultado = TurnoServicio3v3.ObtenerResponsableCanto(mano, cantorId);

            // Then
            Assert.Equal("J2", resultado);
        }

        [Fact]
        public void ObtenerResponsableCanto_CuandoCantorNoExisteEnElOrden_SalteaBucleYRetornaPrimerJugadorContrario()
        {
            // Given
            var mano = CrearManoBase3v3();
            string cantorId = "J_INEXISTENTE";
            mano.Posicion1 = new Jugador { Id = "OTRO" };
            mano.EquipoA.Jugador1 = mano.Posicion1;

            // When
            var resultado = TurnoServicio3v3.ObtenerResponsableCanto(mano, cantorId);

            // Then
            Assert.Equal("J3", resultado);
        }

        //obtener responsable truco
        [Fact]
        public void ObtenerResponsableTruco_CuandoEquipoContrarioTieneAJ1YEstaActivo_DevuelveJ1()
        {
            // Given
            var mano = CrearManoBase3v3();
            string equipoCantorId = "EquipoB";

            // When
            var resultado = TurnoServicio3v3.ObtenerResponsableTruco(mano, equipoCantorId);

            // Then
            Assert.Equal("J1", resultado);
        }

        [Fact]
        public void ObtenerResponsableTruco_CuandoJ1NoEstaActivo_BuscaAlSiguienteDelEquipoContrarioSegunOrden()
        {
            // Given
            var mano = CrearManoBase3v3();
            string equipoCantorId = "EquipoB";
            mano.JugadoresActivos = new List<string> { "J2", "J3", "J4", "J5", "J6" };

            // When
            var resultado = TurnoServicio3v3.ObtenerResponsableTruco(mano, equipoCantorId);

            // Then
            Assert.Equal("J3", resultado);
        }

        [Fact]
        public void ObtenerResponsableTruco_CuandoJ1NoExisteEnElOrden_DevuelvePrimerJugadorDisponibleDelEquipoContrario()
        {
            // Given
            var mano = CrearManoBase3v3(); string equipoCantorId = "EquipoA";
            mano.Posicion1 = new Jugador { Id = "OTRO" };
            mano.EquipoA.Jugador1 = mano.Posicion1;

            // When
            var resultado = TurnoServicio3v3.ObtenerResponsableTruco(mano, equipoCantorId);

            // Then
            Assert.Equal("J2", resultado);
        }


        private ManoTruco3v3 CrearManoBase3v3()
        {
            var mano = new ManoTruco3v3
            {
                JugadorMano = "J1",

                Posicion1 = new Jugador { Id = "J1" }, 
                Posicion2 = new Jugador { Id = "J2" }, 
                Posicion3 = new Jugador { Id = "J3" }, 
                Posicion4 = new Jugador { Id = "J4" },
                Posicion5 = new Jugador { Id = "J5" }, 
                Posicion6 = new Jugador { Id = "J6" }  
            };

            mano.EquipoA.Id = "EquipoA";
            mano.EquipoA.Jugador1 = mano.Posicion1;
            mano.EquipoA.Jugador2 = mano.Posicion3;
            mano.EquipoA.Jugador3 = mano.Posicion5;

            mano.EquipoB.Id = "EquipoB";
            mano.EquipoB.Jugador1 = mano.Posicion2;
            mano.EquipoB.Jugador2 = mano.Posicion4;
            mano.EquipoB.Jugador3 = mano.Posicion6;

            mano.JugadoresActivos = new List<string> { "J1", "J2", "J3", "J4", "J5", "J6" };

            return mano;
        }
    }
}
