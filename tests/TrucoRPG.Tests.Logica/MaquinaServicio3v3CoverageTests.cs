using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de MaquinaServicio3v3: heurísticas de envido/truco,
    /// respuesta al truco en contexto y guardas de AvanzarUnPaso.
    /// Todos los tests fijan RandomNext para ser determinísticos.
    /// </summary>
    public class MaquinaServicio3v3CoverageTests : IDisposable
    {
        public MaquinaServicio3v3CoverageTests()
        {
            MaquinaServicio3v3.RandomNext = _ => 0;
        }

        public void Dispose()
        {
            MaquinaServicio3v3.RandomNext = (max) => new Random().Next(max);
        }

        private static ManoTruco3v3 CrearMano()
        {
            var j1 = new Jugador { Id = "J1" };
            var j2 = new Jugador { Id = "J2", EsMaquina = true };
            var j3 = new Jugador { Id = "J3" };
            var j4 = new Jugador { Id = "J4", EsMaquina = true };
            var j5 = new Jugador { Id = "J5" };
            var j6 = new Jugador { Id = "J6", EsMaquina = true };

            return new ManoTruco3v3
            {
                Posicion1 = j1,
                Posicion2 = j2,
                Posicion3 = j3,
                Posicion4 = j4,
                Posicion5 = j5,
                Posicion6 = j6,
                EquipoA = new Equipo3v3 { Id = "EquipoA", Jugador1 = j1, Jugador2 = j3, Jugador3 = j5 },
                EquipoB = new Equipo3v3 { Id = "EquipoB", Jugador1 = j2, Jugador2 = j4, Jugador3 = j6 },
                TurnoActual = "J2"
            };
        }

        private static List<Carta> ManoConTanto(int a, int b) => new()
        {
            new Carta { Numero = a, Palo = "Espada" },
            new Carta { Numero = b, Palo = "Espada" }
        };

        // ── AceptarEnvido ────────────────────────────────────────────────

        [Fact]
        public void AceptarEnvido_ConTanto30_SiempreAcepta()
        {
            MaquinaServicio3v3.RandomNext = _ => 99;
            Assert.True(MaquinaServicio3v3.AceptarEnvido(ManoConTanto(7, 6)));
        }

        [Fact]
        public void AceptarEnvido_ConTanto20OMenos_SiempreRechaza()
        {
            MaquinaServicio3v3.RandomNext = _ => 0;
            Assert.False(MaquinaServicio3v3.AceptarEnvido(ManoConTanto(10, 11)));
        }

        [Theory]
        [InlineData(7, 2)]  // 29
        [InlineData(6, 2)]  // 28
        [InlineData(5, 2)]  // 27
        [InlineData(4, 2)]  // 26
        [InlineData(3, 2)]  // 25
        [InlineData(3, 1)]  // 24
        [InlineData(2, 1)]  // 23
        [InlineData(2, 10)] // 22
        [InlineData(1, 10)] // 21
        public void AceptarEnvido_RecorreTodosLosNivelesDeTanto(int a, int b)
        {
            MaquinaServicio3v3.RandomNext = _ => 0;
            Assert.True(MaquinaServicio3v3.AceptarEnvido(ManoConTanto(a, b)));
        }

        [Fact]
        public void AceptarEnvido_ConTiradaAlta_RechazaTantoIntermedio()
        {
            MaquinaServicio3v3.RandomNext = _ => 99;
            Assert.False(MaquinaServicio3v3.AceptarEnvido(ManoConTanto(1, 10))); // 21 => 15%
        }

        // ── DebeCantarTruco / AceptarTruco ───────────────────────────────

        [Fact]
        public void DebeCantarTruco_SinCartas_DevuelveFalse()
        {
            Assert.False(MaquinaServicio3v3.DebeCantarTruco(new List<Carta>()));
        }

        [Fact]
        public void DebeCantarTruco_ConCartaBrava_SiempreCanta()
        {
            MaquinaServicio3v3.RandomNext = _ => 99;
            Assert.True(MaquinaServicio3v3.DebeCantarTruco(new List<Carta> { new Carta { ValorTruco = 12 } }));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(99, false)]
        public void DebeCantarTruco_ConUnTres_DependeDelAzar(int tirada, bool esperado)
        {
            MaquinaServicio3v3.RandomNext = _ => tirada;
            Assert.Equal(esperado, MaquinaServicio3v3.DebeCantarTruco(new List<Carta> { new Carta { ValorTruco = 10 } }));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(99, false)]
        public void DebeCantarTruco_ConManoParejaFuerte_DependeDelAzar(int tirada, bool esperado)
        {
            MaquinaServicio3v3.RandomNext = _ => tirada;
            var mano = new List<Carta>
            {
                new Carta { ValorTruco = 9 },
                new Carta { ValorTruco = 8 },
                new Carta { ValorTruco = 7 }
            };
            Assert.Equal(esperado, MaquinaServicio3v3.DebeCantarTruco(mano));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(99, false)]
        public void DebeCantarTruco_ConManoFea_FarolOcasional(int tirada, bool esperado)
        {
            MaquinaServicio3v3.RandomNext = _ => tirada;
            var mano = new List<Carta> { new Carta { ValorTruco = 1 }, new Carta { ValorTruco = 2 } };
            Assert.Equal(esperado, MaquinaServicio3v3.DebeCantarTruco(mano));
        }

        [Theory]
        [InlineData(12)]
        [InlineData(10)]
        [InlineData(9)]
        [InlineData(8)]
        [InlineData(7)]
        [InlineData(6)]
        [InlineData(2)]
        public void AceptarTruco_ConTiradaMinima_AceptaConCualquierCarta(int valor)
        {
            MaquinaServicio3v3.RandomNext = _ => 0;
            Assert.True(MaquinaServicio3v3.AceptarTruco(new List<Carta> { new Carta { ValorTruco = valor } }));
        }

        [Fact]
        public void AceptarTruco_SinCartasYTiradaAlta_Rechaza()
        {
            MaquinaServicio3v3.RandomNext = _ => 99;
            Assert.False(MaquinaServicio3v3.AceptarTruco(new List<Carta>()));
        }

        // ── ResponderTruco (AceptarTrucoEnContexto) ──────────────────────

        private static ManoTruco3v3 CrearManoConTrucoPendiente()
        {
            var mano = CrearMano();
            mano.TrucoPendienteRespuestaDe = "J2";
            mano.TrucoCantado = true;
            mano.NivelTruco = 1;
            mano.CantorTruco = "J1";
            mano.EquipoCantorTruco = "EquipoA";
            return mano;
        }

        [Fact]
        public void ResponderTruco_SiSuEquipoVaGanando_CasiSiempreQuiere()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Vueltas.Add(new Vuelta3v3 { GanadorVuelta = "EquipoB" }); // ganada por J2
            MaquinaServicio3v3.RandomNext = _ => 0;

            MaquinaServicio3v3.ResponderTruco(mano, "J2");

            Assert.DoesNotContain("no quiso", mano.EstadoTruco ?? "");
        }

        [Fact]
        public void ResponderTruco_ConCartaBravaPeroTiradaAlta_NoQuiere()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 12 } };
            MaquinaServicio3v3.RandomNext = _ => 99; // 99 < 88 falla

            MaquinaServicio3v3.ResponderTruco(mano, "J2");

            Assert.Contains("no quiso", mano.EstadoTruco ?? "");
        }

        [Fact]
        public void ResponderTruco_ConCartaBuena_QuiereConTiradaBaja()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 10 } };
            MaquinaServicio3v3.RandomNext = _ => 0;

            MaquinaServicio3v3.ResponderTruco(mano, "J2");

            Assert.DoesNotContain("no quiso", mano.EstadoTruco ?? "");
        }

        [Fact]
        public void ResponderTruco_VaPerdiendoYSinCartas_NoQuiereConTiradaAlta()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Vueltas.Add(new Vuelta3v3 { GanadorVuelta = "EquipoA" }); // perdida por J2
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 5 } };
            MaquinaServicio3v3.RandomNext = _ => 99; // 99 < 15 falla

            MaquinaServicio3v3.ResponderTruco(mano, "J2");

            Assert.Contains("no quiso", mano.EstadoTruco ?? "");
        }

        [Fact]
        public void ResponderTruco_CasoIntermedioConCartas_UsaAceptarTruco()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 7 } };
            MaquinaServicio3v3.RandomNext = _ => 0; // tirada 1 <= 40

            MaquinaServicio3v3.ResponderTruco(mano, "J2");

            Assert.DoesNotContain("no quiso", mano.EstadoTruco ?? "");
        }

        [Fact]
        public void ResponderTruco_CasoIntermedioSinCartas_TiradaBajaQuiere()
        {
            var mano = CrearManoConTrucoPendiente();
            MaquinaServicio3v3.RandomNext = _ => 0; // 0 < 30

            MaquinaServicio3v3.ResponderTruco(mano, "J2");

            Assert.DoesNotContain("no quiso", mano.EstadoTruco ?? "");
        }

        // ── AvanzarUnPaso: guardas y eventos ─────────────────────────────

        [Fact]
        public void AvanzarUnPaso_ActorInexistente_RetornaNull()
        {
            var mano = CrearMano();
            mano.TurnoActual = "J9";

            Assert.Null(MaquinaServicio3v3.AvanzarUnPaso(mano));
        }

        [Fact]
        public void AvanzarUnPaso_ActorNoEsMaquina_RetornaNull()
        {
            var mano = CrearMano();
            mano.TurnoActual = "J3"; // humano compañero

            Assert.Null(MaquinaServicio3v3.AvanzarUnPaso(mano));
        }

        [Fact]
        public void AvanzarUnPaso_EnvidoPendienteSinFase_NoCoincideConTurno_RetornaNull()
        {
            var mano = CrearMano();
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.FaseEnvido = null; // ni pendiente_respuesta ni declarando
            mano.TurnoActual = "J4";

            Assert.Null(MaquinaServicio3v3.AvanzarUnPaso(mano));
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaEscalaAValeCuatro_DevuelveEventoValeCuatro()
        {
            var mano = CrearMano();
            mano.TrucoPendienteRespuestaDe = "J2";
            mano.TrucoCantado = true;
            mano.NivelTruco = 2;
            mano.CantorTruco = "J1";
            mano.EquipoCantorTruco = "EquipoA";
            mano.JugadorMano = "J3"; // deja a J2 como pie del EquipoB
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };
            MaquinaServicio3v3.RandomNext = _ => 10; // acepta y escala

            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("truco", resultado!.Tipo);
            Assert.Equal("¡Vale Cuatro!", resultado.Texto);
        }

        // ── DeclararTanto ────────────────────────────────────────────────

        [Fact]
        public void DeclararTanto_FaseIncorrecta_NoHaceNada()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";

            MaquinaServicio3v3.DeclararTanto(mano, "J2");

            Assert.Empty(mano.TantosDeclarados);
        }

        [Fact]
        public void DeclararTanto_NoEsElResponsable_NoHaceNada()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J4";

            MaquinaServicio3v3.DeclararTanto(mano, "J2");

            Assert.Empty(mano.TantosDeclarados);
        }

        [Fact]
        public void DeclararTanto_JugadorHumano_NoHaceNada()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J1";

            MaquinaServicio3v3.DeclararTanto(mano, "J1");

            Assert.Empty(mano.TantosDeclarados);
        }

        [Fact]
        public void DeclararTanto_SinTantoPrecalculado_UsaLasCartasOriginales()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.Posicion2.Mano = ManoConTanto(7, 6); // 33

            MaquinaServicio3v3.DeclararTanto(mano, "J2");

            Assert.Equal(33, mano.TantosDeclarados["J2"]);
        }

        [Fact]
        public void DeclararTanto_SiElRivalYaMostroMasTantos_DiceSonBuenas()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TantosReales["J2"] = 25;
            mano.TantosDeclarados["J1"] = 30; // rival declaró antes y más alto

            MaquinaServicio3v3.DeclararTanto(mano, "J2");

            Assert.True(mano.SonBuenasDeclarado);
            Assert.Equal("J2", mano.JugadorQueDijoSonBuenas);
        }

        // ── ProcesarTurnoMaquina: guardas extra ──────────────────────────

        [Fact]
        public void ProcesarTurnoMaquina_ConGanadorDeMano_NoHaceNada()
        {
            var mano = CrearMano();
            mano.GanadorMano = "EquipoA";
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };

            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            Assert.Empty(mano.Posicion2.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_ConEnvidoPendiente_NoHaceNada()
        {
            var mano = CrearMano();
            mano.EnvidoPendienteRespuestaDe = "J1";
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };

            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            Assert.Empty(mano.Posicion2.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_JugadorInexistente_NoHaceNada()
        {
            var mano = CrearMano();
            mano.TurnoActual = "J9";

            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J9");

            Assert.Empty(mano.Posicion2.Jugadas);
        }

        // ── ResponderEnvido: escaladas ───────────────────────────────────

        private static ManoTruco3v3 CrearManoConEnvidoPendiente(string tipo, int numeroA = 7, int numeroB = 6)
        {
            var mano = CrearMano();
            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoCantado = true;
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TipoEnvidoCantado = tipo;
            mano.CantorEnvido = "J1";
            mano.JugadorMano = "J1";
            mano.Posicion2.Mano = ManoConTanto(numeroA, numeroB);
            return mano;
        }

        [Fact]
        public void ResponderEnvido_ConTanto33YTiradaMedia_EscalaAEnvidoEnvido()
        {
            var mano = CrearManoConEnvidoPendiente("Envido");
            MaquinaServicio3v3.RandomNext = _ => 45; // no llega a Real (40) pero sí a EE (50)

            MaquinaServicio3v3.ResponderEnvido(mano, "J2");

            Assert.Equal("EnvidoEnvido", mano.TipoEnvidoCantado);
        }

        [Fact]
        public void ResponderEnvido_ConTanto33YTiradaBaja_EscalaARealEnvido()
        {
            var mano = CrearManoConEnvidoPendiente("Envido");
            MaquinaServicio3v3.RandomNext = _ => 10;

            MaquinaServicio3v3.ResponderEnvido(mano, "J2");

            Assert.Equal("RealEnvido", mano.TipoEnvidoCantado);
        }

        [Fact]
        public void ResponderEnvido_DesdeEnvidoEnvido_EscalaARealEnvido()
        {
            var mano = CrearManoConEnvidoPendiente("EnvidoEnvido");
            MaquinaServicio3v3.RandomNext = _ => 10;

            MaquinaServicio3v3.ResponderEnvido(mano, "J2");

            Assert.Equal("RealEnvido", mano.TipoEnvidoCantado);
        }

        [Fact]
        public void ResponderEnvido_DesdeRealEnvidoConTanto33_EscalaAFaltaEnvido()
        {
            var mano = CrearManoConEnvidoPendiente("RealEnvido");
            MaquinaServicio3v3.RandomNext = _ => 10;

            MaquinaServicio3v3.ResponderEnvido(mano, "J2");

            Assert.Equal("FaltaEnvido", mano.TipoEnvidoCantado);
        }

        [Fact]
        public void ResponderEnvido_DesdeFaltaEnvido_NoEscalaYDeclara()
        {
            var mano = CrearManoConEnvidoPendiente("FaltaEnvido");
            MaquinaServicio3v3.RandomNext = _ => 10;

            MaquinaServicio3v3.ResponderEnvido(mano, "J2");

            Assert.Equal("declarando_tantos", mano.FaseEnvido);
        }

        [Fact]
        public void ResponderEnvido_ConTanto31_NoEscalaARealPeroPuedeCantarEnvidoEnvido()
        {
            // tanto 31 (< 32): con tirada 10 va a "Envido Envido" (r < 50)
            var mano = CrearManoConEnvidoPendiente("Envido", numeroA: 7, numeroB: 4);
            MaquinaServicio3v3.RandomNext = _ => 10;

            MaquinaServicio3v3.ResponderEnvido(mano, "J2");

            Assert.Equal("EnvidoEnvido", mano.TipoEnvidoCantado);
        }
    }
}
