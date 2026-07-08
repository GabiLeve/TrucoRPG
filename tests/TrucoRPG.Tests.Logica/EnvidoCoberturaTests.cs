using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de IniciativaMaquinaEnvidoServicio, MentiraEnvidoServicio
    /// y los switches auxiliares de EnvidoServicio.
    /// </summary>
    public class EnvidoCoberturaTests : IDisposable
    {
        public void Dispose()
        {
            AzarServicio.TirarProbabilidadOverride = null;
        }

        private static List<Carta> ManoConTanto(int a, int b) => new()
        {
            new Carta { Numero = a, Palo = "Espada" },
            new Carta { Numero = b, Palo = "Espada" }
        };

        // ── IniciativaMaquinaEnvidoServicio ──────────────────────────────

        [Fact]
        public void DebeCantarEnvido_ConTanto30_SiempreCanta()
        {
            AzarServicio.TirarProbabilidadOverride = _ => false;
            Assert.True(IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(ManoConTanto(7, 6), 0));
        }

        [Fact]
        public void DebeCantarEnvido_ConMentira100_SiempreCanta()
        {
            AzarServicio.TirarProbabilidadOverride = _ => false;
            Assert.True(IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(new List<Carta>(), 100));
        }

        [Theory]
        [InlineData(7, 2)]  // tanto 29
        [InlineData(6, 2)]  // tanto 28
        [InlineData(5, 2)]  // tanto 27
        [InlineData(4, 2)]  // tanto 26
        [InlineData(3, 2)]  // tanto 25
        [InlineData(3, 1)]  // tanto 24
        [InlineData(2, 1)]  // tanto 23
        [InlineData(2, 10)] // tanto 22
        [InlineData(1, 10)] // tanto 21
        [InlineData(10, 11)]// tanto 20
        public void DebeCantarEnvido_RecorreTodosLosNivelesDeTanto(int a, int b)
        {
            AzarServicio.TirarProbabilidadOverride = _ => true;
            Assert.True(IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(ManoConTanto(a, b), 0));
        }

        [Fact]
        public void DebeCantarEnvido_ConTantoMuyBajo_UsaProbabilidadPorDefecto()
        {
            AzarServicio.TirarProbabilidadOverride = _ => false;
            var mano = new List<Carta> { new Carta { Numero = 4, Palo = "Espada" } }; // tanto 4
            Assert.False(IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 0));
        }

        [Fact]
        public void DebeCantarEnvido_ConMentira95_SumaBonusExtra()
        {
            AzarServicio.TirarProbabilidadOverride = p => p >= 0.99;
            // tanto 21 (12) + 95*0.70 (67) + 20 => 99 => no llega a 1.0
            Assert.False(IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(ManoConTanto(1, 10), 95));
        }

        // ── MentiraEnvidoServicio ────────────────────────────────────────

        [Fact]
        public void ObtenerTantoCantado_SinMentira_DevuelveTantoRealNormalizado()
        {
            int cantado = MentiraEnvidoServicio.ObtenerTantoCantado(25, 0, out bool mintio);
            Assert.Equal(25, cantado);
            Assert.False(mintio);
        }

        [Fact]
        public void ObtenerTantoCantado_TantoEntre8Y19_SeNormalizaA7()
        {
            int cantado = MentiraEnvidoServicio.ObtenerTantoCantado(12, 0, out bool mintio);
            Assert.Equal(7, cantado);
            Assert.False(mintio);
        }

        [Fact]
        public void ObtenerTantoCantado_ConMentiraTotalYTantoBajo_MienteHaciaArriba()
        {
            // nivelMentira 100 => siempre miente; tanto 7 + incremento máx 13 => tope 20
            int cantado = MentiraEnvidoServicio.ObtenerTantoCantado(7, 100, out bool mintio);
            Assert.Equal(20, cantado);
            Assert.True(mintio);
        }

        [Fact]
        public void ObtenerTantoCantado_ConMentiraTotalYTantoAlto_NoEsMentiraReal()
        {
            // real 25 (>=20) y cantado >= 20 => no cuenta como mentira real
            int cantado = MentiraEnvidoServicio.ObtenerTantoCantado(25, 100, out bool mintio);
            Assert.InRange(cantado, 26, 33);
            Assert.False(mintio);
        }

        [Fact]
        public void ObtenerTantoCantado_ConTanto33_UsaElFallbackDeCandidatos()
        {
            // tantoBase 33: no hay candidatos >= 33 distintos de 33 => fallback t != 33
            int cantado = MentiraEnvidoServicio.ObtenerTantoCantado(33, 100, out _);
            Assert.NotEqual(33, cantado);
            Assert.InRange(cantado, 0, 32);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(15)]
        [InlineData(25)]
        [InlineData(35)]
        [InlineData(45)]
        [InlineData(55)]
        [InlineData(65)]
        [InlineData(75)]
        [InlineData(85)]
        [InlineData(95)]
        public void ObtenerTantoCantado_ConDistintosNivelesDeMentira_DevuelveSiempreUnTantoValido(int nivel)
        {
            // Ejecuta muchas veces para recorrer las ramas internas de incremento máximo;
            // el resultado siempre tiene que ser un tanto válido de envido.
            for (int i = 0; i < 500; i++)
            {
                int cantado = MentiraEnvidoServicio.ObtenerTantoCantado(24, nivel, out _);
                Assert.True(cantado <= 33 && cantado >= 0, $"Tanto inválido: {cantado}");
                Assert.False(cantado is >= 8 and <= 19, $"Tanto imposible de envido: {cantado}");
            }
        }

        // ── EnvidoServicio: switches auxiliares ──────────────────────────

        [Theory]
        [InlineData("EnvidoEnvido", 2)]
        [InlineData("RealEnvido", 3)]
        [InlineData("FaltaEnvido", 0)]
        [InlineData("Envido", 2)]
        [InlineData(null, 2)]
        public void IncrementoPuntosTipo_DevuelveElIncrementoDeCadaCanto(string? tipo, int esperado)
        {
            Assert.Equal(esperado, EnvidoServicio.IncrementoPuntosTipo(tipo));
        }

        [Theory]
        [InlineData("envido", 2)]
        [InlineData("Envido Envido", 4)]
        [InlineData("envidoenvido", 4)]
        [InlineData("Real Envido", 3)]
        [InlineData("realenvido", 3)]
        [InlineData("Falta Envido", 0)]
        [InlineData("faltaenvido", 0)]
        [InlineData("cualquier cosa", 2)]
        [InlineData(null, 2)]
        [InlineData("   ", 2)]
        public void ObtenerPuntosSegunTipo_DevuelveLosPuntosDeCadaCanto(string? tipo, int esperado)
        {
            Assert.Equal(esperado, EnvidoServicio.ObtenerPuntosSegunTipo(tipo));
        }

        [Theory]
        [InlineData("Envido", 0)]
        [InlineData("EnvidoEnvido", 1)]
        [InlineData("RealEnvido", 2)]
        [InlineData("FaltaEnvido", 3)]
        [InlineData("otro", -1)]
        [InlineData(null, -1)]
        public void OrdinalTipo_DevuelveElOrdenDeCadaCanto(string? tipo, int esperado)
        {
            Assert.Equal(esperado, EnvidoServicio.OrdinalTipo(tipo));
        }

        [Theory]
        [InlineData("envido envido", "EnvidoEnvido")]
        [InlineData("EnvidoEnvido", "EnvidoEnvido")]
        [InlineData("real envido", "RealEnvido")]
        [InlineData("REALENVIDO", "RealEnvido")]
        [InlineData("falta envido", "FaltaEnvido")]
        [InlineData("faltaenvido", "FaltaEnvido")]
        [InlineData("envido", "Envido")]
        [InlineData(null, "Envido")]
        public void NormalizarTipo_ConvierteCadaVariante(string? tipo, string esperado)
        {
            Assert.Equal(esperado, EnvidoServicio.NormalizarTipo(tipo));
        }

        [Theory]
        [InlineData(10, true, "mintio")]
        [InlineData(20, false, "se_jugo")]
        [InlineData(25, false, "tenia")]
        public void ClasificarActitud_SegunTantoYMentira(int tanto, bool mintio, string esperado)
        {
            Assert.Equal(esperado, EnvidoServicio.ClasificarActitud(tanto, mintio));
        }

        [Fact]
        public void CalcularPuntosFalta_NuncaDevuelveMenosDeUno()
        {
            Assert.Equal(5, EnvidoServicio.CalcularPuntosFalta(25));
            Assert.Equal(1, EnvidoServicio.CalcularPuntosFalta(30));
        }
    }
}
