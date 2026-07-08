using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Controllers;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.API
{
    public class Truco2v2ControllerTest
    {
        //nueva partida
        [Fact]
        public void NuevaPartida_CuandoElRequestTieneDatos_DebeCrearYGuardarLaPartidaConEsosDatos()
        {
            // Given
            var controller = new Truco2v2Controller();
            var request = new Truco2v2NuevaPartidaRequest
            {
                NumeroDeMano = 3,
                PuntosA = 10,
                PuntosB = 12
            };

            // When
            var resultado = controller.NuevaPartida(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var mano = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.Equal(3, mano.NumeroDeMano);
        }

        [Fact]
        public void NuevaPartida_CuandoElRequestEsNulo_DebeCrearLaPartidaConValoresPorDefecto()
        {
            // Given
            var controller = new Truco2v2Controller();
            Truco2v2NuevaPartidaRequest? requestNull = null;

            // When
            var resultado = controller.NuevaPartida(requestNull);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var mano = Assert.IsType<ManoTruco2v2>(okResult.Value);
            Assert.Equal(1, mano.NumeroDeMano);
        }

        [Fact]
        public void NuevaPartida_CuandoLasPropiedadesDelRequestSonNulas_DebeUsarValoresPorDefecto()
        {
            // Given
            var controller = new Truco2v2Controller();
            var requestConNulos = new Truco2v2NuevaPartidaRequest
            {
                NumeroDeMano = null,
                PuntosA = null,
                PuntosB = null
            };

            // When
            var resultado = controller.NuevaPartida(requestConNulos);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var mano = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.Equal(1, mano.NumeroDeMano);
        }

        //jugar carta
        [Fact]
        public void JugarCarta_CuandoLaCartaEsValida_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";

            var request = new Truco2v2CartaRequest(manoId, 7, "Espada");
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                Posicion1 = new Jugador
                {
                    Id = jugadorHumanoId,
                    Nombre = "Vos",
                    EsMaquina = false,
                    Mano = new List<Carta> { new Carta { Numero = 7, Palo = "Espada" } }
                },
                VueltaActual = new Vuelta2v2()
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.JugarCarta(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void JugarCarta_CuandoLaManoNoExisteEnMemoria_DebeLanzarKeyNotFoundException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var idInexistente = Guid.NewGuid();
            var request = new Truco2v2CartaRequest(idInexistente, 1, "Basto");

            // When
            Action act = () => controller.JugarCarta(request);

            // Then
            Assert.Throws<KeyNotFoundException>(act);

        }

        [Fact]
        public void JugarCarta_CuandoElJugadorNoTieneEsaCarta_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2CartaRequest(manoId, 4, "Copa");
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                Posicion1 = new Jugador
                {
                    Id = jugadorHumanoId,
                    Nombre = "Vos",
                    EsMaquina = false,
                    Mano = new List<Carta>()
                },
                VueltaActual = new Vuelta2v2()
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action act = () => controller.JugarCarta(request);
            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        //cantar envido
        [Fact]
        public void CantarEnvido_CuandoElCantoEsValido_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";

            var request = new Truco2v2EnvidoRequest(manoId, "Envido");
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = false,
                EnvidoResuelto = false,
                ManoTerminada = false
            };
            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.CantarEnvido(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void CantarEnvido_CuandoNoSePuedeCantar_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";

            var request = new Truco2v2EnvidoRequest(manoId, "Envido");
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = true,
                EnvidoResuelto = true
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.CantarEnvido(request);
            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //responder envido
        [Fact]
        public void ResponderEnvido_CuandoEsRespuestaSimple_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2ResponderEnvidoRequest(manoId, true, null);

            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = true,
                EnvidoResuelto = false,
                EnvidoPendienteRespuestaDe = jugadorHumanoId,
                FaseEnvido = "pendiente_respuesta",
                EstadoEnvido = "Envido"
            };
            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderEnvido(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void ResponderEnvido_CuandoSeEscalaLaApuesta_DebeLlamarAEscalarYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2ResponderEnvidoRequest(manoId, true, "RealEnvido");

            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = true,
                EnvidoResuelto = false,
                EnvidoPendienteRespuestaDe = jugadorHumanoId,
                FaseEnvido = "pendiente_respuesta"
            };
            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderEnvido(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void ResponderEnvido_CuandoLaAccionEsInvalida_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2ResponderEnvidoRequest(manoId, true, null);

            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = true,
                EnvidoResuelto = true
            };
            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.ResponderEnvido(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //declara tantos
        [Fact]
        public void DeclararTanto_CuandoFaseYTurnoSonValidos_DebeActualizarYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2TantoRequest(manoId, 28);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                FaseEnvido = "declarando_tantos",
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };
            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.DeclararTanto(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void DeclararTanto_CuandoNoEstaEnFaseDeDeclaracion_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2TantoRequest(manoId, 33);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                FaseEnvido = "pendiente_respuesta",
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.DeclararTanto(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void DeclararTanto_CuandoNoEsElTurnoDelJugador_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2TantoRequest(manoId, 20);

            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                FaseEnvido = "declarando_tantos",
                EnvidoPendienteRespuestaDe = "J2"
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.DeclararTanto(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //son buenas
        [Fact]
        public void SonBuenas_CuandoFaseYTurnoSonValidos_DebeActualizarYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2Request(manoId);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                FaseEnvido = "declarando_tantos",
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };
            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.SonBuenas(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void SonBuenas_CuandoNoEstaEnFaseDeDeclaracion_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2Request(manoId);

            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                FaseEnvido = "pendiente_respuesta",
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.SonBuenas(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void SonBuenas_CuandoNoEsElTurnoDelJugador_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2Request(manoId);

            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                FaseEnvido = "declarando_tantos",
                EnvidoPendienteRespuestaDe = "J3"
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.SonBuenas(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //cantar truco
        [Fact]
        public void CantarTruco_CuandoElCantoEsValido_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2Request(manoId);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = false,
                TrucoResuelto = false,
                NivelTruco = 0,
                ManoTerminada = false
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.CantarTruco(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void CantarTruco_CuandoNoSePuedeCantar_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2Request(manoId);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = true,
                TrucoResuelto = true,
                NivelTruco = 1 
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.CantarTruco(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //escalar truco

        [Fact]
        public void EscalarTruco_CuandoNoSePuedeEscalar_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2Request(manoId);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = false, 
                NivelTruco = 0
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.EscalarTruco(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //responder truco
        [Fact]
        public void ResponderTruco_CuandoLaRespuestaEsValida_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2ResponderTrucoRequest(manoId, true, null);

            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = true,
                TrucoResuelto = false,
                NivelTruco = 1,
                EstadoTruco = "Truco",
                TrucoPendienteRespuestaDe = jugadorHumanoId
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderTruco(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void ResponderTruco_CuandoLaAccionEsInvalida_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2ResponderTrucoRequest(manoId, true, null);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = false, 
                ManoTerminada = true  
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action act = () => controller.ResponderTruco(request);

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        //irse al mazo
        [Fact]
        public void IrseAlMazo_CuandoLaAccionEsValida_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2Request(manoId);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                ManoTerminada = false,
                PartidaTerminada = false
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.IrseAlMazo(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void IrseAlMazo_CuandoLaAccionEsInvalida_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco2v2Request(manoId);

            // Seteamos que la mano ya terminó para obligar al servicio a devolver false
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                ManoTerminada = true // Ya terminó, no se puede ir al mazo ahora
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action act = () => controller.IrseAlMazo(request);

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        //nueva mano
        [Fact]
        public void NuevaMano_CuandoLaManoAnteriorTerminoYLaPartidaSigue_DebeCrearLaSiguienteYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco2v2Request(manoId);
            var manoAnteriorSimulada = new ManoTruco2v2
            {
                Id = manoId,
                NumeroDeMano = 1,
                PuntosEquipoA = 12,
                PuntosEquipoB = 8,
                GanadorMano = "EquipoA",   
                PartidaTerminada = false    
            };

            Truco2v2MemoriaServicio.Guardar(manoAnteriorSimulada);

            // When
            var resultado = controller.NuevaMano(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var nuevaMano = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(nuevaMano);
            Assert.Equal(2, nuevaMano.NumeroDeMano); // 1 + 1
        }

        [Fact]
        public void NuevaMano_CuandoLaManoActualAunNoTermino_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco2v2Request(manoId);
            var manoAnteriorSimulada = new ManoTruco2v2
            {
                Id = manoId,
                GanadorMano = null,       
                PartidaTerminada = false
            };

            Truco2v2MemoriaServicio.Guardar(manoAnteriorSimulada);

            // When
            Action action = () => controller.NuevaMano(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void NuevaMano_CuandoLaPartidaYaTermino_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco2v2Request(manoId);
            var manoAnteriorSimulada = new ManoTruco2v2
            {
                Id = manoId,
                GanadorMano = "EquipoB",
                PartidaTerminada = true 
            };

            Truco2v2MemoriaServicio.Guardar(manoAnteriorSimulada);

            // When
            Action action = () => controller.NuevaMano(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //responder consulta envido
        [Fact]
        public void ResponderConsultaEnvido_CuandoSeProcesaLaRespuesta_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1"; 
            var request = new Truco2v2ConsultaEnvidoRequest(manoId, true);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                CompaConsultaEnvido = true,        
                CompaEnvidoConsultado = false,     
                CompaPista = "Tengo mucho"          
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderConsultaEnvido(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        //repsonder consulta truco
        [Fact]
        public void ResponderConsultaTruco_CuandoElCompañeroConsulta_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco2v2ConsultaTrucoRequest(manoId, true);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                CompaConsultaTruco = true,       
                CompaTrucoConsultado = false
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderConsultaTruco(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void ResponderConsultaTruco_CuandoElCompañeroNoConsulta_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco2v2ConsultaTrucoRequest(manoId, true);

            // Dejamos la bandera en 'false' para forzar el lanzamiento de la excepción
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                CompaConsultaTruco = false // No hay consulta activa
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.ResponderConsultaTruco(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //avanzar un paso
        [Fact]
        public void AvanzarMaquina_CuandoSeEjecutaElPaso_DebeActualizarLaMemoriaYRetornarOkConLaRespuesta()
        {
            // Given
            var controller = new Truco2v2Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco2v2Request(manoId);
            var manoSimulada = new ManoTruco2v2
            {
                Id = manoId,
                TurnoActual = "J2", 
                ManoTerminada = false
            };
            manoSimulada.Posicion2 = new Jugador
            {
                Id = "J2",
                Nombre = "Rival 1",
                EsMaquina = true,
                Mano = new List<Carta> { new Carta() }
            };

            Truco2v2MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.AvanzarMaquina(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var pasoResponse = Assert.IsType<Truco2v2PasoResponse>(okResult.Value);

            // Verificamos que se haya guardado el estado actualizado en la memoria simulada
            var manoEnMemoria = Truco2v2MemoriaServicio.Obtener(manoId);
            Assert.NotNull(manoEnMemoria);
        }

    }
}
