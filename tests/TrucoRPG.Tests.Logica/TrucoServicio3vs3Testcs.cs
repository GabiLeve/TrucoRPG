using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public  class TrucoServicio3vs3Testcs
    {
        //responder
        [Fact]
        public void Responder_CuandoNoHayTrucoCantadoONoEsElTurnoDelJugador_DeberiaRetornarFalse()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TrucoCantado = false,
                TrucoPendienteRespuestaDe = "J2"
            };
            var jugadorId = "J1"; 

            // When 
            var resultado = TrucoServicio3v3.Responder(mano, jugadorId, true, null, (m, id) => "EquipoA");

            // Then 
            Assert.False(resultado);
        }

        [Fact]
        public void Responder_CuandoElJugadorNoAceptaElTruco_DeberiaTerminarLaManoYAsignarPuntosAlCantor()
        {
            // Given
            var mano = new ManoTruco3v3
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = "J2",
                NivelTruco = 2,
                EquipoCantorTruco = "EquipoA",
                ManoTerminada = false,
                TrucoResuelto = false
            };

            // When 
            var resultado = TrucoServicio3v3.Responder(mano, "J2", false, null, (m, id) => "EquipoA");

            // Then 
            Assert.True(resultado);
            Assert.Null(mano.TrucoPendienteRespuestaDe);
            Assert.True(mano.TrucoResuelto);
        }

        [Fact]
        public void Responder_CuandoElJugadorAceptaSinEscalar_DeberiaMarcarTrucoResueltoYSeguirLaMano()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = "J2",
                PuntosTrucoMano = 2,
                TrucoResuelto = false,
                ManoTerminada = false
            };

            // When
            var resultado = TrucoServicio3v3.Responder(mano, "J2", true, null,(m, id) => "EquipoB");

            // Then 
            Assert.True(resultado);
            Assert.Null(mano.TrucoPendienteRespuestaDe);
            Assert.True(mano.TrucoResuelto);
        }

        [Fact]
        public void Responder_CuandoElJugadorQuiereYEscalaLaApuesta_DeberiaSubirDeNivelYRetornarTrue()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = "J2",
                NivelTruco = 1, 
                TrucoResuelto = false
            };


            // When 
            var resultado = TrucoServicio3v3.Responder(mano, "J2",true,  "retruco", (m, id) => "EquipoB");

            // Then
            Assert.True(resultado);
            Assert.Null(mano.TrucoPendienteRespuestaDe);
        }

        //escalar
        [Fact]
        public void Escalar_CuandoSeCumplenTodasLasCondiciones_DeberiaSubirNivelYRetornarTrue()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = null, 
                NivelTruco = 1, 
                EquipoCantorTruco = "EquipoA",
                TurnoActual = "J2",
                ManoTerminada = false,
                PartidaTerminada = false,
                GanadorMano = null
            };

            // When 
            var resultado = TrucoServicio3v3.Escalar(mano, "J2", (m, id) => "EquipoB");

            // Then 
            Assert.True(resultado);
        }

        [Fact]
        public void Escalar_CuandoElTrucoYaEstaEnNivelMaximo_DeberiaRetornarFalse()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = null,
                NivelTruco = 3,
                EquipoCantorTruco = "EquipoA",
                TurnoActual = "J2",
                ManoTerminada = false,
                PartidaTerminada = false,
                GanadorMano = null
            };

            // When 
            var resultado = TrucoServicio3v3.Escalar(mano, "J2", (m, id) => "EquipoB");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void Escalar_AlInvocarSubirNivel_DeberiaModificarElEstadoInternoDeLaManoCorrectamente()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = null,
                NivelTruco = 1, 
                EquipoCantorTruco = "EquipoA",
                TurnoActual = "J2",
                ManoTerminada = false,
                PartidaTerminada = false,
                GanadorMano = null
            };
            Func<ManoTruco3v3, string, string> responsableMock = (m, id) => "J1";

            // When 
            var resultado = TrucoServicio3v3.Escalar(mano, "J2", responsableMock);

            // Then 
            Assert.True(resultado);

            // Verificaciones de la lógica interna de SubirNivel:
            Assert.Equal(2, mano.NivelTruco); 
            Assert.Equal(3, mano.PuntosTrucoMano); 
        }

        //irse a mazo
        [Fact]
        public void IrseAlMazo_CuandoNoEsTurnoDelJugadorNiTieneRespuestaPendiente_DeberiaRetornarFalse()
        {
            // Given
            var mano = new ManoTruco3v3
            {
                TurnoActual = "J2",
                TrucoPendienteRespuestaDe = null,
                ManoTerminada = false,
                PartidaTerminada = false,
                GanadorMano = null
            };

            // When
            var resultado = TrucoServicio3v3.IrseAlMazo(mano, "J1");

            // Then 
            Assert.False(resultado);
        }

        [Fact]
        public void IrseAlMazo_CuandoNoHayTrucoCantado_DeberiaDarUnPuntoAlRivalYTerminarMano()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TurnoActual = "J3",
                TrucoCantado = false,
                ManoTerminada = false,
                PartidaTerminada = false,
                GanadorMano = null
            };

            // When 
            var resultado = TrucoServicio3v3.IrseAlMazo(mano, "J3");

            // Then 
            Assert.True(resultado);
            Assert.True(mano.ManoTerminada);
        }

        [Fact]
        public void IrseAlMazo_CuandoHayTrucoCantadoSinResponder_DeberiaValerElNivelDelTruco()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TurnoActual = "J2",
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = "J2",
                NivelTruco = 1,
                ManoTerminada = false,
                PartidaTerminada = false,
                GanadorMano = null
            };
            // When
            var resultado = TrucoServicio3v3.IrseAlMazo(mano, "J2");

            // Then 
            Assert.True(resultado);
            Assert.True(mano.ManoTerminada);
            Assert.Equal("EquipoA", mano.GanadorMano);
        }

        [Fact]
        public void IrseAlMazo_CuandoElTrucoYaFueQuerido_DeberiaLlevarseLosPuntosDeLaApuestaActual()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TurnoActual = "J1",
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = null, 
                PuntosTrucoMano = 3, 
                ManoTerminada = false,
                PartidaTerminada = false,
                GanadorMano = null
            };
            // When 
            var resultado = TrucoServicio3v3.IrseAlMazo(mano, "J1");

            // Then 
            Assert.True(resultado);
            Assert.True(mano.ManoTerminada);
            Assert.Equal("EquipoA", mano.GanadorMano);
        }
    }
}
