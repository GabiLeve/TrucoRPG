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
        [Fact]
        public void JugarCarta_CuandoHayEnvidoPendiente_DebeRetornarFalseYNoModificarNada()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Humano";
            estado.Mano.EnvidoPendienteRespuestaHumano = true; 

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 7, palo: "Espada");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoNoEsElTurnoDelJugador_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Maquina"; 

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 7, palo: "Espada"); // J1 intenta jugar

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoElJugadorNoTieneEsaCartaEnLaMano_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Humano";

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Ancho Falso");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoJ1AbreBaza_DeBeGuardarCartaPendienteYPasarTurnoAMaquina()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Humano";
            estado.Mano.CartaMaquinaEnMesa = null; 

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 7, palo: "Espada");

            // Then
            Assert.True(resultado);
            Assert.Empty(estado.Mano.Humano.Mano); 
            Assert.Equal("Maquina", estado.Mano.TurnoActual);
        }

        [Fact]
        public void JugarCarta_CuandoJ1CierraBaza_DebeLimpiarMesaYResolverBaza()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Humano";

            // J2 ya había jugado (Mesa con carta)
            var cartaJ2 = new Carta { Numero = 1, Palo = "Basto" };
            estado.Mano.CartaMaquinaEnMesa = cartaJ2;

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 7, palo: "Espada");

            // Then
            Assert.True(resultado);
            Assert.Null(estado.Mano.CartaMaquinaEnMesa); 
        }

        //responder envido
        [Fact]
        public void ResponderEnvido_SiEnvidoNoCantadoOResuelto_DevuelveFalse()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_SiEsJ1YNoTieneRespuestaPendiente_DevuelveFalse()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoPendienteRespuestaHumano = false; 

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_SiEsJ2YNoTieneRespuestaPendiente_DevuelveFalse()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.EnvidoPendienteRespuestaJ2 = false; 

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: false, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_SiNoSeAcepta_ResuelveEnvidoYAsignaPuntosAlCantor()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.EnvidoPendienteRespuestaHumano = true; 
            estado.Mano.CantorEnvido = "Maquina";
            estado.PuntosEnvidoNoQuiero = 2;

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: false);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal(2, estado.Mano.PuntosEnvido);
            Assert.Contains("No quiso", estado.Mano.EstadoEnvido);
            Assert.False(estado.Mano.EnvidoPendienteRespuestaHumano);
            Assert.False(estado.EnvidoPendienteRespuestaJ2);
        }

        [Fact]
        public void ResponderEnvido_SiSeAceptaYHumanoTieneMayorTanto_GanaHumano()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.EnvidoPendienteRespuestaJ2 = true; 
            estado.PuntosEnvidoEnJuego = 4;

            estado.Mano.Humano.Mano = new List<Carta> {
                new Carta { Numero = 7, Palo = "Espada" }, new Carta { Numero = 6, Palo = "Espada" }
            };
            estado.Mano.Maquina.Mano = new List<Carta> {
                new Carta { Numero = 4, Palo = "Copas" }
            }; 

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: false, aceptar: true);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal("Humano", estado.Mano.GanadorEnvido);
            Assert.Equal(4, estado.Mano.PuntosEnvido);
            Assert.Contains("Quiso", estado.Mano.EstadoEnvido);
        }

        [Fact]
        public void ResponderEnvido_SiEsFaltaEnvido_CalculaPuntosEspecialesDeFalta()
        {
            // Given
            var estado = CrearEstadoBase();

            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.EnvidoPendienteRespuestaJ2 = true;

            estado.Mano.TipoEnvidoCantado = "FaltaEnvido";
            estado.PuntosEnvidoEnJuego = 0;
            estado.Mano.PuntosHumano = 12;
            estado.Mano.PuntosMaquina = 10;
            estado.Mano.ManoIniciadaPor = "Maquina"; 
            estado.Mano.Humano.Mano = new List<Carta>();
            estado.Mano.Maquina.Mano = new List<Carta>();

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: false, aceptar: true);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.True(estado.Mano.PuntosEnvido > 0);
        }
        private static EstadoTrucoMulti1v1 CrearEstadoBase()
        {
            return new EstadoTrucoMulti1v1
            {
                CartaPendienteJ1 = null,
                EnvidoPendienteRespuestaJ2 = false,
                TrucoPendienteRespuestaJ2 = false,
                Mano = new ManoTruco
                {
                    GanadorMano = null,
                    PartidaTerminada = false,
                    EnvidoPendienteRespuestaHumano = false,
                    TrucoPendienteRespuestaHumano = false,
                    CartaMaquinaEnMesa = null,
                    Humano = new()
                    {
                        Mano = new List<Carta> { new Carta { Numero = 7, Palo = "Espada" } },
                        Jugadas = new List<Carta>()
                    },
                    Maquina = new()
                    {
                        Mano = new List<Carta> { new Carta { Numero = 6, Palo = "Copas" } },
                        Jugadas = new List<Carta>()
                    }
                }
            };
        }

    }
}
