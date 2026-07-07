using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class JuegoServic3vs3Test
    {
        private Equipo3v3 CrearEquipoA()
        {
            var j1 = new Jugador { Id = "J1" };
            var j3 = new Jugador { Id = "J3" };
            var j5 = new Jugador { Id = "J5" };

            return new Equipo3v3
            {
                Id = "EquipoA",
                Jugador1 = j1,
                Jugador2 = j3,
                Jugador3 = j5
            };
        }


        private Equipo3v3 CrearEquipoB()
        {
            var j2 = new Jugador { Id = "J2" };
            var j4 = new Jugador { Id = "J4" };
            var j6 = new Jugador { Id = "J6" };

            return new Equipo3v3
            {
                Id = "EquipoB",
                Jugador1 = j2,
                Jugador2 = j4,
                Jugador3 = j6
            };
        }

        //resolver vuelta + obtener mejor valor ... 
        [Fact]
        public void ResolverVuelta_CartaEquipoAMasAlta_GanaEquipoA()
        {
            // Given
            var vuelta = new Vuelta3v3();

            vuelta.CartasJugadas = new Dictionary<string, Carta>
            {
                { "J1", new Carta {ValorTruco = 14 }},
                { "J2", new Carta{ValorTruco = 7}}
            };

            var equipoA = CrearEquipoA();
            var equipoB = CrearEquipoB();

            // When
            var resultado = JuegoServicio3v3.ResolverVuelta(vuelta, equipoA, equipoB);

            // Then
            Assert.Equal("EquipoA", resultado);
        }

        [Fact]
        public void ResolverVuelta_CartaEquipoAMasAlta_GanaEquipoB()
        {
            // Given
            var vuelta = new Vuelta3v3();

            vuelta.CartasJugadas = new Dictionary<string, Carta>
            {
                { "J1", new Carta {ValorTruco = 7 }},
                { "J2", new Carta{ValorTruco = 14}}
            };

            var equipoA = CrearEquipoA();
            var equipoB = CrearEquipoB();

            // When
            var resultado = JuegoServicio3v3.ResolverVuelta(vuelta, equipoA, equipoB);

            // Then
            Assert.Equal("EquipoB", resultado);
        }

        [Fact]
        public void ResolverVuelta_CartaEquipoAMasAlta_Empate()
        {
            // Given
            var vuelta = new Vuelta3v3();

            vuelta.CartasJugadas = new Dictionary<string, Carta>
            {
                { "J1", new Carta {ValorTruco = 10 }},
                { "J2", new Carta{ValorTruco = 10}}
            };

            var equipoA = CrearEquipoA();
            var equipoB = CrearEquipoB();

            // When
            var resultado = JuegoServicio3v3.ResolverVuelta(vuelta, equipoA, equipoB);

            // Then
            Assert.Equal("Parda", resultado);
        }

        [Fact]
        public void ResolverVuelta_GuardaLasMejoresCartas()
        {
            // Given
            var cartaFuerteA = new Carta { Numero = 1, ValorTruco = 14 };
            var cartaFuerteB = new Carta { Numero = 7, ValorTruco = 12 };

            var vuelta = new Vuelta3v3();
            vuelta.CartasJugadas = new Dictionary<string, Carta>
            {
                {"J1",cartaFuerteA},
                {"J2",cartaFuerteB}
            };

            var equipoA = CrearEquipoA();
            var equipoB = CrearEquipoB();


            // When
            JuegoServicio3v3.ResolverVuelta(vuelta, equipoA, equipoB);

            // Then
            Assert.Equal(cartaFuerteA, vuelta.MejorCartaEquipoA);
            Assert.Equal(cartaFuerteB, vuelta.MejorCartaEquipoB);
        }

        //resolver ganador
        [Fact]
        public void ResolverGanadorMano_SinVueltas_RetornaNull()
        {
            // Given
            var ganadores = new List<string>();

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoA");

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void ResolverGanadorMano_EquipoAGanaDosVueltas_RetornaEquipoA()
        {
            // Given
            var ganadores = new List<string>
            {
                "EquipoA",
                "EquipoA"
            };

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoB");

            // Then
            Assert.Equal("EquipoA", resultado);
        }

        [Fact]
        public void ResolverGanadorMano_EquipoAGanaDosVueltas_RetornaEquipoB()
        {
            // Given
            var ganadores = new List<string>
            {
                "EquipoB",
                "EquipoB"
            };

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoA");

            // Then
            Assert.Equal("EquipoB", resultado);
        }

        [Fact]
        public void ResolverGanadorMano_EquipoAParda_RetornaEquipoA()
        {
            // Given
            var ganadores = new List<string>
            {
                "EquipoA",
                "Parda"
            };

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoB");

            // Then
            Assert.Equal("EquipoA", resultado);
        }

        [Fact]
        public void ResolverGanadorMano_PrimeraA_SegundaB_TerceraA_RetornaA()
        {
            // Given
            var ganadores = new List<string>
            {
                "EquipoA",
                "EquipoB",
                "EquipoA"
            };

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoB");

            // Then
            Assert.Equal("EquipoA", resultado);
        }

        [Fact]
        public void ResolverGanadorMano_PrimeraA_SegundaB_SinTercera_RetornaNull()
        {
            // Given
            var ganadores = new List<string>
            {
                "EquipoA",
                "EquipoB"
            };

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoA");

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void ResolverGanadorMano_PrimeraParda_SegundaB_RetornaB()
        {
            // Given
            var ganadores = new List<string>
            {
                "Parda",
                "EquipoB"
            };

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoA");

            // Then
            Assert.Equal("EquipoB", resultado);
        }

        [Fact]
        public void ResolverGanadorMano_DosPardas_TerceraA_RetornaA()
        {
            // Given
            var ganadores = new List<string>
            {
                "Parda",
                "Parda",
                "EquipoA"
            };

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoB");

            // Then
            Assert.Equal("EquipoA", resultado);
        }

        [Fact]
        public void ResolverGanadorMano_TresPardas_RetornaEquipoMano()
        {
            // Given
            var ganadores = new List<string>
            {
                "Parda",
                "Parda",
                "Parda"
            };

            // When
            var resultado = JuegoServicio3v3.ResolverGanadorMano(ganadores, "EquipoB");

            // Then
            Assert.Equal("EquipoB", resultado);
        }

        // validar accion jugador
        [Fact]
        public void ValidarAccionJugador_ManoTerminada_LanzaExcepcion()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.ManoTerminada = true;

            // When
            var ex = Assert.Throws<InvalidOperationException>(() =>
                JuegoServicio3v3.ValidarAccionJugador(mano, "J1")
            );

            // Then
            Assert.Equal("La mano/partida ya terminó.", ex.Message);
        }

        [Fact]
        public void ValidarAccionJugador_PartidaTerminada_LanzaExcepcion()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.PartidaTerminada = true;

            // When
            var ex = Assert.Throws<InvalidOperationException>(() =>
                JuegoServicio3v3.ValidarAccionJugador(mano, "J1")
            );

            // Then
            Assert.Equal("La mano/partida ya terminó.", ex.Message);
        }

        [Fact]
        public void ValidarAccionJugador_TrucoPendiente_LanzaExcepcion()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.TurnoActual = "J1";
            mano.TrucoPendienteRespuestaDe = "J1";

            // When
            var ex = Assert.Throws<InvalidOperationException>(() =>
                JuegoServicio3v3.ValidarAccionJugador(mano, "J1")
            );

            // Then
            Assert.Equal("Debés responder el truco antes de jugar.", ex.Message);
        }

        [Fact]
        public void ValidarAccionJugador_EnvidoPendiente_LanzaExcepcion()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.TurnoActual = "J1";
            mano.EnvidoPendienteRespuestaDe = "J1";

            // When
            var ex = Assert.Throws<InvalidOperationException>(() =>
                JuegoServicio3v3.ValidarAccionJugador(mano, "J1")
            );

            // Then
            Assert.Equal("Debés responder el envido antes de jugar.", ex.Message);
        }

        [Fact]
        public void ValidarAccionJugador_NoEsSuTurno_LanzaExcepcion()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.TurnoActual = "J2";

            // When
            var ex = Assert.Throws<InvalidOperationException>(() =>
                JuegoServicio3v3.ValidarAccionJugador(mano, "J1")
            );

            // Then
            Assert.Equal("No es tu turno.", ex.Message);
        }

        [Fact]
        public void ValidarAccionJugador_TodoCorrecto_NoLanzaExcepcion()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.TurnoActual = "J1";

            // When
            var ex = Record.Exception(() =>
                JuegoServicio3v3.ValidarAccionJugador(mano, "J1")
            );

            // Then
            Assert.Null(ex);
        }

        //sumar puntos + evaluar fin de partida
        [Fact]
        public void SumarPuntos_PuntosMenorOIgualCero_NoSuma()
        {
            // Given
            var mano = new ManoTruco3v3
            {
                PuntosEquipoA = 10,
                PuntosEquipoB = 5
            };

            // When
            JuegoServicio3v3.SumarPuntos(mano, "EquipoA", 0);

            // Then
            Assert.Equal(10, mano.PuntosEquipoA);
            Assert.Equal(5, mano.PuntosEquipoB);
        }

        [Fact]
        public void SumarPuntos_EquipoA_SumaCorrectamente()
        {
            // Given
            var mano = new ManoTruco3v3();

            // When
            JuegoServicio3v3.SumarPuntos(mano, "EquipoA", 3);

            // Then
            Assert.Equal(3, mano.PuntosEquipoA);
            Assert.Equal(0, mano.PuntosEquipoB);
        }

        [Fact]
        public void SumarPuntos_EquipoB_SumaCorrectamente()
        {
            // Given
            var mano = new ManoTruco3v3();

            // When
            JuegoServicio3v3.SumarPuntos(mano, "EquipoB", 4);

            // Then
            Assert.Equal(0, mano.PuntosEquipoA);
            Assert.Equal(4, mano.PuntosEquipoB);
        }

        [Fact]
        public void SumarPuntos_EquipoA_LlegaA30_TerminaPartida()
        {
            // Given
            var mano = new ManoTruco3v3
            {
                PuntosEquipoA = 28
            };

            // When
            JuegoServicio3v3.SumarPuntos(mano, "EquipoA", 2);

            // Then
            Assert.Equal(30, mano.PuntosEquipoA);
            Assert.True(mano.PartidaTerminada);
            Assert.Equal("EquipoA", mano.GanadorPartida);
        }

        [Fact]
        public void SumarPuntos_EquipoB_LlegaA30_TerminaPartida()
        {
            // Given
            var mano = new ManoTruco3v3
            {
                PuntosEquipoB = 29
            };

            // When
            JuegoServicio3v3.SumarPuntos(mano, "EquipoB", 1);

            // Then
            Assert.Equal(30, mano.PuntosEquipoB);
            Assert.True(mano.PartidaTerminada);
            Assert.Equal("EquipoB", mano.GanadorPartida);
        }

        [Fact]
        public void SumarPuntos_EquipoInexistente_NoSuma()
        {
            // Given
            var mano = new ManoTruco3v3();

            // When
            JuegoServicio3v3.SumarPuntos(mano, "EquipoC", 5);

            // Then
            Assert.Equal(0, mano.PuntosEquipoA);
            Assert.Equal(0, mano.PuntosEquipoB);
        }

        //jugar carta valor
        [Fact]
        public void JugarCartaPorValor_ManoYaGanada_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.GanadorMano = "EquipoA";

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 1, "Espada");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCartaPorValor_ManoTerminada_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.ManoTerminada = true;

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 1, "Espada");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCartaPorValor_PartidaTerminada_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.PartidaTerminada = true;

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 1, "Espada");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCartaPorValor_TrucoPendiente_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.TrucoPendienteRespuestaDe = "J1";

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 1, "Espada");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCartaPorValor_EnvidoPendiente_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.EnvidoPendienteRespuestaDe = "J1";

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 1, "Espada");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCartaPorValor_NoEsTurnoJugador_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3();
            mano.TurnoActual = "J2";

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 1, "Espada");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCartaPorValor_CartaNoExiste_RetornaFalse()
        {
            // Given
            var jugador = new Jugador { Id = "J1", Mano = new List<Carta>() };
            var mano = new ManoTruco3v3
            {
                Posicion1 = jugador,
                TurnoActual = "J1"
            };

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 7, "Oro");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCartaPorValor_CartaExiste_JuegaCartaYRetornaTrue()
        {
            // Given
            var carta = new Carta { Numero = 7, Palo = "Oro", ValorTruco = 10 };
            var jugador = new Jugador { Id = "J1", Mano = new List<Carta> { carta } };
            var mano = new ManoTruco3v3
            {
                Posicion1 = jugador,
                TurnoActual = "J1"
            };

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 7, "Oro");

            // Then
            Assert.True(resultado);
            Assert.Single(jugador.Jugadas);
            Assert.Empty(jugador.Mano);
        }

        [Fact]
        public void JugarCartaPorValor_PaloDiferenteMayuscula_JuegaCarta()
        {
            // Given
            var carta = new Carta { Numero = 1, Palo = "espada", ValorTruco = 14 };
            var jugador = new Jugador { Id = "J1", Mano = new List<Carta> { carta } };
            var mano = new ManoTruco3v3
            {
                Posicion1 = jugador,
                TurnoActual = "J1"
            };

            // When
            var resultado = JuegoServicio3v3.JugarCartaPorValor(mano, "J1", 1, "ESPADA");

            // Then
            Assert.True(resultado);
        }

        //jugar carta
        [Fact]
        public void JugarCarta_ManoYaGanada_RetornaTrue()
        {
            // Given
            var mano = new ManoTruco3v3 { GanadorMano = "EquipoA" };
            var carta = new Carta();

            // When
            var resultado = JuegoServicio3v3.JugarCarta(mano, "J1", carta);

            // Then
            Assert.True(resultado);
        }

        [Fact]
        public void JugarCarta_PartidaTerminada_RetornaTrue()
        {
            // Given
            var mano = new ManoTruco3v3 { PartidaTerminada = true };
            var carta = new Carta();

            // When
            var resultado = JuegoServicio3v3.JugarCarta(mano, "J1", carta);

            // Then
            Assert.True(resultado);
        }

        [Fact]
        public void JugarCarta_TrucoPendiente_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3
            {
                TrucoPendienteRespuestaDe = "J1",
                TurnoActual = "J1"
            };

            var carta = new Carta();

            // When
            var resultado = JuegoServicio3v3.JugarCarta(mano, "J1", carta);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_EnvidoPendiente_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3
            {
                EnvidoPendienteRespuestaDe = "J1",
                TurnoActual = "J1"
            };

            var carta = new Carta();

            // When
            var resultado = JuegoServicio3v3.JugarCarta(mano, "J1", carta);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_NoEsTurno_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3 { TurnoActual = "J2" };
            var carta = new Carta();

            // When
            var resultado = JuegoServicio3v3.JugarCarta(mano, "J1", carta);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_JugadorNoExiste_RetornaFalse()
        {
            // Given
            var mano = new ManoTruco3v3 { TurnoActual = "J1" };
            var carta = new Carta();

            // When
            var resultado = JuegoServicio3v3.JugarCarta(mano, "J1", carta);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CartaNoPerteneceJugador_RetornaFalse()
        {
            // Given
            var jugador = new Jugador { Id = "J1", Mano = new List<Carta>() };
            var mano = new ManoTruco3v3
            {
                Posicion1 = jugador,
                TurnoActual = "J1"
            };
            var carta = new Carta { Numero = 1, Palo = "Oro" };

            // When
            var resultado = JuegoServicio3v3.JugarCarta(mano, "J1", carta);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CartaValida_MueveCartaAJugadas()
        {
            // Given
            var carta = new Carta { Numero = 5, Palo = "Oro", ValorTruco = 10 };
            var jugador = new Jugador { Id = "J1", Mano = new List<Carta> { carta } };

            var mano = new ManoTruco3v3
            {
                Posicion1 = jugador,
                TurnoActual = "J1"
            };

            // When
            var resultado = JuegoServicio3v3.JugarCarta(mano, "J1", carta);

            // Then
            Assert.False(resultado);
            Assert.Empty(jugador.Mano);
            Assert.Single(jugador.Jugadas);
            Assert.NotNull(mano.VueltaActual);
        }


    }
}
