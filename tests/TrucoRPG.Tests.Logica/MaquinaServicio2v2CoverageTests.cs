using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de MaquinaServicio2v2: heurísticas de envido/truco,
    /// elección de carta en equipo, respuesta al truco, escaladas de envido,
    /// consultas del compañero y órdenes del humano.
    /// Todos los tests fijan RandomNext para ser determinísticos.
    /// </summary>
    public class MaquinaServicio2v2CoverageTests : IDisposable
    {
        public MaquinaServicio2v2CoverageTests()
        {
            MaquinaServicio2v2.RandomNext = _ => 0;
        }

        public void Dispose()
        {
            MaquinaServicio2v2.RandomNext = (max) => new Random().Next(max);
        }

        private static ManoTruco2v2 CrearMano()
        {
            var j1 = new Jugador { Id = "J1" };
            var j2 = new Jugador { Id = "J2", EsMaquina = true };
            var j3 = new Jugador { Id = "J3", EsMaquina = true };
            var j4 = new Jugador { Id = "J4", EsMaquina = true };

            return new ManoTruco2v2
            {
                Posicion1 = j1,
                Posicion2 = j2,
                Posicion3 = j3,
                Posicion4 = j4,
                EquipoA = new Equipo2v2 { Id = "EquipoA", Jugador1 = j1, Jugador2 = j3 },
                EquipoB = new Equipo2v2 { Id = "EquipoB", Jugador1 = j2, Jugador2 = j4 },
                TurnoActual = "J2"
            };
        }

        private static List<Carta> ManoConTanto(int a, int b) => new()
        {
            new Carta { Numero = a, Palo = "Espada" },
            new Carta { Numero = b, Palo = "Espada" }
        };

        // ── AceptarEnvido / DebeDeclararSonBuenas ────────────────────────

        [Fact]
        public void AceptarEnvido_ConTanto30_SiempreAcepta()
        {
            MaquinaServicio2v2.RandomNext = _ => 99;
            Assert.True(MaquinaServicio2v2.AceptarEnvido(ManoConTanto(7, 6)));
        }

        [Fact]
        public void AceptarEnvido_ConTanto20OMenos_SiempreRechaza()
        {
            MaquinaServicio2v2.RandomNext = _ => 0;
            Assert.False(MaquinaServicio2v2.AceptarEnvido(ManoConTanto(10, 11)));
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
            MaquinaServicio2v2.RandomNext = _ => 0;
            Assert.True(MaquinaServicio2v2.AceptarEnvido(ManoConTanto(a, b)));
        }

        [Fact]
        public void AceptarEnvido_TantoIntermedioConTiradaAlta_Rechaza()
        {
            MaquinaServicio2v2.RandomNext = _ => 99;
            Assert.False(MaquinaServicio2v2.AceptarEnvido(ManoConTanto(1, 10)));
        }

        [Fact]
        public void DebeDeclararSonBuenas_SinTantoRival_DevuelveFalse()
        {
            Assert.False(MaquinaServicio2v2.DebeDeclararSonBuenas(20, null));
        }

        [Fact]
        public void DebeDeclararSonBuenas_ConDiferenciaClara_DevuelveTrue()
        {
            Assert.True(MaquinaServicio2v2.DebeDeclararSonBuenas(20, 25));
        }

        [Fact]
        public void DebeDeclararSonBuenas_ConDiferenciaChica_DevuelveFalse()
        {
            Assert.False(MaquinaServicio2v2.DebeDeclararSonBuenas(24, 26));
        }

        // ── DebeCantarTruco / AceptarTruco ───────────────────────────────

        [Fact]
        public void DebeCantarTruco_SinCartas_DevuelveFalse()
        {
            Assert.False(MaquinaServicio2v2.DebeCantarTruco(new List<Carta>()));
        }

        [Fact]
        public void DebeCantarTruco_ConCartaBrava_SiempreCanta()
        {
            MaquinaServicio2v2.RandomNext = _ => 99;
            Assert.True(MaquinaServicio2v2.DebeCantarTruco(new List<Carta> { new Carta { ValorTruco = 13 } }));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(99, false)]
        public void DebeCantarTruco_ConUnTres_DependeDelAzar(int tirada, bool esperado)
        {
            MaquinaServicio2v2.RandomNext = _ => tirada;
            Assert.Equal(esperado, MaquinaServicio2v2.DebeCantarTruco(new List<Carta> { new Carta { ValorTruco = 10 } }));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(99, false)]
        public void DebeCantarTruco_ConManoParejaFuerte_DependeDelAzar(int tirada, bool esperado)
        {
            MaquinaServicio2v2.RandomNext = _ => tirada;
            var mano = new List<Carta>
            {
                new Carta { ValorTruco = 9 },
                new Carta { ValorTruco = 8 },
                new Carta { ValorTruco = 7 }
            };
            Assert.Equal(esperado, MaquinaServicio2v2.DebeCantarTruco(mano));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(99, false)]
        public void DebeCantarTruco_ConManoFea_FarolOcasional(int tirada, bool esperado)
        {
            MaquinaServicio2v2.RandomNext = _ => tirada;
            var mano = new List<Carta> { new Carta { ValorTruco = 1 }, new Carta { ValorTruco = 2 } };
            Assert.Equal(esperado, MaquinaServicio2v2.DebeCantarTruco(mano));
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
            MaquinaServicio2v2.RandomNext = _ => 0;
            Assert.True(MaquinaServicio2v2.AceptarTruco(new List<Carta> { new Carta { ValorTruco = valor } }));
        }

        [Fact]
        public void AceptarTruco_SinCartasYTiradaAlta_Rechaza()
        {
            MaquinaServicio2v2.RandomNext = _ => 99;
            Assert.False(MaquinaServicio2v2.AceptarTruco(new List<Carta>()));
        }

        // ── ElegirCartaEnEquipo (vía ProcesarTurnoMaquina) ───────────────

        private static ManoTruco2v2 CrearManoParaJugarCarta()
        {
            var mano = CrearMano();
            mano.EnvidoResuelto = true;
            mano.TrucoResuelto = true;
            mano.VueltaActual = new Vuelta2v2();
            return mano;
        }

        [Fact]
        public void ProcesarTurnoMaquina_AbreVuelta_JuegaCartaMedia()
        {
            var mano = CrearManoParaJugarCarta();
            var cMala = new Carta { ValorTruco = 1 };
            var cMedia = new Carta { ValorTruco = 7 };
            var cBuena = new Carta { ValorTruco = 14 };
            mano.Posicion2.Mano = new List<Carta> { cMala, cMedia, cBuena };

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.Equal(cMedia, mano.VueltaActual!.CartasJugadas["J2"]);
        }

        [Fact]
        public void ProcesarTurnoMaquina_SiSuCompaneroVaGanando_TiraLaMasBaja()
        {
            var mano = CrearManoParaJugarCarta();
            mano.TurnoActual = "J4";
            var cMala = new Carta { ValorTruco = 2 };
            var cBuena = new Carta { ValorTruco = 12 };
            mano.Posicion4.Mano = new List<Carta> { cBuena, cMala };
            mano.VueltaActual!.CartasJugadas["J2"] = new Carta { ValorTruco = 11 }; // compañero gana
            mano.VueltaActual.CartasJugadas["J1"] = new Carta { ValorTruco = 3 };

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J4");

            Assert.Equal(cMala, mano.VueltaActual.CartasJugadas["J4"]);
        }

        [Fact]
        public void ProcesarTurnoMaquina_SiElRivalVaGanando_GanaConLoJusto()
        {
            var mano = CrearManoParaJugarCarta();
            var cTres = new Carta { ValorTruco = 10 };
            var cAncho = new Carta { ValorTruco = 14 };
            mano.Posicion2.Mano = new List<Carta> { cAncho, cTres };
            mano.VueltaActual!.CartasJugadas["J1"] = new Carta { ValorTruco = 11 }; // rival gana

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.Equal(cAncho, mano.VueltaActual.CartasJugadas["J2"]);
        }

        [Fact]
        public void ProcesarTurnoMaquina_SiNoPuedeGanar_TiraLaMasBaja()
        {
            var mano = CrearManoParaJugarCarta();
            var cFea1 = new Carta { ValorTruco = 2 };
            var cFea2 = new Carta { ValorTruco = 5 };
            mano.Posicion2.Mano = new List<Carta> { cFea2, cFea1 };
            mano.VueltaActual!.CartasJugadas["J1"] = new Carta { ValorTruco = 14 };

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.Equal(cFea1, mano.VueltaActual.CartasJugadas["J2"]);
        }

        [Fact]
        public void ProcesarTurnoMaquina_ConOrdenJugarMayor_JuegaLaMasAlta()
        {
            var mano = CrearManoParaJugarCarta();
            mano.OrdenJugarMayor = "J2";
            var cMala = new Carta { ValorTruco = 1 };
            var cBuena = new Carta { ValorTruco = 14 };
            mano.Posicion2.Mano = new List<Carta> { cMala, cBuena };

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.Null(mano.OrdenJugarMayor);
            Assert.Equal(cBuena, mano.VueltaActual!.CartasJugadas["J2"]);
        }

        // ── ProcesarTurnoMaquina: cantos y guardas ───────────────────────

        [Fact]
        public void ProcesarTurnoMaquina_ManoTerminada_NoHaceNada()
        {
            var mano = CrearMano();
            mano.ManoTerminada = true;
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.Empty(mano.Posicion2.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_NoEsSuTurno_NoHaceNada()
        {
            var mano = CrearMano();
            mano.TurnoActual = "J1";
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.Empty(mano.Posicion2.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_JugadorSinCartas_NoHaceNada()
        {
            var mano = CrearMano();

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.False(mano.TrucoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_PieRivalConTantoAlto_CantaEnvido()
        {
            var mano = CrearMano();
            mano.JugadorMano = "J3"; // orden J3,J4,J1,J2 => pie de EquipoB es J2
            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { Palo = "Oro", Numero = 6, ValorTruco = 6 },
                new Carta { Palo = "Oro", Numero = 7, ValorTruco = 7 },
                new Carta { Palo = "Copa", Numero = 12, ValorTruco = 5 }
            };

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.True(mano.EnvidoCantado);
            Assert.Equal("J2", mano.CantorEnvido);
            Assert.Equal("J1", mano.EnvidoPendienteRespuestaDe);
        }

        [Fact]
        public void ProcesarTurnoMaquina_ConVueltaIniciadaYCartaBrava_CantaTruco()
        {
            var mano = CrearMano();
            mano.VueltaActual = new Vuelta2v2();
            mano.EnvidoResuelto = true;
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 13 } };

            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            Assert.True(mano.TrucoCantado);
            Assert.Equal("J2", mano.CantorTruco);
            Assert.Equal("EquipoB", mano.EquipoCantorTruco);
            Assert.Equal("J1", mano.TrucoPendienteRespuestaDe);
        }

        // ── ResponderTruco ───────────────────────────────────────────────

        private static ManoTruco2v2 CrearManoConTrucoPendiente()
        {
            var mano = CrearMano();
            mano.TrucoCantado = true;
            mano.TrucoPendienteRespuestaDe = "J2";
            mano.NivelTruco = 1;
            mano.PuntosTrucoMano = 2;
            mano.CantorTruco = "J1";
            mano.EquipoCantorTruco = "EquipoA";
            return mano;
        }

        [Fact]
        public void ResponderTruco_NoEsElResponsable_NoHaceNada()
        {
            var mano = CrearManoConTrucoPendiente();

            MaquinaServicio2v2.ResponderTruco(mano, "J4");

            Assert.Equal("J2", mano.TrucoPendienteRespuestaDe);
        }

        [Fact]
        public void ResponderTruco_SinCartasYTiradaAlta_NoQuiereYPierdeLaMano()
        {
            var mano = CrearManoConTrucoPendiente();
            MaquinaServicio2v2.RandomNext = _ => 99;

            MaquinaServicio2v2.ResponderTruco(mano, "J2");

            Assert.True(mano.TrucoResuelto);
            Assert.True(mano.ManoTerminada);
            Assert.Equal("EquipoA", mano.GanadorMano);
            Assert.Contains("no quiso", mano.EstadoTruco);
        }

        [Fact]
        public void ResponderTruco_SiSuEquipoVaGanando_Quiere()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Vueltas.Add(new Vuelta2v2 { GanadorVuelta = "EquipoB" });
            MaquinaServicio2v2.RandomNext = _ => 0;

            MaquinaServicio2v2.ResponderTruco(mano, "J2");

            Assert.False(mano.TrucoResuelto);
            Assert.Contains("quiso truco", mano.EstadoTruco);
        }

        [Fact]
        public void ResponderTruco_VaPerdiendoSinCartas_NoQuiereConTiradaAlta()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Vueltas.Add(new Vuelta2v2 { GanadorVuelta = "EquipoA" });
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 4 } };
            MaquinaServicio2v2.RandomNext = _ => 99;

            MaquinaServicio2v2.ResponderTruco(mano, "J2");

            Assert.True(mano.TrucoResuelto);
            Assert.Contains("no quiso", mano.EstadoTruco);
        }

        [Fact]
        public void ResponderTruco_ConCartaBuena_QuiereSinEscalar()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 10 } };
            MaquinaServicio2v2.RandomNext = _ => 0;

            MaquinaServicio2v2.ResponderTruco(mano, "J2");

            Assert.False(mano.TrucoResuelto);
            Assert.Equal(1, mano.NivelTruco); // con carta 10 no escala
        }

        [Fact]
        public void ResponderTruco_ConCartaBravaYTiradaBaja_EscalaARetruco()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };
            MaquinaServicio2v2.RandomNext = _ => 10;

            MaquinaServicio2v2.ResponderTruco(mano, "J2");

            Assert.Equal(2, mano.NivelTruco);
            Assert.Equal(3, mano.PuntosTrucoMano);
            Assert.Equal("EquipoB", mano.EquipoCantorTruco);
            Assert.Equal("J2", mano.CantorTruco);
            Assert.Equal("J1", mano.TrucoPendienteRespuestaDe);
            Assert.Contains("Retruco", mano.EstadoTruco);
        }

        [Fact]
        public void ResponderTruco_EnNivel2ConCartaBrava_EscalaAValeCuatro()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.NivelTruco = 2;
            mano.PuntosTrucoMano = 3;
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };
            MaquinaServicio2v2.RandomNext = _ => 10;

            MaquinaServicio2v2.ResponderTruco(mano, "J2");

            Assert.Equal(3, mano.NivelTruco);
            Assert.Equal(4, mano.PuntosTrucoMano);
            Assert.Contains("Vale Cuatro", mano.EstadoTruco);
        }

        // ── DeclararTanto ────────────────────────────────────────────────

        [Fact]
        public void DeclararTanto_FaseIncorrecta_NoHaceNada()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";

            MaquinaServicio2v2.DeclararTanto(mano, "J2");

            Assert.Empty(mano.TantosDeclarados);
        }

        [Fact]
        public void DeclararTanto_SinTantoPrecalculado_UsaLasCartasOriginales()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.Posicion2.Mano = ManoConTanto(7, 6); // 33

            MaquinaServicio2v2.DeclararTanto(mano, "J2");

            Assert.Equal(33, mano.TantosDeclarados["J2"]);
        }

        [Fact]
        public void DeclararTanto_SiElRivalYaMostroMasTantos_DiceSonBuenas()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TantosReales["J2"] = 25;
            mano.TantosDeclarados["J1"] = 30;

            MaquinaServicio2v2.DeclararTanto(mano, "J2");

            Assert.True(mano.SonBuenasDeclarado);
            Assert.Equal("J2", mano.JugadorQueDijoSonBuenas);
        }

        // ── ResponderEnvido: escaladas ───────────────────────────────────

        private static ManoTruco2v2 CrearManoConEnvidoPendiente(string tipo, int numeroA = 7, int numeroB = 6)
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
        public void ResponderEnvido_SinCartas_ResuelveNoQuiero()
        {
            var mano = CrearManoConEnvidoPendiente("Envido");
            mano.Posicion2.Mano = new List<Carta>();

            MaquinaServicio2v2.ResponderEnvido(mano, "J2");

            Assert.True(mano.EnvidoResuelto);
            Assert.Equal("EquipoA", mano.GanadorEnvido);
        }

        [Fact]
        public void ResponderEnvido_ConTanto33YTiradaMedia_EscalaAEnvidoEnvido()
        {
            var mano = CrearManoConEnvidoPendiente("Envido");
            MaquinaServicio2v2.RandomNext = _ => 45;

            MaquinaServicio2v2.ResponderEnvido(mano, "J2");

            Assert.Equal("EnvidoEnvido", mano.TipoEnvidoCantado);
            Assert.Equal("J2", mano.CantorEnvido);
        }

        [Fact]
        public void ResponderEnvido_ConTanto33YTiradaBaja_EscalaARealEnvido()
        {
            var mano = CrearManoConEnvidoPendiente("Envido");
            MaquinaServicio2v2.RandomNext = _ => 10;

            MaquinaServicio2v2.ResponderEnvido(mano, "J2");

            Assert.Equal("RealEnvido", mano.TipoEnvidoCantado);
        }

        [Fact]
        public void ResponderEnvido_DesdeRealEnvidoConTanto33_EscalaAFaltaEnvido()
        {
            var mano = CrearManoConEnvidoPendiente("RealEnvido");
            MaquinaServicio2v2.RandomNext = _ => 10;

            MaquinaServicio2v2.ResponderEnvido(mano, "J2");

            Assert.Equal("FaltaEnvido", mano.TipoEnvidoCantado);
        }

        [Fact]
        public void ResponderEnvido_DesdeFaltaEnvido_NoEscalaYDeclara()
        {
            var mano = CrearManoConEnvidoPendiente("FaltaEnvido");
            MaquinaServicio2v2.RandomNext = _ => 10;

            MaquinaServicio2v2.ResponderEnvido(mano, "J2");

            Assert.Equal("declarando_tantos", mano.FaseEnvido);
        }

        [Fact]
        public void ResponderEnvido_ConTantoJusto_AceptaSinEscalar()
        {
            // tanto 28: acepta con tirada baja pero no escala (necesita 30+)
            var mano = CrearManoConEnvidoPendiente("Envido", numeroA: 6, numeroB: 2);
            MaquinaServicio2v2.RandomNext = _ => 0;

            MaquinaServicio2v2.ResponderEnvido(mano, "J2");

            Assert.Equal("declarando_tantos", mano.FaseEnvido);
            Assert.Equal("Envido", mano.TipoEnvidoCantado);
        }

        // ── Consultas del compañero y órdenes ────────────────────────────

        [Fact]
        public void ResolverConsultaEnvido_SinConsulta_LanzaInvalidOperationException()
        {
            var mano = CrearMano();

            Assert.Throws<InvalidOperationException>(() =>
                MaquinaServicio2v2.ResolverConsultaEnvido(mano, aceptar: true, (m, j) => "J2"));
        }

        [Fact]
        public void ResolverConsultaEnvido_AceptarFalse_NoCantaNada()
        {
            var mano = CrearMano();
            mano.CompaConsultaEnvido = true;

            MaquinaServicio2v2.ResolverConsultaEnvido(mano, aceptar: false, (m, j) => "J2");

            Assert.False(mano.CompaConsultaEnvido);
            Assert.True(mano.CompaEnvidoConsultado);
            Assert.False(mano.EnvidoCantado);
        }

        [Fact]
        public void ResolverConsultaEnvido_AceptarTrue_ElCompaneroCantaEnvido()
        {
            var mano = CrearMano();
            mano.CompaConsultaEnvido = true;

            MaquinaServicio2v2.ResolverConsultaEnvido(mano, aceptar: true, (m, j) => "J2");

            Assert.True(mano.EnvidoCantado);
            Assert.Equal("J3", mano.CantorEnvido);
        }

        [Fact]
        public void ResolverConsultaTruco_SinCartas_SoloLimpiaLosFlags()
        {
            var mano = CrearMano();
            mano.CompaConsultaTruco = true;

            MaquinaServicio2v2.ResolverConsultaTruco(mano, voy: true);

            Assert.False(mano.CompaConsultaTruco);
            Assert.True(mano.CompaTrucoConsultado);
        }

        [Fact]
        public void ResolverConsultaTruco_ConVoy_JuegaLaMasBajaYDejaPista()
        {
            var mano = CrearMano();
            mano.CompaConsultaTruco = true;
            mano.TurnoActual = "J3";
            var cMala = new Carta { Numero = 4, Palo = "Basto", ValorTruco = 1 };
            var cBuena = new Carta { Numero = 3, Palo = "Espada", ValorTruco = 10 };
            mano.Posicion3.Mano = new List<Carta> { cBuena, cMala };

            MaquinaServicio2v2.ResolverConsultaTruco(mano, voy: true);

            Assert.NotNull(mano.CompaPista);
            Assert.Equal(cMala, mano.VueltaActual!.CartasJugadas["J3"]);
        }

        [Fact]
        public void ResolverConsultaTruco_ConPongo_JuegaLaMasAlta()
        {
            var mano = CrearMano();
            mano.CompaConsultaTruco = true;
            mano.TurnoActual = "J3";
            mano.EnvidoResuelto = true; // sin pista
            var cMala = new Carta { ValorTruco = 1 };
            var cBuena = new Carta { ValorTruco = 10 };
            mano.Posicion3.Mano = new List<Carta> { cBuena, cMala };

            MaquinaServicio2v2.ResolverConsultaTruco(mano, voy: false);

            Assert.Equal(cBuena, mano.VueltaActual!.CartasJugadas["J3"]);
        }

        [Fact]
        public void OrdenarJugarMayor_JugadorInexistente_LanzaInvalidOperationException()
        {
            var mano = CrearMano();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                MaquinaServicio2v2.OrdenarJugarMayor(mano, "J9"));
            Assert.Contains("no encontrado", ex.Message);
        }

        [Fact]
        public void OrdenarJugarMayor_JugadorRival_LanzaInvalidOperationException()
        {
            var mano = CrearMano();
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 10 } };

            Assert.Throws<InvalidOperationException>(() =>
                MaquinaServicio2v2.OrdenarJugarMayor(mano, "J2"));
        }

        [Fact]
        public void OrdenarJugarMayor_CompaneroSinCartas_LanzaInvalidOperationException()
        {
            var mano = CrearMano();

            var ex = Assert.Throws<InvalidOperationException>(() =>
                MaquinaServicio2v2.OrdenarJugarMayor(mano, "J3"));
            Assert.Contains("no tiene cartas", ex.Message);
        }

        [Fact]
        public void OrdenarJugarMayor_CompaneroValido_GuardaLaOrden()
        {
            var mano = CrearMano();
            mano.Posicion3.Mano = new List<Carta> { new Carta { ValorTruco = 10 } };

            MaquinaServicio2v2.OrdenarJugarMayor(mano, "J3");

            Assert.Equal("J3", mano.OrdenJugarMayor);
        }

        // ── AvanzarUnPaso ────────────────────────────────────────────────

        [Fact]
        public void AvanzarUnPaso_PartidaTerminada_RetornaNull()
        {
            var mano = CrearMano();
            mano.PartidaTerminada = true;

            Assert.Null(MaquinaServicio2v2.AvanzarUnPaso(mano));
        }

        [Fact]
        public void AvanzarUnPaso_TurnoDelHumano_RetornaNull()
        {
            var mano = CrearMano();
            mano.TurnoActual = "J1";

            Assert.Null(MaquinaServicio2v2.AvanzarUnPaso(mano));
        }

        [Fact]
        public void AvanzarUnPaso_ActorInexistente_RetornaNull()
        {
            var mano = CrearMano();
            mano.TurnoActual = "J9";

            Assert.Null(MaquinaServicio2v2.AvanzarUnPaso(mano));
        }

        [Fact]
        public void AvanzarUnPaso_ActorNoEsMaquina_RetornaNull()
        {
            var mano = CrearMano();
            mano.Posicion2.EsMaquina = false;

            Assert.Null(MaquinaServicio2v2.AvanzarUnPaso(mano));
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaNoQuiereElTruco_DevuelveEventoNoQuiero()
        {
            var mano = CrearManoConTrucoPendiente();
            MaquinaServicio2v2.RandomNext = _ => 99;

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("truco-resp", resultado!.Tipo);
            Assert.Equal("¡No quiero!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaEscalaElTruco_DevuelveEventoRetruco()
        {
            var mano = CrearManoConTrucoPendiente();
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };
            MaquinaServicio2v2.RandomNext = _ => 10;

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("truco", resultado!.Tipo);
            Assert.Equal("¡Retruco!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaRespondeEnvidoNoQuiero()
        {
            var mano = CrearManoConEnvidoPendiente("Envido");
            mano.Posicion2.Mano = new List<Carta>();

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("envido-resp", resultado!.Tipo);
            Assert.Contains("No quiero", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaNoQuiereEnvidoYRecuerdaTrucoPendiente()
        {
            var mano = CrearManoConEnvidoPendiente("Envido");
            mano.Posicion2.Mano = new List<Carta>();
            mano.TrucoCantado = true;
            mano.TrucoPendienteRespuestaDe = "J1";

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("¡No quiero! ¿Y el truco?", resultado!.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaEscalaElEnvido_DevuelveEventoEnvido()
        {
            var mano = CrearManoConEnvidoPendiente("Envido");
            MaquinaServicio2v2.RandomNext = _ => 45;

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("envido", resultado!.Tipo);
            Assert.Equal("¡EnvidoEnvido!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaDeclaraTanto_DevuelveElNumero()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TantosReales["J2"] = 27;

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("tanto", resultado!.Tipo);
            Assert.Equal("27", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaDiceSonBuenas_DevuelveEventoSonBuenas()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TantosReales["J2"] = 20;
            mano.TantosDeclarados["J1"] = 30;

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("tanto", resultado!.Tipo);
            Assert.Equal("¡Son buenas!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaJuegaCarta_DevuelveEventoCarta()
        {
            var mano = CrearManoParaJugarCarta();
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 5 } };

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("carta", resultado!.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_CompaneroPideCantarLosTantos()
        {
            var mano = CrearMano();
            mano.TurnoActual = "J3";
            mano.JugadorMano = "J1"; // orden J1,J2,J3,J4 => J3 es el pie de EquipoA
            mano.Posicion3.Mano = ManoConTanto(7, 6);

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("consulta-envido", resultado!.Tipo);
            Assert.True(mano.CompaConsultaEnvido);
            Assert.Equal("Tengo mucho", mano.CompaPista);
        }

        [Fact]
        public void AvanzarUnPaso_CompaneroPreguntaVoyOPongo()
        {
            var mano = CrearMano();
            mano.TurnoActual = "J3";
            mano.JugadorMano = "J3"; // J3 no es pie => no consulta envido
            mano.CompaEnvidoConsultado = true;
            mano.Posicion3.Mano = new List<Carta> { new Carta { ValorTruco = 12 } };

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("consulta-truco", resultado!.Tipo);
            Assert.True(mano.CompaConsultaTruco);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaCantaTrucoYOfreceEnvidoVaPrimero()
        {
            var mano = CrearMano();
            mano.VueltaActual = new Vuelta2v2();
            mano.Posicion1.Jugadas = new List<Carta> { new Carta { ValorTruco = 5 } }; // J1 ya jugó
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 13 } };
            mano.Posicion3.Mano = ManoConTanto(7, 6); // pista "Tengo mucho"

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("truco", resultado!.Tipo);
            Assert.Equal("¡Truco!", resultado.Texto);
            Assert.True(mano.CompaConsultaEnvido); // ofrece el envido "va primero"
            Assert.Equal("Tengo mucho", mano.CompaPista);
        }
    }
}
