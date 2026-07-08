using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Tests de EscalarTrucoUseCase: validaciones y los tres desenlaces posibles
    /// (máquina no quiere, máquina acepta y contra-escala, máquina solo acepta).
    /// </summary>
    public class EscalarTrucoUseCaseTest : IDisposable
    {
        private readonly Func<int, int> _randomOriginal = DecisionMaquinaServicio.RandomNext;

        public void Dispose()
        {
            DecisionMaquinaServicio.RandomNext = _randomOriginal;
        }

        private ManoTruco CrearManoEscalable(Guid id, int nivelTruco = 1)
        {
            var mano = new ManoTruco
            {
                Id = id,
                TrucoCantado = true,
                TrucoResuelto = false,
                NivelTruco = nivelTruco,
                CantorTruco = "Maquina",
                Maquina = new Jugador { Mano = new List<Carta> { new Carta { ValorTruco = 14 } } }
            };
            PartidaMemoriaServicio.Guardar(mano);
            return mano;
        }

        // ── Validaciones ─────────────────────────────────────────────────

        [Fact]
        public void Ejecutar_ManoInexistente_LanzaKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => new EscalarTrucoUseCase().Ejecutar(Guid.NewGuid()));
        }

        [Fact]
        public void Ejecutar_PartidaTerminada_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id);
            mano.PartidaTerminada = true;

            var ex = Assert.Throws<InvalidOperationException>(() => new EscalarTrucoUseCase().Ejecutar(id));
            Assert.Contains("ya terminó", ex.Message);
        }

        [Fact]
        public void Ejecutar_ManoTerminada_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id);
            mano.GanadorMano = "Humano";

            Assert.Throws<InvalidOperationException>(() => new EscalarTrucoUseCase().Ejecutar(id));
        }

        [Fact]
        public void Ejecutar_SinTrucoCantado_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id);
            mano.TrucoCantado = false;

            var ex = Assert.Throws<InvalidOperationException>(() => new EscalarTrucoUseCase().Ejecutar(id));
            Assert.Contains("No hay truco activo", ex.Message);
        }

        [Fact]
        public void Ejecutar_TrucoYaResuelto_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id);
            mano.TrucoResuelto = true;

            var ex = Assert.Throws<InvalidOperationException>(() => new EscalarTrucoUseCase().Ejecutar(id));
            Assert.Contains("No hay truco activo", ex.Message);
        }

        [Fact]
        public void Ejecutar_ConTrucoPendienteDeRespuesta_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id);
            mano.TrucoPendienteRespuestaHumano = true;

            var ex = Assert.Throws<InvalidOperationException>(() => new EscalarTrucoUseCase().Ejecutar(id));
            Assert.Contains("pendiente", ex.Message);
        }

        [Fact]
        public void Ejecutar_ConEnvidoPendienteDeRespuesta_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id);
            mano.EnvidoPendienteRespuestaHumano = true;

            var ex = Assert.Throws<InvalidOperationException>(() => new EscalarTrucoUseCase().Ejecutar(id));
            Assert.Contains("pendiente", ex.Message);
        }

        [Fact]
        public void Ejecutar_EnNivelMaximo_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id, nivelTruco: 3);

            var ex = Assert.Throws<InvalidOperationException>(() => new EscalarTrucoUseCase().Ejecutar(id));
            Assert.Contains("nivel máximo", ex.Message);
        }

        [Fact]
        public void Ejecutar_SiElHumanoEsElCantor_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id);
            mano.CantorTruco = "Humano";

            var ex = Assert.Throws<InvalidOperationException>(() => new EscalarTrucoUseCase().Ejecutar(id));
            Assert.Contains("propio canto", ex.Message);
        }

        // ── Desenlaces ───────────────────────────────────────────────────

        [Fact]
        public void Ejecutar_MaquinaNoQuiereElRetruco_GanaElHumano()
        {
            var id = Guid.NewGuid();
            CrearManoEscalable(id, nivelTruco: 1);
            DecisionMaquinaServicio.RandomNext = _ => 99; // nunca acepta

            var resultado = new EscalarTrucoUseCase().Ejecutar(id);

            Assert.True(resultado.TrucoResuelto);
            Assert.Equal("Humano", resultado.GanadorMano);
            Assert.Equal(2, resultado.NivelTruco);
            Assert.Equal(2, resultado.PuntosTrucoMano);
            Assert.Contains("no quiso el Retruco", resultado.EstadoTruco);
        }

        [Fact]
        public void Ejecutar_MaquinaAceptaYContraEscalaAValeCuatro()
        {
            var id = Guid.NewGuid();
            CrearManoEscalable(id, nivelTruco: 1);
            // Tirada fija 21: acepta (prob 90) y escala en nivel 2 (prob 35).
            DecisionMaquinaServicio.RandomNext = _ => 20;

            var resultado = new EscalarTrucoUseCase().Ejecutar(id);

            Assert.False(resultado.TrucoResuelto);
            Assert.Equal(3, resultado.NivelTruco);
            Assert.Equal(4, resultado.PuntosTrucoMano);
            Assert.Equal("Maquina", resultado.CantorTruco);
            Assert.True(resultado.TrucoPendienteRespuestaHumano);
            Assert.Contains("Vale Cuatro", resultado.EstadoTruco);
        }

        [Fact]
        public void Ejecutar_MaquinaSoloAceptaElRetruco_QuedaAbiertoEnNivel2()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoEscalable(id, nivelTruco: 1);
            // Carta débil: acepta con tirada 21 (prob 28) pero no escala (prob 3).
            mano.Maquina.Mano = new List<Carta> { new Carta { ValorTruco = 5 } };
            DecisionMaquinaServicio.RandomNext = _ => 20;

            var resultado = new EscalarTrucoUseCase().Ejecutar(id);

            Assert.False(resultado.TrucoResuelto);
            Assert.Equal(2, resultado.NivelTruco);
            Assert.Equal(3, resultado.PuntosTrucoMano);
            Assert.Contains("quiso el Retruco", resultado.EstadoTruco);
        }

        [Fact]
        public void Ejecutar_EscalarAValeCuatroYMaquinaAcepta_CierraLaNegociacion()
        {
            var id = Guid.NewGuid();
            CrearManoEscalable(id, nivelTruco: 2);
            DecisionMaquinaServicio.RandomNext = _ => 0; // siempre acepta

            var resultado = new EscalarTrucoUseCase().Ejecutar(id);

            Assert.True(resultado.TrucoResuelto);
            Assert.Equal(3, resultado.NivelTruco);
            Assert.Equal(4, resultado.PuntosTrucoMano);
            Assert.Contains("quiso el Vale Cuatro", resultado.EstadoTruco);
        }
    }
}
