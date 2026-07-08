using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;
using Xunit;

namespace TrucoRPG.Tests
{
    public class AullidoServicioTests
    {
        private ManoTruco CrearManoBaseAullido()
        {
            return new ManoTruco
            {
                Configuracion = new()
                {
                    Modo = ModoJuego.Historia
                },
                GanadorMano = null,
                AullidoUsadoEnMano = false,
                AullidoBloqueando = false,
                Bazas = new List<Baza> { new Baza() }, 
                VistaHabilidadesRival = null
            };
        }


        [Fact]
        public void IntentarTrasPrimeraBaza_CuandoNoEsLobizonHistoria_DevuelveFalse()
        {
            // Given
            var mano = CrearManoBaseAullido();
            mano.Configuracion.Modo = ModoJuego.Tradicional; 

            // When
            var resultado = AullidoServicio.IntentarTrasPrimeraBaza(mano, "Humano");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IntentarTrasPrimeraBaza_CuandoElAullidoYaFueUsadoOBloquea_DevuelveFalse()
        {
            // Given
            var mano = CrearManoBaseAullido();
            mano.AullidoUsadoEnMano = true; 

            // When
            var resultado = AullidoServicio.IntentarTrasPrimeraBaza(mano, "Humano");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IntentarTrasPrimeraBaza_CuandoManoYaTieneGanador_DevuelveFalse()
        {
            // Given
            var mano = CrearManoBaseAullido();
            mano.GanadorMano = "Maquina"; 

            // When
            var resultado = AullidoServicio.IntentarTrasPrimeraBaza(mano, "Humano");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IntentarTrasPrimeraBaza_CuandoNoEsLaPrimeraBaza_DevuelveFalse()
        {
            // Given
            var mano = CrearManoBaseAullido();
            mano.Bazas = new List<Baza> { new Baza(), new Baza() };

            // When
            var resultado = AullidoServicio.IntentarTrasPrimeraBaza(mano, "Humano");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IntentarTrasPrimeraBaza_CuandoElGanadorNoEsElHumano_DevuelveFalse()
        {
            // Given
            var mano = CrearManoBaseAullido();
            string ganadorBaza = "Maquina"; 

            // When
            var resultado = AullidoServicio.IntentarTrasPrimeraBaza(mano, ganadorBaza);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IntentarTrasPrimeraBaza_CuandoSeCumplenCondicionesYAzarFavorece_ActivaAullidoYDevuelveTrue()
        {
            // Given
            var mano = new ManoTruco
            {
                Configuracion = new()
                {
                    Modo = ModoJuego.Historia,
                    RivalDeLaMaquina = ClaseRival.Lobizon
                },
                GanadorMano = null,
                AullidoUsadoEnMano = false,
                AullidoBloqueando = false,
                Bazas = new List<Baza> { new Baza() } 
            };

            string ganadorBaza = "Humano"; 
            bool logroActivarAullido = false;
            int maxIntentos = 1000;

            // When
            for (int i = 0; i < maxIntentos; i++)
            {
                mano.AullidoBloqueando = false;
                mano.AullidoUsadoEnMano = false;
                mano.UltimoMensajeHabilidadRival = null;

                var resultado = AullidoServicio.IntentarTrasPrimeraBaza(mano, ganadorBaza);

                if (resultado)
                {
                    logroActivarAullido = true;
                    break; 
                }
            }

            // Then
            Assert.True(mano.AullidoBloqueando);
            Assert.True(mano.AullidoUsadoEnMano);
            Assert.Equal("¡Aullido! El Lobizón te asustó. Te vas al mazo...", mano.UltimoMensajeHabilidadRival);
        }

        [Fact]
        public void ActualizarVista_AlSerInvocado_AsignaLaVistaDeHabilidadesAlRival()
        {
            // Given
            var mano = CrearManoBaseAullido();
            mano.VistaHabilidadesRival = null;

            // When
            HabilidadesRivalOrquestador.ActualizarVista(mano);

            // Then
            Assert.NotNull(mano.VistaHabilidadesRival);
        }

        private ManoTruco CrearManoBaseMazo()
        {
            return new ManoTruco
            {
                EnvidoPendienteRespuestaHumano = true,
                TrucoPendienteRespuestaHumano = true,
                CartaHumanoEnMesa = new Carta(),
                CartaMaquinaEnMesa = new Carta(),
                GanadorMano = null,
                TrucoResuelto = false,
                AullidoBloqueando = true,
                PuntosMaquina = 0,
                CantorTruco = "Humano"
            };
        }

        [Fact]
        public void EjecutarIrAlMazo_CuandoTrucoNoFueCantado_AsignaUnPuntoALaMaquinaYResuelveMano()
        {
            // Given
            var mano = CrearManoBaseMazo();
            mano.TrucoCantado = false;
            mano.PuntosTrucoMano = 1;

            // When
            AullidoServicio.EjecutarIrAlMazo(mano);

            // Then
            Assert.False(mano.EnvidoPendienteRespuestaHumano);
            Assert.False(mano.TrucoPendienteRespuestaHumano);
            Assert.Null(mano.CartaHumanoEnMesa);
        }

        [Fact]
        public void EjecutarIrAlMazo_CuandoTrucoFueCantado_AsignaLosPuntosDelTrucoALaMaquina()
        {
            // Given
            var mano = CrearManoBaseMazo();
            mano.TrucoCantado = true;
            mano.PuntosTrucoMano = 3;

            // When
            AullidoServicio.EjecutarIrAlMazo(mano);

            // Then
            Assert.False(mano.EnvidoPendienteRespuestaHumano);
            Assert.Null(mano.CartaHumanoEnMesa);
            Assert.True(mano.PuntosMaquina > 0);
        }
    }
}
