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
