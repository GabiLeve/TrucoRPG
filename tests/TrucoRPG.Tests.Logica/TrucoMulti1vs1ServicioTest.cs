using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class TrucoMulti1vs1ServicioTest
    {
        // iniciar nueva mano
        [Fact]
        public void IniciarNuevaMano_CuandoEsPrimeraPartida_DebeSetearNumeroDeManoEnUno()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1();

            //When
            TrucoMulti1v1Servicio.IniciarNuevaMano(estado, esPrimeraPartida: true);

            //Then
            Assert.Equal(1, estado.Mano.NumeroDeMano);
        }

        [Fact]
        public void IniciarNuevaMano_CuandoEsPrimeraPartida_DebeSetearPuntosDeJugadoresEnCero()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1();

            //When
            TrucoMulti1v1Servicio.IniciarNuevaMano(estado, esPrimeraPartida: true);

            //Then
            Assert.Equal(0, estado.Mano.PuntosHumano);
            Assert.Equal(0, estado.Mano.PuntosMaquina);
        }

        [Fact]
        public void IniciarNuevaMano_CuandoNoEsPrimeraPartida_DebeIncrementarElNumeroDeMano()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1();

            estado.Mano = new ManoTruco
            {
                NumeroDeMano = 2,
                PuntosHumano = 10,
                PuntosMaquina = 5,
                Humano = new Jugador(),
                Maquina = new Jugador()
            };

            //When
            TrucoMulti1v1Servicio.IniciarNuevaMano(estado, esPrimeraPartida: false);

            //Then
            Assert.Equal(3, estado.Mano.NumeroDeMano);
        }

        [Fact]
        public void IniciarNuevaMano_CuandoNoEsPrimeraPartida_DebeMantenerLosPuntosAnteriores()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1();

            estado.Mano = new ManoTruco
            {
                NumeroDeMano = 2,
                PuntosHumano = 12,
                PuntosMaquina = 9,
                Humano = new Jugador(),
                Maquina = new Jugador()
            };

            //When
            TrucoMulti1v1Servicio.IniciarNuevaMano(estado, esPrimeraPartida: false);

            //Then
            Assert.Equal(12, estado.Mano.PuntosHumano);
            Assert.Equal(9, estado.Mano.PuntosMaquina);
        }

        [Fact]
        public void IniciarNuevaMano_Siempre_DebeConfigurarNombresYValoresBaseDeLaMano()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1();

            //When
            TrucoMulti1v1Servicio.IniciarNuevaMano(estado, esPrimeraPartida: true);

            //Then
            Assert.Equal("Jugador 1", estado.Mano.Humano.Nombre);
            Assert.Equal("Jugador 2", estado.Mano.Maquina.Nombre);
            Assert.Equal(1, estado.Mano.PuntosTrucoMano);
        }

        [Fact]
        public void IniciarNuevaMano_Siempre_DebeLimpiarCartasPendientes()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1 { CartaPendienteJ1 = new Carta { Palo = "Espada", Numero = 7 } };

            //When
            TrucoMulti1v1Servicio.IniciarNuevaMano(estado, esPrimeraPartida: true);

            //Then
            Assert.Null(estado.CartaPendienteJ1);
        }

        [Fact]
        public void IniciarNuevaMano_Siempre_DebeSetearRespuestasPendientesEnFalso()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                TrucoPendienteRespuestaJ2 = true,
                EnvidoPendienteRespuestaJ2 = true
            };

            //When
            TrucoMulti1v1Servicio.IniciarNuevaMano(estado, esPrimeraPartida: true);

            //Then
            Assert.False(estado.TrucoPendienteRespuestaJ2);
            Assert.False(estado.EnvidoPendienteRespuestaJ2);
        }

        [Fact]
        public void IniciarNuevaMano_Siempre_DebeReiniciarValoresDePuntosDeEnvido()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                PuntosEnvidoEnJuego = 4,
                PuntosEnvidoNoQuiero = 2
            };

            //When
            TrucoMulti1v1Servicio.IniciarNuevaMano(estado, esPrimeraPartida: true);

            //Then
            Assert.Equal(0, estado.PuntosEnvidoEnJuego);
            Assert.Equal(1, estado.PuntosEnvidoNoQuiero);
        }

        //jugar carta
        [Fact]
        public void JugarCarta_CuandoManoOPartidaTerminada_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { GanadorMano = "Humano" } // Mano terminada
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Espada");

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoHayEnvidoPendiente_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoPendienteRespuestaHumano = true }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Espada");

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoHayTrucoPendiente_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco(),
                TrucoPendienteRespuestaJ2 = true
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Espada");

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoNoEsElTurnoDelJugador_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TurnoActual = "Maquina" }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Espada");

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoElJugadorNoTieneEsaCarta_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TurnoActual = "Humano" }
            };
            estado.Mano.Humano.Mano = new List<Carta> { new Carta { Numero = 7, Palo = "Espada" } };

            //When
            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Espada"); // No tiene el Ancho

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoJ1AbreBaza_DebeSetearCartaPendienteYCambiarTurnoAMaquina()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TurnoActual = "Humano" }
            };
            var carta = new Carta { Numero = 1, Palo = "Espada" };
            estado.Mano.Humano.Mano.Add(carta);
            estado.Mano.CartaMaquinaEnMesa = null;

            //When
            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Espada");

            //Then
            Assert.True(resultado);
            Assert.Empty(estado.Mano.Humano.Mano);
            Assert.Contains(carta, estado.Mano.Humano.Jugadas);
            Assert.Equal(carta, estado.CartaPendienteJ1);
            Assert.Equal("Maquina", estado.Mano.TurnoActual);
        }

        [Fact]
        public void JugarCarta_CuandoJ1RespondeAJ2_DebeResolverBazaYLimpiarMesa()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TurnoActual = "Humano" }
            };
            var cartaJ1 = new Carta { Numero = 1, Palo = "Espada" };
            var cartaJ2 = new Carta { Numero = 4, Palo = "Copa" };

            estado.Mano.Humano.Mano.Add(cartaJ1);
            estado.Mano.CartaMaquinaEnMesa = cartaJ2;

            //When
            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Espada");

            //Then
            Assert.True(resultado);
            Assert.Null(estado.Mano.CartaMaquinaEnMesa);
        }

        [Fact]
        public void JugarCarta_CuandoJ2AbreBaza_DebeSetearCartaEnMesaYCambiarTurnoAHumano()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TurnoActual = "Maquina" }
            };
            var carta = new Carta { Numero = 1, Palo = "Basto" };
            estado.Mano.Maquina.Mano.Add(carta);
            estado.CartaPendienteJ1 = null;
            //When

            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: false, numero: 1, palo: "Basto");

            //Then
            Assert.True(resultado);
            Assert.Empty(estado.Mano.Maquina.Mano);
            Assert.Contains(carta, estado.Mano.Maquina.Jugadas);
            Assert.Equal(carta, estado.Mano.CartaMaquinaEnMesa);
            Assert.Equal("Humano", estado.Mano.TurnoActual);
        }

        [Fact]
        public void JugarCarta_CuandoJ2RespondeAJ1_DebeResolverBazaYLimpiarCartaPendiente()
        {
            // Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TurnoActual = "Maquina" }
            };
            var cartaJ1 = new Carta { Numero = 1, Palo = "Espada" };
            var cartaJ2 = new Carta { Numero = 7, Palo = "Oro" };

            estado.Mano.Maquina.Mano.Add(cartaJ2);
            estado.CartaPendienteJ1 = cartaJ1;

            //When
            bool resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: false, numero: 7, palo: "Oro");

            //Then
            Assert.True(resultado);
            Assert.Null(estado.CartaPendienteJ1);
        }

        // cantar envido
        [Fact]
        public void CantarEnvido_CuandoYaFueCantadoOResuelto_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = true }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, esJ1: true, tipo: "Envido");

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarEnvido_CuandoYaHayBazasJugadas_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    Bazas = new List<Baza> { new Baza() } 
                }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, esJ1: true, tipo: "Envido");

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarEnvido_CuandoManoFinalizada_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { GanadorMano = "Maquina" } 
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, esJ1: true, tipo: "Envido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarEnvido_CuandoCantaJ1_DebeRegistrarApuestaYPasarRespuestaAJ2()
        {
            //Given
            var estado = CrearEstadoBaseParaEnvido();
            string tipoEsperado = "Envido";
            int puntosEsperados = 2;

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, esJ1: true, tipo: "envido");

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoCantado);
            Assert.Equal("Humano", estado.Mano.CantorEnvido);
            Assert.Equal(puntosEsperados, estado.PuntosEnvidoEnJuego);
            Assert.Equal(1, estado.PuntosEnvidoNoQuiero);
            Assert.False(estado.Mano.EnvidoPendienteRespuestaHumano); 
            Assert.True(estado.EnvidoPendienteRespuestaJ2);         
        }

        [Fact]
        public void CantarEnvido_CuandoCantaJ2_DebeRegistrarApuestaYPasarRespuestaAHumano()
        {
            //Given
            var estado = CrearEstadoBaseParaEnvido();

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, esJ1: false, tipo: "Real Envido");

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoCantado);
            Assert.Equal("Maquina", estado.Mano.CantorEnvido);
            Assert.True(estado.Mano.EnvidoPendienteRespuestaHumano); 
            Assert.False(estado.EnvidoPendienteRespuestaJ2); 
            Assert.Contains("cantó Real Envido", estado.Mano.EstadoEnvido);
        }

        // responder envido
        [Fact]
        public void ResponderEnvido_CuandoNoCorrespondeResponder_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = false, EnvidoResuelto = true }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_CuandoJ1IntentaResponderSinTenerTurno_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = true, EnvidoResuelto = false, EnvidoPendienteRespuestaHumano = false }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_CuandoJ2IntentaResponderSinTenerTurno_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = true, EnvidoResuelto = false },
                EnvidoPendienteRespuestaJ2 = false
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: false, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_CuandoNoSeAcepta_DebeDarPuntosAlCantorYResolver()
        {
            //Given
            var estado = CrearEstadoBaseParaResponder(humanoResponde: true);
            estado.Mano.CantorEnvido = "Maquina";
            estado.PuntosEnvidoNoQuiero = 2;

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: false);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal("Maquina", estado.Mano.GanadorEnvido);
            Assert.Equal(2, estado.Mano.PuntosEnvido);
            Assert.False(estado.Mano.EnvidoPendienteRespuestaHumano);
            Assert.False(estado.EnvidoPendienteRespuestaJ2);
        }

        [Fact]
        public void ResponderEnvido_QuizoHumanoTieneMejorMano_CuandoGanaHumanoPorTanto()
        {
            //Given
            var estado = CrearEstadoBaseParaResponder(humanoResponde: false); 
            estado.PuntosEnvidoEnJuego = 2;
            estado.Mano.Humano.Mano = GenerarManoConTanto(33);
            estado.Mano.Maquina.Mano = GenerarManoConTanto(20);

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: false, aceptar: true);

            //Then
            Assert.True(resultado);
            Assert.Equal("Humano", estado.Mano.GanadorEnvido);
            Assert.True(estado.Mano.EnvidoResuelto);
        }

        [Fact]
        public void ResponderEnvido_QuisoMaquinaTieneMejorMano_CuandoGanaMaquinaPorTanto()
        {
            //Given
            var estado = CrearEstadoBaseParaResponder(humanoResponde: true);
            estado.PuntosEnvidoEnJuego = 2;
            estado.Mano.Humano.Mano = GenerarManoConTanto(20);
            estado.Mano.Maquina.Mano = GenerarManoConTanto(33);

            estado.Mano.Maquina.Mano = new List<Carta>
            {
                new Carta { Numero = 7, Palo = "Espada" },
                new Carta { Numero = 6, Palo = "Espada" },
                new Carta { Numero = 1, Palo = "Basto" }
            };
            estado.Mano.Humano.Mano = new List<Carta>
            {
               new Carta { Numero = 7, Palo = "Oro" },
               new Carta { Numero = 5, Palo = "Copa" },
               new Carta { Numero = 2, Palo = "Basto" }
            };
            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.True(resultado);
            Assert.Equal("Maquina", estado.Mano.GanadorEnvido);
        }

        [Fact]
        public void ResponderEnvido_QuizoEmpatanTantos_DebeGanarQuienEsMano()
        {
            //Given
            var estado = CrearEstadoBaseParaResponder(humanoResponde: true);
            estado.PuntosEnvidoEnJuego = 2;
            estado.Mano.ManoIniciadaPor = "Humano"; 
            estado.Mano.Humano.Mano = GenerarManoConTanto(28);
            estado.Mano.Maquina.Mano = GenerarManoConTanto(28); 

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.True(resultado);
            Assert.Equal("Humano", estado.Mano.GanadorEnvido);
        }

        [Fact]
        public void ResponderEnvido_QuizoEsFaltaEnvido_DebeCalcularPuntosDeFalta()
        {
            //Given
            var estado = CrearEstadoBaseParaResponder(humanoResponde: true);
            estado.Mano.TipoEnvidoCantado = "FaltaEnvido";
            estado.Mano.PuntosHumano = 12;
            estado.Mano.PuntosMaquina = 18; 

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.PuntosEnvido > 0);
        }

        // son buenas
        [Fact]
        public void SonBuenas_CuandoNoHayEnvidoActivo_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = false, EnvidoResuelto = true }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.SonBuenas(estado, esJ1: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void SonBuenas_CuandoJ1IntentaDeclararSinTenerTurno_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = true, EnvidoResuelto = false, EnvidoPendienteRespuestaHumano = false }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.SonBuenas(estado, esJ1: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void SonBuenas_CuandoJ2IntentaDeclararSinTenerTurno_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = true, EnvidoResuelto = false },
                EnvidoPendienteRespuestaJ2 = false
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.SonBuenas(estado, esJ1: false);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void SonBuenas_CuandoSeDeclaran_DebeDarPuntosAlCantorYResolverEnvido()
        {
            //Given
            var estado = CrearEstadoBaseParaSonBuenas(humanoResponde: true);
            estado.Mano.CantorEnvido = "Maquina";
            estado.PuntosEnvidoEnJuego = 4; 

            //When
            bool resultado = TrucoMulti1v1Servicio.SonBuenas(estado, esJ1: true);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.SonBuenasDeclarado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal("Maquina", estado.Mano.GanadorEnvido);
            Assert.Equal(4, estado.Mano.PuntosEnvido);
        }

        [Fact]
        public void SonBuenas_CuandoEsFaltaEnvido_DebeCalcularPuntosDeFalta()
        {
            //Given
            var estado = CrearEstadoBaseParaSonBuenas(humanoResponde: false); 
            estado.Mano.CantorEnvido = "Humano";
            estado.Mano.TipoEnvidoCantado = "FaltaEnvido";
            estado.Mano.PuntosHumano = 10;
            estado.Mano.PuntosMaquina = 20;

            //When
            bool resultado = TrucoMulti1v1Servicio.SonBuenas(estado, esJ1: false);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal("Humano", estado.Mano.GanadorEnvido);
        }
        //escalar envido

        [Fact]
        public void EscalarEnvido_CuandoNoHayEnvidoActivo_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = false, EnvidoResuelto = true }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado, esJ1: true, tipo: "RealEnvido");

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarEnvido_CuandoElMismoCantorIntentaEscalar_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = true, EnvidoResuelto = false, CantorEnvido = "Humano" }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado, esJ1: true, tipo: "RealEnvido"); 

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarEnvido_CuandoElTipoNuevoEsMenorOIgualAlActual_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { EnvidoCantado = true, EnvidoResuelto = false, CantorEnvido = "Maquina", TipoEnvidoCantado = "RealEnvido" }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado, esJ1: true, tipo: "Envido");

            //Then
            Assert.False(resultado);
        }
        [Fact]
        public void EscalarEnvido_CuandoEsUnEscaladoNormal_DebeAcumularPuntosEIncrementarJuego()
        {
            //Given
            var estado = CrearEstadoBaseParaEscalar(cantorActual: "Maquina", tipoActual: "Envido");
            estado.PuntosEnvidoEnJuego = 2;
            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado, esJ1: true, tipo: "RealEnvido");

            //Then
            Assert.True(resultado);
            Assert.Equal("RealEnvido", estado.Mano.TipoEnvidoCantado);
            Assert.Equal("Humano", estado.Mano.CantorEnvido);
        }

        [Fact]
        public void EscalarEnvido_CuandoSeEscalaAFaltaEnvido_DebeSetearPuntosEnJuegoEnCero()
        {
            //Given
            var estado = CrearEstadoBaseParaEscalar(cantorActual: "Humano", tipoActual: "RealEnvido");
            estado.PuntosEnvidoEnJuego = 5;

            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado, esJ1: false, tipo: "FaltaEnvido"); 

            //Then
            Assert.True(resultado);
            Assert.Equal("FaltaEnvido", estado.Mano.TipoEnvidoCantado);
            Assert.Equal("Maquina", estado.Mano.CantorEnvido);
            Assert.Equal(5, estado.PuntosEnvidoNoQuiero); 
            Assert.Equal(0, estado.PuntosEnvidoEnJuego);  
        }

        // cantar truco
        [Fact]
        public void CantarTruco_CuandoYaFueCantadoOTerminada_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TrucoCantado = true, GanadorMano = null }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarTruco(estado, esJ1: true);

            //Then
            Assert.False(resultado);
        }
        [Fact]
        public void CantarTruco_CuandoPartidaTerminada_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TrucoCantado = false, PartidaTerminada = true }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarTruco(estado, esJ1: true);

            //Then
            Assert.False(resultado);
        }
        [Fact]
        public void CantarTruco_CuandoCantaJ1_DebeSetearValoresBaseYPasarRespuestaAJ2()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    TrucoCantado = false,
                    GanadorMano = null,
                    PartidaTerminada = false
                }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarTruco(estado, esJ1: true);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.TrucoCantado);
            Assert.Equal(1, estado.Mano.NivelTruco);
            Assert.Equal(2, estado.Mano.PuntosTrucoMano); 
            Assert.Equal("Humano", estado.Mano.CantorTruco);       
        }

        [Fact]
        public void CantarTruco_CuandoCantaJ2_DebeSetearValoresBaseYPasarRespuestaAHumano()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    TrucoCantado = false,
                    GanadorMano = null,
                    PartidaTerminada = false
                }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.CantarTruco(estado, esJ1: false);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.TrucoCantado);
            Assert.Equal("Maquina", estado.Mano.CantorTruco);
            Assert.True(estado.Mano.TrucoPendienteRespuestaHumano); 
            Assert.False(estado.TrucoPendienteRespuestaJ2);
            Assert.Contains("cantó Truco", estado.Mano.EstadoTruco);
        }

        //responder truco
        [Fact]
        public void ResponderTruco_CuandoJ1IntentaResponderSinTenerTurno_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TrucoPendienteRespuestaHumano = false }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, esJ1: true, aceptar: true, escalarA: null);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderTruco_CuandoJ2IntentaResponderSinTenerTurno_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco(),
                TrucoPendienteRespuestaJ2 = false
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, esJ1: false, aceptar: true, escalarA: null);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderTruco_CuandoNoSeAcepta_DebeDarPuntosAlCantorYResolverMano()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    TrucoResuelto = false,
                    TrucoPendienteRespuestaHumano = true,
                    Humano = new Jugador(),
                    Maquina = new Jugador()
                },
                EnvidoPendienteRespuestaJ2 = false,
                TrucoPendienteRespuestaJ2 = false
            };
            estado.Mano.NivelTruco = 1; 
            estado.Mano.CantorTruco = "Maquina";

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, esJ1: true, aceptar: false, escalarA: null);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.TrucoResuelto);
            Assert.Equal("Maquina", estado.Mano.GanadorMano);
            Assert.Equal(1, estado.Mano.PuntosTrucoMano);
            Assert.False(estado.Mano.TrucoPendienteRespuestaHumano);
            Assert.False(estado.TrucoPendienteRespuestaJ2);
        }

        [Fact]
        public void ResponderTruco_CuandoSeAceptaSinEscalar_DebeMarcarComoResuelto()
        {
            // Given
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    TrucoResuelto = false,
                    TrucoPendienteRespuestaHumano = false,
                    Humano = new Jugador(),
                    Maquina = new Jugador()
                },
                EnvidoPendienteRespuestaJ2 = false,
                TrucoPendienteRespuestaJ2 = true
            };
            estado.Mano.NivelTruco = 1;
            estado.Mano.PuntosTrucoMano = 2;

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, esJ1: false, aceptar: true, escalarA: null);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.TrucoResuelto);
            Assert.Equal(2, estado.Mano.PuntosTrucoMano); 
            Assert.Contains("Quiso.", estado.Mano.EstadoTruco);
        }

        [Fact]
        public void ResponderTruco_CuandoJ1EscalaARetruco_DebeSubirNivelYPasarRespuestaAJ2()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    TrucoResuelto = false,
                    TrucoPendienteRespuestaHumano = true,
                    Humano = new Jugador(),
                    Maquina = new Jugador()
                },
                EnvidoPendienteRespuestaJ2 = false,
                TrucoPendienteRespuestaJ2 = false
            };
            estado.Mano.NivelTruco = 1; 

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, esJ1: true, aceptar: true, escalarA: "retruco");

            //Then
            Assert.True(resultado);
            Assert.False(estado.Mano.TrucoResuelto); 
            Assert.Equal(2, estado.Mano.NivelTruco); 
            Assert.Equal(3, estado.Mano.PuntosTrucoMano); 
            Assert.Equal("Humano", estado.Mano.CantorTruco);
            Assert.Contains("cantó Retruco!", estado.Mano.EstadoTruco);
        }

        [Fact]
        public void ResponderTruco_CuandoJ2EscalaAValeCuatro_DebeSubirNivelYPasarRespuestaAHumano()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    TrucoResuelto = false,
                    TrucoPendienteRespuestaHumano = false,
                    Humano = new Jugador(),
                    Maquina = new Jugador()
                },
                EnvidoPendienteRespuestaJ2 = false,
                TrucoPendienteRespuestaJ2 = true

            };
            estado.Mano.NivelTruco = 2; 

            //When
            bool resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, esJ1: false, aceptar: true, escalarA: "valecuatro");

            //Then
            Assert.True(resultado);
            Assert.Equal(3, estado.Mano.NivelTruco);
            Assert.Equal(4, estado.Mano.PuntosTrucoMano); 
            Assert.Equal("Maquina", estado.Mano.CantorTruco);
            Assert.True(estado.Mano.TrucoPendienteRespuestaHumano);
            Assert.Contains("cantó Vale Cuatro!", estado.Mano.EstadoTruco);
        }

        //escalar truco
        [Fact]
        public void EscalarTruco_CuandoNoEstaCantadoOMaximoNivel_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TrucoCantado = false, NivelTruco = 3 }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_CuandoHayRespuestaPendiente_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TrucoCantado = true, NivelTruco = 1, TrucoPendienteRespuestaHumano = true }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_CuandoManoOPartidaTerminada_DebeRetornarFalse()
        {
            //Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TrucoCantado = true, NivelTruco = 1, GanadorMano = "Humano" }
            };

            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_CuandoElMismoCantorIntentaEscalar_DebeRetornarFalse()
        {
            // Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { TrucoCantado = true, NivelTruco = 1, CantorTruco = "Humano" }
            };

            // When
            bool resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true); 

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_CuandoJ1EscalaARetruco_DebeConfigurarTresPuntosYPasarRespuestaAJ2()
        {
            //Given
            var estado = CrearEstadoBaseParaEscalarTruco(cantorActual: "Maquina", nivelActual: 1);

            //When
            bool resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            //Then
            Assert.True(resultado);
            Assert.Equal(2, estado.Mano.NivelTruco); 
            Assert.Equal(3, estado.Mano.PuntosTrucoMano); 
            Assert.False(estado.Mano.TrucoResuelto); 
            Assert.Equal("Humano", estado.Mano.CantorTruco);
            Assert.Contains("cantó Retruco!", estado.Mano.EstadoTruco);
        }

        [Fact]
        public void EscalarTruco_CuandoJ2EscalaAValeCuatro_DebeConfigurarCuatroPuntosYPasarRespuestaAHumano()
        {
            // Given
            var estado = CrearEstadoBaseParaEscalarTruco(cantorActual: "Humano", nivelActual: 2);

            // When
            bool resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: false); 

            // Then
            Assert.True(resultado);
            Assert.Equal(3, estado.Mano.NivelTruco); 
            Assert.Equal(4, estado.Mano.PuntosTrucoMano); 
            Assert.Equal("Maquina", estado.Mano.CantorTruco);
            Assert.False(estado.TrucoPendienteRespuestaJ2);
            Assert.Contains("cantó Vale Cuatro!", estado.Mano.EstadoTruco);
        }

        //irse al mazo
        [Fact]
        public void IrseAlMazo_CuandoManoOPartidaTerminada_DebeRetornarFalse()
        {
            // Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco { GanadorMano = "Maquina", PartidaTerminada = false }
            };

            // When
            bool resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }
        [Fact]
        public void IrseAlMazo_CuandoNoHayTrucoCantado_DebeDarUnPuntoAlRival()
        {
            // Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    GanadorMano = null,
                    PartidaTerminada = false,
                    TrucoCantado = false,
                    TrucoResuelto = false
                }
            };
            estado.Mano.TrucoCantado = false;

            // When
            bool resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true); 

            // Then
            Assert.True(resultado);
            Assert.Equal("Maquina", estado.Mano.GanadorMano); 
            Assert.Equal(1, estado.Mano.PuntosTrucoMano);     
        }

        [Fact]
        public void IrseAlMazo_CuandoHayCantoSinResponder_DebeDarPuntosSegunNivelTruco()
        {
            // Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    GanadorMano = null,
                    PartidaTerminada = false,
                    TrucoCantado = false,
                    TrucoResuelto = false
                }
            };
            estado.Mano.TrucoCantado = true;
            estado.Mano.PuntosTrucoMano = 2;
            estado.Mano.NivelTruco = 2; 
            estado.TrucoPendienteRespuestaJ2 = true; 

            // When
            bool resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.True(resultado);
            Assert.Equal("Maquina", estado.Mano.GanadorMano);
            Assert.Equal(2, estado.Mano.PuntosTrucoMano); 
        }

        [Fact]
        public void IrseAlMazo_CuandoTrucoYaEstaQuerido_DebeDarLosPuntosApostados()
        {
            // Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    GanadorMano = null,
                    PartidaTerminada = false,
                    TrucoCantado = false,
                    TrucoResuelto = false
                }
            };
            estado.Mano.TrucoCantado = true;
            estado.Mano.PuntosTrucoMano = 3; 
            estado.Mano.TrucoPendienteRespuestaHumano = false;
            estado.TrucoPendienteRespuestaJ2 = false; 

            // When
            bool resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: false);

            // Then
            Assert.True(resultado);
            Assert.Equal("Humano", estado.Mano.GanadorMano); 
            Assert.Equal(3, estado.Mano.PuntosTrucoMano);  
        }

        [Fact]
        public void IrseAlMazo_Siempre_DebeLimpiarTodosLosFlagsDeRespuestaPendientes()
        {
            // Given
            var estado = new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    GanadorMano = null,
                    PartidaTerminada = false,
                    TrucoCantado = false,
                    TrucoResuelto = false
                }
            };
            estado.Mano.TrucoPendienteRespuestaHumano = true;
            estado.TrucoPendienteRespuestaJ2 = true;
            estado.Mano.EnvidoPendienteRespuestaHumano = true;
            estado.EnvidoPendienteRespuestaJ2 = true;

            // When
            TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.True(estado.Mano.TrucoResuelto);
            Assert.False(estado.Mano.TrucoPendienteRespuestaHumano);
            Assert.False(estado.TrucoPendienteRespuestaJ2);
            Assert.False(estado.Mano.EnvidoPendienteRespuestaHumano);
            Assert.False(estado.EnvidoPendienteRespuestaJ2);
        }
        private static EstadoTrucoMulti1v1 CrearEstadoBaseParaEscalarTruco(string cantorActual, int nivelActual)
        {
            return new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    TrucoCantado = true,
                    NivelTruco = nivelActual,
                    CantorTruco = cantorActual,
                    TrucoResuelto = true, 
                    GanadorMano = null,
                    PartidaTerminada = false,
                    TrucoPendienteRespuestaHumano = false
                },
                TrucoPendienteRespuestaJ2 = false
            };
        }
        private static EstadoTrucoMulti1v1 CrearEstadoBaseParaEscalar(string cantorActual, string tipoActual)
        {
            return new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    EnvidoCantado = true,
                    EnvidoResuelto = false,
                    CantorEnvido = cantorActual,
                    TipoEnvidoCantado = tipoActual
                }
            };
        }
        private static EstadoTrucoMulti1v1 CrearEstadoBaseParaSonBuenas(bool humanoResponde)
        {
            return new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    EnvidoCantado = true,
                    EnvidoResuelto = false,
                    SonBuenasDeclarado = false,
                    EnvidoPendienteRespuestaHumano = humanoResponde,
                    Humano = new Jugador(),
                    Maquina = new Jugador()
                },
                EnvidoPendienteRespuestaJ2 = !humanoResponde
            };
        }
        private static List<Carta> GenerarManoConTanto(int tantoObjetivo)
        {
            return new List<Carta> { new Carta { Numero = 1, Palo = "Espada" } };
        }
        private static EstadoTrucoMulti1v1 CrearEstadoBaseParaResponder(bool humanoResponde)
        {
            return new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    EnvidoCantado = true,
                    EnvidoResuelto = false,
                    EnvidoPendienteRespuestaHumano = humanoResponde,
                    Humano = new Jugador { Mano = new List<Carta>(), Jugadas = new List<Carta>() },
                    Maquina = new Jugador { Mano = new List<Carta>(), Jugadas = new List<Carta>() }
                },
                EnvidoPendienteRespuestaJ2 = !humanoResponde
            };
        }

        private static EstadoTrucoMulti1v1 CrearEstadoBaseParaEnvido()
        {
            return new EstadoTrucoMulti1v1
            {
                Mano = new ManoTruco
                {
                    EnvidoCantado = false,
                    EnvidoResuelto = false,
                    PartidaTerminada = false,
                    GanadorMano = null,
                    Bazas = new List<Baza>()
                }
            };
        }
    }
}
