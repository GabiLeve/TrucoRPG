using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de DecisionMaquinaServicio: recorre todos los brazos
    /// de los switch de AceptarEnvido, AceptarTruco y EscalarTruco usando el
    /// seam RandomNext para que el resultado sea determinístico.
    /// </summary>
    public class DecisionMaquinaServicioCoverageTests : IDisposable
    {
        private readonly Func<int, int> _randomOriginal;

        public DecisionMaquinaServicioCoverageTests()
        {
            _randomOriginal = DecisionMaquinaServicio.RandomNext;
        }

        public void Dispose()
        {
            DecisionMaquinaServicio.RandomNext = _randomOriginal;
        }

        // Dos cartas del mismo palo => tanto = 20 + suma de valores de envido.
        private static List<Carta> ManoConTanto(int a, int b) => new()
        {
            new Carta { Numero = a, Palo = "Espada" },
            new Carta { Numero = b, Palo = "Espada" }
        };

        private static List<Carta> ManoConValorTruco(int valor) => new()
        {
            new Carta { Numero = 1, Palo = "Espada", ValorTruco = valor }
        };

        // ── AceptarEnvido ────────────────────────────────────────────────

        [Fact]
        public void AceptarEnvido_ConTanto30OMas_SiempreAcepta()
        {
            DecisionMaquinaServicio.RandomNext = _ => 99;
            Assert.True(DecisionMaquinaServicio.AceptarEnvido(ManoConTanto(7, 6), 0)); // 33
        }

        [Fact]
        public void AceptarEnvido_ConTantoBajoYSinMentira_SiempreRechaza()
        {
            DecisionMaquinaServicio.RandomNext = _ => 0;
            Assert.False(DecisionMaquinaServicio.AceptarEnvido(ManoConTanto(10, 11), 0)); // 20
        }

        [Theory]
        [InlineData(7, 2)]  // tanto 29
        [InlineData(6, 2)]  // tanto 28
        [InlineData(5, 2)]  // tanto 27
        [InlineData(4, 2)]  // tanto 26
        [InlineData(3, 2)]  // tanto 25
        [InlineData(3, 1)]  // tanto 24
        [InlineData(2, 1)]  // tanto 23
        [InlineData(2, 10)] // tanto 22 (la figura vale 0)
        [InlineData(1, 10)] // tanto 21
        [InlineData(10, 11)]// tanto 20 (entra al switch porque hay mentira)
        public void AceptarEnvido_ConTiradaMinima_AceptaEnTodosLosNiveles(int a, int b)
        {
            DecisionMaquinaServicio.RandomNext = _ => 0; // tirada 1: siempre <= probabilidad
            Assert.True(DecisionMaquinaServicio.AceptarEnvido(ManoConTanto(a, b), 10));
        }

        [Fact]
        public void AceptarEnvido_TantoMuyBajoConMentira_UsaProbabilidadPorDefecto()
        {
            DecisionMaquinaServicio.RandomNext = _ => 0;
            var mano = new List<Carta> { new Carta { Numero = 4, Palo = "Espada" } }; // tanto 4
            Assert.True(DecisionMaquinaServicio.AceptarEnvido(mano, 10));
        }

        [Fact]
        public void AceptarEnvido_ConTiradaMaxima_RechazaSiProbabilidadNoLlegaA100()
        {
            DecisionMaquinaServicio.RandomNext = _ => 99; // tirada 100
            Assert.False(DecisionMaquinaServicio.AceptarEnvido(ManoConTanto(1, 10), 1)); // 21 => ~19%
        }

        [Fact]
        public void AceptarEnvido_ConMentiraExtrema_SumaBonusYAcepta()
        {
            DecisionMaquinaServicio.RandomNext = _ => 99;
            // tanto 25 (55) + bonus 55*... : mentira 100 => 55 + 55 + 20 + 20 => clamp 100
            Assert.True(DecisionMaquinaServicio.AceptarEnvido(ManoConTanto(3, 2), 100));
        }

        [Fact]
        public void AceptarEnvido_ConMentiraAlta_SumaBonusIntermedio()
        {
            DecisionMaquinaServicio.RandomNext = _ => 0;
            // mentira 85: cubre el bonus de >=80 sin llegar a >=95
            Assert.True(DecisionMaquinaServicio.AceptarEnvido(ManoConTanto(1, 10), 85));
        }

        // ── AceptarTruco ─────────────────────────────────────────────────

        [Theory]
        [InlineData(14)] // >= 11
        [InlineData(10)]
        [InlineData(9)]
        [InlineData(8)]
        [InlineData(7)]
        [InlineData(6)]
        [InlineData(5)]
        [InlineData(2)]  // default
        public void AceptarTruco_ConTiradaMinima_AceptaConCualquierCarta(int valor)
        {
            DecisionMaquinaServicio.RandomNext = _ => 0;
            Assert.True(DecisionMaquinaServicio.AceptarTruco(ManoConValorTruco(valor), 0));
        }

        [Fact]
        public void AceptarTruco_SinCartasYTiradaMaxima_Rechaza()
        {
            DecisionMaquinaServicio.RandomNext = _ => 99;
            Assert.False(DecisionMaquinaServicio.AceptarTruco(new List<Carta>(), 0));
        }

        [Fact]
        public void AceptarTruco_ConMentira_AgregaBonusPorCaradurez()
        {
            DecisionMaquinaServicio.RandomNext = _ => 0;
            Assert.True(DecisionMaquinaServicio.AceptarTruco(ManoConValorTruco(5), 50));
        }

        // ── EscalarTruco ─────────────────────────────────────────────────

        [Fact]
        public void EscalarTruco_EnNivelMaximo_NuncaEscala()
        {
            DecisionMaquinaServicio.RandomNext = _ => 0;
            Assert.False(DecisionMaquinaServicio.EscalarTruco(ManoConValorTruco(14), 100, 3));
        }

        [Theory]
        [InlineData(14)] // >= 13
        [InlineData(12)]
        [InlineData(11)]
        [InlineData(10)]
        [InlineData(9)]
        [InlineData(8)]
        [InlineData(3)]  // default
        public void EscalarTruco_ConTiradaMinima_EscalaConCualquierCarta(int valor)
        {
            DecisionMaquinaServicio.RandomNext = _ => 0;
            Assert.True(DecisionMaquinaServicio.EscalarTruco(ManoConValorTruco(valor), 0, 1));
        }

        [Fact]
        public void EscalarTruco_EnNivel2_ReduceLaProbabilidad()
        {
            DecisionMaquinaServicio.RandomNext = _ => 0;
            Assert.True(DecisionMaquinaServicio.EscalarTruco(ManoConValorTruco(14), 0, 2));
        }

        [Fact]
        public void EscalarTruco_SinCartasYTiradaMaxima_NoEscala()
        {
            DecisionMaquinaServicio.RandomNext = _ => 99;
            Assert.False(DecisionMaquinaServicio.EscalarTruco(new List<Carta>(), 0, 1));
        }
    }
}
