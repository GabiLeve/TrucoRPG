using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de TurnoServicio2v2 (responsables y siguiente vuelta)
    /// y de EnvidoEquiposServicio (guardas de cantar/responder/escalar).
    /// </summary>
    public class TurnoYEnvidoEquiposCoverageTests
    {
        // ── Helpers ──────────────────────────────────────────────────────

        private static ManoTruco2v2 CrearMano2v2()
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
                JugadorMano = "J1"
            };
        }

        private static ManoTruco3v3 CrearMano3v3()
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
                EquipoB = new Equipo3v3 { Id = "EquipoB", Jugador1 = j2, Jugador2 = j4, Jugador3 = j6 }
            };
        }

        // ── TurnoServicio2v2 ─────────────────────────────────────────────

        [Fact]
        public void SiguienteJugador_JugadorInexistente_DevuelveNull()
        {
            Assert.Null(TurnoServicio2v2.SiguienteJugador(CrearMano2v2(), "J9"));
        }

        [Fact]
        public void SiguienteJugador_UltimoDelOrden_DevuelveNull()
        {
            Assert.Null(TurnoServicio2v2.SiguienteJugador(CrearMano2v2(), "J4"));
        }

        [Fact]
        public void SiguienteJugador_CasoNormal_DevuelveElQueSigue()
        {
            Assert.Equal("J3", TurnoServicio2v2.SiguienteJugador(CrearMano2v2(), "J2"));
        }

        [Fact]
        public void ObtenerAbreSiguienteVuelta_Parda_AbreElJugadorMano()
        {
            var mano = CrearMano2v2();
            Assert.Equal("J1", TurnoServicio2v2.ObtenerAbreSiguienteVuelta(mano, new Vuelta2v2(), "Parda"));
        }

        [Fact]
        public void ObtenerAbreSiguienteVuelta_AbreElQueJugoLaCartaMasAlta()
        {
            var mano = CrearMano2v2();
            var vuelta = new Vuelta2v2();
            vuelta.CartasJugadas["J2"] = new Carta { ValorTruco = 5 };
            vuelta.CartasJugadas["J4"] = new Carta { ValorTruco = 12 };

            Assert.Equal("J4", TurnoServicio2v2.ObtenerAbreSiguienteVuelta(mano, vuelta, "EquipoB"));
        }

        [Fact]
        public void ObtenerAbreSiguienteVuelta_SinCartasDelGanador_AbreElJugadorMano()
        {
            var mano = CrearMano2v2();
            Assert.Equal("J1", TurnoServicio2v2.ObtenerAbreSiguienteVuelta(mano, new Vuelta2v2(), "EquipoB"));
        }

        [Fact]
        public void PuedeCantarTruco_SinTrucoPrevio_DevuelveTrue()
        {
            Assert.True(TurnoServicio2v2.PuedeCantarTruco(CrearMano2v2(), "J1"));
        }

        [Fact]
        public void PuedeCantarTruco_ConTrucoCantado_DevuelveFalse()
        {
            var mano = CrearMano2v2();
            mano.TrucoCantado = true;
            Assert.False(TurnoServicio2v2.PuedeCantarTruco(mano, "J1"));
        }

        [Fact]
        public void PuedeCantarTruco_ConManoGanada_DevuelveFalse()
        {
            var mano = CrearMano2v2();
            mano.GanadorMano = "EquipoA";
            Assert.False(TurnoServicio2v2.PuedeCantarTruco(mano, "J1"));
        }

        [Fact]
        public void PuedeCantarTruco_ConPartidaTerminada_DevuelveFalse()
        {
            var mano = CrearMano2v2();
            mano.PartidaTerminada = true;
            Assert.False(TurnoServicio2v2.PuedeCantarTruco(mano, "J1"));
        }

        [Fact]
        public void ObtenerResponsableTruco_SiElContrarioIncluyeAlHumano_RespondeElHumano()
        {
            Assert.Equal("J1", TurnoServicio2v2.ObtenerResponsableTruco(CrearMano2v2(), "EquipoB"));
        }

        [Fact]
        public void ObtenerResponsableTruco_SiCantaElEquipoDelHumano_RespondeElRivalSiguiente()
        {
            Assert.Equal("J2", TurnoServicio2v2.ObtenerResponsableTruco(CrearMano2v2(), "EquipoA"));
        }

        [Fact]
        public void ObtenerResponsableCanto_RespondeElRivalQueSigueAlCantor()
        {
            Assert.Equal("J3", TurnoServicio2v2.ObtenerResponsableCanto(CrearMano2v2(), "J2"));
            Assert.Equal("J2", TurnoServicio2v2.ObtenerResponsableCanto(CrearMano2v2(), "J1"));
        }

        [Fact]
        public void ObtenerResponsableCanto_CantorInexistente_UsaElPrimerRival()
        {
            // "J9" no pertenece a EquipoA => se lo trata como EquipoB; responde el primer EquipoA
            Assert.Equal("J1", TurnoServicio2v2.ObtenerResponsableCanto(CrearMano2v2(), "J9"));
        }

        // ── EnvidoEquiposServicio: PuedeCantarEnvido ─────────────────────

        [Fact]
        public void PuedeCantarEnvido_EnManoLimpia_DevuelveTrue()
        {
            Assert.True(EnvidoEquiposServicio.PuedeCantarEnvido(CrearMano3v3(), "J2"));
        }

        [Fact]
        public void PuedeCantarEnvido_ConEnvidoYaCantado_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            mano.EnvidoCantado = true;
            Assert.False(EnvidoEquiposServicio.PuedeCantarEnvido(mano, "J2"));
        }

        [Fact]
        public void PuedeCantarEnvido_ConPartidaTerminada_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            mano.PartidaTerminada = true;
            Assert.False(EnvidoEquiposServicio.PuedeCantarEnvido(mano, "J2"));
        }

        [Fact]
        public void PuedeCantarEnvido_SiElJugadorNoEstaActivo_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            mano.JugadoresActivos = new List<string> { "J1", "J4" }; // pica-pica
            Assert.False(EnvidoEquiposServicio.PuedeCantarEnvido(mano, "J2"));
        }

        [Fact]
        public void PuedeCantarEnvido_ConVueltasJugadas_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            mano.Vueltas.Add(new Vuelta3v3());
            Assert.False(EnvidoEquiposServicio.PuedeCantarEnvido(mano, "J2"));
        }

        [Fact]
        public void PuedeCantarEnvido_SiElJugadorYaJugoCarta_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            mano.Posicion2.Jugadas.Add(new Carta { ValorTruco = 5 });
            Assert.False(EnvidoEquiposServicio.PuedeCantarEnvido(mano, "J2"));
        }

        [Fact]
        public void PuedeCantarEnvido_ConTrucoRivalNivel1SinResolver_ElEnvidoVaPrimero()
        {
            var mano = CrearMano3v3();
            mano.TrucoCantado = true;
            mano.NivelTruco = 1;
            mano.TrucoResuelto = false;
            mano.EquipoCantorTruco = "EquipoA"; // cantó el rival de J2
            Assert.True(EnvidoEquiposServicio.PuedeCantarEnvido(mano, "J2"));
        }

        [Fact]
        public void PuedeCantarEnvido_ConTrucoPropioCantado_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            mano.TrucoCantado = true;
            mano.NivelTruco = 1;
            mano.TrucoResuelto = false;
            mano.EquipoCantorTruco = "EquipoB"; // su propio equipo cantó
            Assert.False(EnvidoEquiposServicio.PuedeCantarEnvido(mano, "J2"));
        }

        [Fact]
        public void PuedeCantarEnvido_ConRetrucoCantado_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            mano.TrucoCantado = true;
            mano.NivelTruco = 2;
            mano.TrucoResuelto = false;
            mano.EquipoCantorTruco = "EquipoA";
            Assert.False(EnvidoEquiposServicio.PuedeCantarEnvido(mano, "J2"));
        }

        // ── EnvidoEquiposServicio: Cantar / Responder / Escalar ──────────

        [Fact]
        public void Cantar_SiNoPuede_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            mano.EnvidoCantado = true;

            Assert.False(EnvidoEquiposServicio.Cantar(mano, "J2", "Envido", _ => "J1"));
        }

        [Fact]
        public void Cantar_CasoValido_DejaElEnvidoPendiente()
        {
            var mano = CrearMano3v3();

            bool resultado = EnvidoEquiposServicio.Cantar(mano, "J2", "Real Envido", _ => "J1");

            Assert.True(resultado);
            Assert.True(mano.EnvidoCantado);
            Assert.Equal("RealEnvido", mano.TipoEnvidoCantado);
            Assert.Equal(3, mano.PuntosEnvido);
            Assert.Equal("J1", mano.EnvidoPendienteRespuestaDe);
            Assert.Equal("pendiente_respuesta", mano.FaseEnvido);
        }

        private static ManoTruco3v3 CrearManoConEnvidoCantado()
        {
            var mano = CrearMano3v3();
            EnvidoEquiposServicio.Cantar(mano, "J2", "Envido", _ => "J1");
            return mano;
        }

        [Fact]
        public void Responder_SinEnvidoCantado_DevuelveFalse()
        {
            var mano = CrearMano3v3();
            Assert.False(EnvidoEquiposServicio.Responder(mano, "J1", true,
                () => new List<string> { "J1", "J2" }, (e, p) => { }));
        }

        [Fact]
        public void Responder_JugadorEquivocado_DevuelveFalse()
        {
            var mano = CrearManoConEnvidoCantado();
            Assert.False(EnvidoEquiposServicio.Responder(mano, "J4", true,
                () => new List<string> { "J1", "J2" }, (e, p) => { }));
        }

        [Fact]
        public void Responder_NoQuiero_ElCantorGanaLosPuntosDelRechazo()
        {
            var mano = CrearManoConEnvidoCantado();
            int puntosSumados = 0;
            string? equipoGanador = null;

            bool resultado = EnvidoEquiposServicio.Responder(mano, "J1", false,
                () => new List<string> { "J1", "J2" }, (e, p) => { equipoGanador = e; puntosSumados = p; });

            Assert.True(resultado);
            Assert.True(mano.EnvidoResuelto);
            Assert.Equal("resuelto", mano.FaseEnvido);
            Assert.Equal("EquipoB", mano.GanadorEnvido);
            Assert.Equal("EquipoB", equipoGanador);
            Assert.Equal(1, puntosSumados);
        }

        [Fact]
        public void Responder_Quiero_IniciaLaDeclaracionDeTantos()
        {
            var mano = CrearManoConEnvidoCantado();

            bool resultado = EnvidoEquiposServicio.Responder(mano, "J1", true,
                () => new List<string> { "J1", "J2", "J3", "J4", "J5", "J6" }, (e, p) => { });

            Assert.True(resultado);
            Assert.Equal("declarando_tantos", mano.FaseEnvido);
            Assert.Equal("J1", mano.EnvidoPendienteRespuestaDe);
        }

        [Fact]
        public void Escalar_AUnTipoMenorOIgual_DevuelveFalse()
        {
            var mano = CrearManoConEnvidoCantado();
            mano.TipoEnvidoCantado = "RealEnvido";

            Assert.False(EnvidoEquiposServicio.Escalar(mano, "J1", "Envido", _ => "J2"));
        }

        [Fact]
        public void Escalar_AFaltaEnvido_DejaLosPuntosEnCero()
        {
            var mano = CrearManoConEnvidoCantado();

            bool resultado = EnvidoEquiposServicio.Escalar(mano, "J1", "Falta Envido", _ => "J2");

            Assert.True(resultado);
            Assert.Equal("FaltaEnvido", mano.TipoEnvidoCantado);
            Assert.Equal(0, mano.PuntosEnvido);
            Assert.Equal(2, mano.PuntosEnvidoNoQuiero); // lo que valía el Envido aceptado
            Assert.Equal("J1", mano.CantorEnvido);
        }

        [Fact]
        public void ResolverNoQuiero_SinCantor_NoHaceNada()
        {
            var mano = CrearMano3v3();
            bool sumo = false;

            EnvidoEquiposServicio.ResolverNoQuiero(mano, (e, p) => sumo = true);

            Assert.False(sumo);
            Assert.False(mano.EnvidoResuelto);
        }

        [Fact]
        public void DebeDeclararSonBuenas_CubreLosTresCasos()
        {
            Assert.False(MaquinaServicio2v2.DebeDeclararSonBuenas(30, null));
            Assert.True(MaquinaServicio2v2.DebeDeclararSonBuenas(20, 27));
            Assert.False(MaquinaServicio2v2.DebeDeclararSonBuenas(27, 28));
        }
    }
}
