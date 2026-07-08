using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de Equipo3v3, TurnoServicio3v3 y PartidaServicio3v3
    /// (rotación de pica-pica incluida).
    /// </summary>
    public class Truco3v3CoverageExtraTests
    {
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
                EquipoB = new Equipo3v3 { Id = "EquipoB", Jugador1 = j2, Jugador2 = j4, Jugador3 = j6 }
            };
        }

        // ── Equipo3v3 ────────────────────────────────────────────────────

        [Fact]
        public void ObtenerJugador_EncuentraACadaMiembroDelEquipo()
        {
            var equipo = new Equipo3v3
            {
                Jugador1 = new Jugador { Id = "J1" },
                Jugador2 = new Jugador { Id = "J3" },
                Jugador3 = new Jugador { Id = "J5" }
            };

            Assert.Equal("J1", equipo.ObtenerJugador("J1")!.Id);
            Assert.Equal("J3", equipo.ObtenerJugador("J3")!.Id);
            Assert.Equal("J5", equipo.ObtenerJugador("J5")!.Id);
            Assert.Null(equipo.ObtenerJugador("J9"));
        }

        // ── TurnoServicio3v3 ─────────────────────────────────────────────

        [Fact]
        public void SiguienteJugador_JugadorInexistente_DevuelveNull()
        {
            Assert.Null(TurnoServicio3v3.SiguienteJugador(CrearMano(), "J9"));
        }

        [Fact]
        public void SiguienteJugador_CasoNormal_DevuelveElQueSigue()
        {
            Assert.Equal("J3", TurnoServicio3v3.SiguienteJugador(CrearMano(), "J2"));
        }

        [Fact]
        public void PuedeEscalarTruco_SinTrucoCantado_DevuelveFalse()
        {
            Assert.False(TurnoServicio3v3.PuedeEscalarTruco(CrearMano(), "J2"));
        }

        [Fact]
        public void PuedeEscalarTruco_SoloElPieDelEquipoContrarioPuede()
        {
            var mano = CrearMano();
            mano.TrucoCantado = true;
            mano.EquipoCantorTruco = "EquipoA";
            mano.JugadorMano = "J3"; // orden J3..J2 => el pie de EquipoB es J2

            Assert.True(TurnoServicio3v3.PuedeEscalarTruco(mano, "J2"));
            Assert.False(TurnoServicio3v3.PuedeEscalarTruco(mano, "J4")); // contrario pero no pie
            Assert.False(TurnoServicio3v3.PuedeEscalarTruco(mano, "J1")); // equipo cantor
        }

        [Fact]
        public void PuedeCantarTruco_CubreTodasLasGuardas()
        {
            Assert.True(TurnoServicio3v3.PuedeCantarTruco(CrearMano(), "J1"));

            var conTruco = CrearMano();
            conTruco.TrucoCantado = true;
            Assert.False(TurnoServicio3v3.PuedeCantarTruco(conTruco, "J1"));

            var resuelto = CrearMano();
            resuelto.TrucoResuelto = true;
            Assert.False(TurnoServicio3v3.PuedeCantarTruco(resuelto, "J1"));

            var ganada = CrearMano();
            ganada.GanadorMano = "EquipoA";
            Assert.False(TurnoServicio3v3.PuedeCantarTruco(ganada, "J1"));

            var terminada = CrearMano();
            terminada.PartidaTerminada = true;
            Assert.False(TurnoServicio3v3.PuedeCantarTruco(terminada, "J1"));
        }

        [Fact]
        public void ObtenerResponsableCanto_RespondeElRivalQueSigueAlCantor()
        {
            Assert.Equal("J3", TurnoServicio3v3.ObtenerResponsableCanto(CrearMano(), "J2"));
            Assert.Equal("J2", TurnoServicio3v3.ObtenerResponsableCanto(CrearMano(), "J1"));
        }

        [Fact]
        public void ObtenerResponsableCanto_CantorInexistente_UsaElPrimerRival()
        {
            Assert.Equal("J1", TurnoServicio3v3.ObtenerResponsableCanto(CrearMano(), "J9"));
        }

        [Fact]
        public void ObtenerResponsableTruco_SiElHumanoEstaActivo_RespondeElHumano()
        {
            var mano = CrearMano();
            mano.JugadoresActivos = new List<string> { "J1", "J2", "J3", "J4", "J5", "J6" };

            Assert.Equal("J1", TurnoServicio3v3.ObtenerResponsableTruco(mano, "EquipoB"));
        }

        // ── PartidaServicio3v3 ───────────────────────────────────────────

        [Fact]
        public void CrearManoNueva_ConJugadoresPropios_LosReutiliza()
        {
            var j1 = new Jugador { Id = "J1", Nombre = "Gonza" };
            var j2 = new Jugador { Id = "J2", EsMaquina = true };
            var j3 = new Jugador { Id = "J3" };
            var j4 = new Jugador { Id = "J4", EsMaquina = true };
            var j5 = new Jugador { Id = "J5" };
            var j6 = new Jugador { Id = "J6", EsMaquina = true };

            var mano = PartidaServicio3v3.CrearManoNueva(1, 3, 4, j1, j2, j3, j4, j5, j6);

            Assert.Equal("Gonza", mano.Posicion1.Nombre);
            Assert.Equal("J1", mano.JugadorMano);
        }

        private static ManoTruco3v3 ProximaMano(int ptsA, int ptsB, int prevSlot)
        {
            var j1 = new Jugador { Id = "J1" };
            var j2 = new Jugador { Id = "J2", EsMaquina = true };
            var j3 = new Jugador { Id = "J3" };
            var j4 = new Jugador { Id = "J4", EsMaquina = true };
            var j5 = new Jugador { Id = "J5" };
            var j6 = new Jugador { Id = "J6", EsMaquina = true };
            return PartidaServicio3v3.CrearProximaMano(2, ptsA, ptsB, prevSlot, j1, j2, j3, j4, j5, j6);
        }

        [Fact]
        public void CrearProximaMano_SinLlegarAlUmbral_EsManoNormal()
        {
            Assert.False(ProximaMano(0, 0, -1).PicaPica);
        }

        [Fact]
        public void CrearProximaMano_AlLlegarAlUmbral_ArrancaElPicaPica()
        {
            Assert.True(ProximaMano(10, 0, -1).PicaPica);
        }

        [Fact]
        public void CrearProximaMano_DentroDelCiclo_SigueElPicaPica()
        {
            Assert.True(ProximaMano(10, 0, 0).PicaPica);
        }

        [Fact]
        public void CrearProximaMano_ElCuartoSlotEsLaManoRedonda()
        {
            Assert.False(ProximaMano(10, 0, 2).PicaPica);
        }

        [Fact]
        public void CrearProximaMano_ConSlotFinal_QuedaEnJuegoNormal()
        {
            Assert.False(ProximaMano(10, 0, -2).PicaPica);
        }

        [Fact]
        public void CrearProximaMano_SobreElUmbralFinal_CierraElCicloPicaPica()
        {
            // El siguiente slot sería 3 y ya se superó el puntaje final => queda normal
            Assert.False(ProximaMano(26, 0, 2).PicaPica);
            // Venía de la redonda (slot 3) y también se corta
            Assert.False(ProximaMano(26, 0, 3).PicaPica);
        }
    }
}
