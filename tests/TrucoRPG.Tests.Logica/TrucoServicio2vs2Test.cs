using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class TrucoServicio2vs2Test
    {
        //cantar 
        [Fact]
        public void Cantar_DadoQueElJugadorNoPuedeCantar_DebeRetornarFalse()
        {
            // Given
            var jugadorId = "Jugador1";
            var mano = new ManoTruco2v2
            {
                ManoTerminada = true
            };
            Func<ManoTruco2v2, string, string> callbackResponsable = (m, id) => "Rival1";

            // When
            var resultado = TrucoServicio2v2.Cantar(mano, jugadorId, callbackResponsable);

            // Then
            Assert.False(resultado);
        }

        //responder
        [Fact]
        public void Responder_DadoTrucoNoCantado_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2
            {
                TrucoCantado = false,
                TrucoPendienteRespuestaDe = "Jugador1"
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Responder(mano, "Jugador1", true,null, callback);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void Responder_DadoJugadorQueNoLeCorrespondeResponder_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = "Jugador2" 
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Responder(mano, "Jugador1",true, null, callback); 

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void Responder_DadoJugadorRechazaTruco_DebeFinalizarLaManoYAsignarPuntosAlRival()
        {
            // Given
            var jugadorId = "Jugador1";
            var mano = new ManoTruco2v2
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = jugadorId,
                NivelTruco = 2, // Retruco
                EquipoCantorTruco = "EquipoB"
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Responder(mano, jugadorId, false,null, callback);

            // Then
            Assert.True(resultado);
            Assert.Null(mano.TrucoPendienteRespuestaDe);
            Assert.Equal("EquipoB", mano.GanadorMano);
            Assert.Equal(2, mano.PuntosTrucoMano);
        }

        [Fact]
        public void Responder_DadoJugadorAceptaYEscalaValido_DebeLlamarSubirNivelYRetornarTrue()
        {
            // Given
            var jugadorId = "Jugador1";
            var mano = new ManoTruco2v2
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = jugadorId,
                NivelTruco = 1, 
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "JugadorRival";

            // When
            var resultado = TrucoServicio2v2.Responder(mano, jugadorId,true, "retruco", callback);

            // Then
            Assert.True(resultado);
            Assert.Null(mano.TrucoPendienteRespuestaDe);
        }

        [Fact]
        public void Responder_DadoIntentoDeEscalarEnNivelMaximo_DebeIgnorarEscaladaYSoloQuerer()
        {
            // Given
            var jugadorId = "Jugador1";
            var mano = new ManoTruco2v2
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = jugadorId,
                NivelTruco = 3, // Ya está en Vale Cuatro
                PuntosTrucoMano = 4
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Responder(mano, jugadorId,true,"vale cuatro", callback);

            // Then
            Assert.True(resultado);
            Assert.True(mano.TrucoResuelto);
        }

        [Fact]
        public void Responder_DadoJugadorAceptaSinEscalar_DebeResolverTrucoComoQuerido()
        {
            // Given
            var jugadorId = "Jugador1";
            var mano = new ManoTruco2v2
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = jugadorId,
                NivelTruco = 1,
                PuntosTrucoMano = 2
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Responder(mano, jugadorId, true, null, callback);

            // Then
            Assert.True(resultado);
            Assert.Null(mano.TrucoPendienteRespuestaDe);
            Assert.True(mano.TrucoResuelto);
        }

        //escalar
        [Fact]
        public void Escalar_DadoManoConGanadorYaDefinido_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2 { GanadorMano = "EquipoA" };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Escalar(mano, "Jugador1", callback);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void Escalar_DadoManoTerminada_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2 { ManoTerminada = true };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Escalar(mano, "Jugador1", callback);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void Escalar_DadoPartidaTerminada_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2 { PartidaTerminada = true };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Escalar(mano, "Jugador1", callback);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void Escalar_DadoTrucoNoCantadoAun_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2
            {
                TrucoCantado = false,
                TrucoPendienteRespuestaDe = null
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Escalar(mano, "Jugador1", callback);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void Escalar_DadoQueHayUnaRespuestaPendienteEnCurso_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = "Jugador2" // Hay alguien respondiendo ahora mismo
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Escalar(mano, "Jugador1", callback);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void Escalar_DadoNivelDeTrucoMaximo_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2
            {
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = null,
                NivelTruco = 3 
            };
            Func<ManoTruco2v2, string, string> callback = (m, id) => "Otro";

            // When
            var resultado = TrucoServicio2v2.Escalar(mano, "Jugador1", callback);

            // Then
            Assert.False(resultado);
        }

        //irse al mazo
        [Fact]
        public void IrseAlMazo_DadoManoYaTerminada_DebeRetornarFalse()
        {
            // Given
            var mano = new ManoTruco2v2
            {
                ManoTerminada = true
            };

            // When
            var resultado = TrucoServicio2v2.IrseAlMazo(mano, "Jugador1");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IrseAlMazo_DadoJugadorSinTurnoYSinRespuestaPendiente_DebeRetornarFalse()
        {
            // Given
            var jugadorId = "Jugador1";
            var mano = new ManoTruco2v2
            {
                ManoTerminada = false,
                GanadorMano = null,
                PartidaTerminada = false,
                TurnoActual = "Jugador2",              
                TrucoPendienteRespuestaDe = "Jugador3"  
            };

            // When
            var resultado = TrucoServicio2v2.IrseAlMazo(mano, jugadorId);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IrseAlMazo_DadoQueSeVaSinTrucoCantado_DebeDar1PuntoAlRivalYTerminarMano()
        {
            // Given
            var jugadorId = "Jugador2";
            var mano = new ManoTruco2v2
            {
                ManoTerminada = false,
                GanadorMano = null,
                PartidaTerminada = false,
                TurnoActual = jugadorId, 
                TrucoCantado = false,

                Posicion1 = new Jugador { Id = "Jugador1" },
                Posicion2 = new Jugador { Id = jugadorId },
                Posicion3 = new Jugador { Id = "J3" },       
                Posicion4 = new Jugador { Id = "J4" }        
            };

            // When
            var resultado = TrucoServicio2v2.IrseAlMazo(mano, jugadorId);

            // Then
            Assert.True(resultado);
            Assert.Equal("EquipoA", mano.GanadorMano);
            Assert.True(mano.ManoTerminada);
            Assert.True(mano.TrucoResuelto);
        }

        [Fact]
        public void IrseAlMazo_DadoQueSeVaConTrucoPendienteDeRespuesta_DebeValerPuntosDelNivelYTerminarMano()
        {
            // Given
            var jugadorId = "Jugador2"; 
            var mano = new ManoTruco2v2
            {
                ManoTerminada = false,
                GanadorMano = null,
                PartidaTerminada = false,
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = jugadorId, 
                NivelTruco = 2,

                Posicion1 = new Jugador { Id = "Jugador1" }, 
                Posicion2 = new Jugador { Id = jugadorId },
                Posicion3 = new Jugador { Id = "J3" },       
                Posicion4 = new Jugador { Id = "J4" }      
            };

            // When
            var resultado = TrucoServicio2v2.IrseAlMazo(mano, jugadorId);

            // Then
            Assert.True(resultado);
            Assert.Equal("EquipoA", mano.GanadorMano);
            Assert.True(mano.ManoTerminada);
        }

        [Fact]
        public void IrseAlMazo_DadoQueSeVaConTrucoYaQuerido_DebeValerPuntosApostadosYTerminarMano()
        {
            // Given
            var jugadorId = "Jugador2"; 
            var mano = new ManoTruco2v2
            {
                ManoTerminada = false,
                GanadorMano = null,
                PartidaTerminada = false,
                TurnoActual = jugadorId,
                TrucoCantado = true,
                TrucoPendienteRespuestaDe = null,
                PuntosTrucoMano = 3,

                Posicion1 = new Jugador { Id = "Jugador1" }, 
                Posicion2 = new Jugador { Id = jugadorId },   
                Posicion3 = new Jugador { Id = "J3" },       
                Posicion4 = new Jugador { Id = "J4" }        
            };

            // When
            var resultado = TrucoServicio2v2.IrseAlMazo(mano, jugadorId);

            // Then
            Assert.True(resultado);
            Assert.Equal("EquipoA", mano.GanadorMano); 
            Assert.True(mano.ManoTerminada);
        }
    }
}
