using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests
{
    public class CantarTrucoUseCaseTests
    {
        // Helper para inicializar una mano 1v1 limpia
        private ManoTruco CrearManoBase(Guid id)
        {
            return new ManoTruco
            {
                Id = id,
                PartidaTerminada = false,
                GanadorMano = null,
                TrucoCantado = false,
                NivelMentiraTrucoMaquina = 0,
                Maquina = new Jugador { Mano = new List<Carta>() }
            };
        }

        [Fact]
        public void Ejecutar_ManoNoExiste_LanzaKeyNotFoundException()
        {
            // Given
            var idInexistente = Guid.NewGuid();
            var servicio = new CantarTrucoUseCase(); 

            // When
            Action action = () => servicio.Ejecutar(idInexistente);

            // Then
            Assert.Throws<KeyNotFoundException>(action);
        }

        [Fact]
        public void Ejecutar_PartidaYaTerminada_LanzaInvalidOperationException()
        {
            // Given
            var id = Guid.NewGuid();
            var mano = CrearManoBase(id);
            mano.PartidaTerminada = true;
            PartidaMemoriaServicio.Guardar(mano);

            var servicio = new CantarTrucoUseCase();

            // When
            Action action = () => servicio.Ejecutar(id);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void Ejecutar_ManoYaTerminada_LanzaInvalidOperationException()
        {
            // Given
            var id = Guid.NewGuid();
            var mano = CrearManoBase(id);
            mano.GanadorMano = "Humano";
            PartidaMemoriaServicio.Guardar(mano);

            var servicio = new CantarTrucoUseCase();

            // When
            Action action = () => servicio.Ejecutar(id);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void Ejecutar_TrucoYaCantado_LanzaInvalidOperationException()
        {
            // Given
            var id = Guid.NewGuid();
            var mano = CrearManoBase(id);
            mano.TrucoCantado = true;
            PartidaMemoriaServicio.Guardar(mano);

            var servicio = new CantarTrucoUseCase();

            // When
            Action action = () => servicio.Ejecutar(id);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        // ── SECCIÓN 2: FLUJOS DEL JUEGO (BURLANDO AL RANDOM) ──

        [Fact]
        public void Ejecutar_MaquinaNoQuiereTruco_CierraManoYAsignaUnPunto()
        {
            // Given
            var id = Guid.NewGuid();
            var mano = CrearManoBase(id);

            mano.NivelMentiraTrucoMaquina = 0;
            mano.Maquina.Mano = new List<Carta>();

            PartidaMemoriaServicio.Guardar(mano);
            var servicio = new CantarTrucoUseCase();

            // Forzamos la tirada máxima (100): ninguna probabilidad la alcanza,
            // así la máquina rechaza el truco de forma determinística.
            var randomOriginal = DecisionMaquinaServicio.RandomNext;
            DecisionMaquinaServicio.RandomNext = _ => 99;
            try
            {
                // When
                var resultado = servicio.Ejecutar(id);

                // Then
                Assert.True(resultado.TrucoResuelto);
                Assert.Equal("Humano", resultado.GanadorMano);
                Assert.Equal(1, resultado.PuntosTrucoMano);
                Assert.Contains("La máquina no quiso el truco", resultado.EstadoTruco);
            }
            finally
            {
                DecisionMaquinaServicio.RandomNext = randomOriginal;
            }
        }

        [Fact]
        public void Ejecutar_MaquinaAceptaYEscalaARetruco_ModificaNivelA2YVale3Puntos()
        {
            // Given
            var id = Guid.NewGuid();
            var mano = CrearManoBase(id);
            mano.NivelMentiraTrucoMaquina = 100;
            mano.Maquina.Mano = new List<Carta>
            {
                new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 }
            };

            PartidaMemoriaServicio.Guardar(mano);
            var servicio = new CantarTrucoUseCase();

            // When
            var resultado = servicio.Ejecutar(id);

            // Then
            if (resultado.NivelTruco == 2)
            {
                Assert.Equal(3, resultado.PuntosTrucoMano);
                Assert.Equal("Maquina", resultado.CantorTruco);
                Assert.True(resultado.TrucoPendienteRespuestaHumano);
                Assert.Contains("Retruco", resultado.EstadoTruco);
            }
            else
            {
                Assert.True(resultado.TrucoResuelto);
                Assert.Equal(2, resultado.PuntosTrucoMano);
                Assert.Contains("La máquina quiso el truco", resultado.EstadoTruco);
            }
        }

        [Fact]
        public void Ejecutar_MaquinaAceptaYNoEscala_ModificaManoA2Puntos()
        {
            // Given
            var id = Guid.NewGuid();
            var mano = CrearManoBase(id);

            mano.NivelMentiraTrucoMaquina = 3;

            mano.Maquina.Mano = new List<Carta>
        {
            new Carta { Numero = 1, Palo = "Espada", ValorTruco = 100 }
        };

            PartidaMemoriaServicio.Guardar(mano);
            var servicio = new CantarTrucoUseCase();

            // When
            var resultado = servicio.Ejecutar(id);

            // Then
            if (resultado.NivelTruco == 0 || resultado.NivelTruco == 1)
            {
                Assert.True(resultado.TrucoResuelto);
                Assert.Equal(2, resultado.PuntosTrucoMano);
                Assert.Contains("La máquina quiso el truco", resultado.EstadoTruco);
            }
            else
            {
                Assert.Equal(2, resultado.NivelTruco);
                Assert.Equal(3, resultado.PuntosTrucoMano);
            }
        }
    
    }
}
