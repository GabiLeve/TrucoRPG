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
    public class Truco3v3ControllerTest
    {
        //nueva partida
        [Fact]
        public void NuevaPartida_CuandoSeEnvianValoresEnElRequest_DebeCrearLaManoConEsosValoresYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var request = new Truco3v3NuevaPartidaRequest(NumeroDeMano: 3, PuntosA: 10, PuntosB: 14);

            // When
            var resultado = controller.NuevaPartida(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoCreada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            var manoEnMemoria = Truco3v3MemoriaServicio.Obtener(manoCreada.Id);
            Assert.NotNull(manoEnMemoria);
        }

        [Fact]
        public void NuevaPartida_CuandoElRequestEsNull_DebeCrearLaManoConValoresPorDefectoYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();

            Truco3v3NuevaPartidaRequest? request = null;

            // When
            var resultado = controller.NuevaPartida(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoCreada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoCreada);

        }

        //jugar cartas
        [Fact]
        public void JugarCarta_CuandoEstadoYCartaSonValidos_DebeActualizarMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3CartaRequest(manoId, 7, "Espada");

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                ManoTerminada = false,
                PartidaTerminada = false,
                TrucoPendienteRespuestaDe = null,
                EnvidoPendienteRespuestaDe = null,
                TurnoActual = jugadorHumanoId,
                GanadorMano = null
            };

            manoSimulada.Posicion1 = new Jugador
            {
                Id = jugadorHumanoId,
                Mano = new List<Carta> { new Carta { Numero = 7, Palo = "Espada" } }
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.JugarCarta(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void JugarCarta_CuandoLaManoYaTermino_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3CartaRequest(manoId, 1, "Ancho");

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                ManoTerminada = true
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.JugarCarta(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void JugarCarta_CuandoTieneTrucoPendiente_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3CartaRequest(manoId, 1, "Ancho");

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                ManoTerminada = false,
                PartidaTerminada = false,
                TrucoPendienteRespuestaDe = jugadorHumanoId // El rival le cantó truco a J1
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action act = () => controller.JugarCarta(request);

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void JugarCarta_CuandoTieneEnvidoPendiente_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3CartaRequest(manoId, 1, "Ancho");

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                ManoTerminada = false,
                PartidaTerminada = false,
                TrucoPendienteRespuestaDe = null,
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action act = () => controller.JugarCarta(request);

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void JugarCarta_CuandoNoEsElTurnoDelJugador_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3CartaRequest(manoId, 1, "Ancho");

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                ManoTerminada = false,
                PartidaTerminada = false,
                TrucoPendienteRespuestaDe = null,
                EnvidoPendienteRespuestaDe = null,
                TurnoActual = "J2" // Le toca a una máquina, no a J1
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.JugarCarta(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void JugarCarta_CuandoLaCartaNoExisteEnLaMano_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";

            // Pide tirar un 4 de Copas...
            var request = new Truco3v3CartaRequest(manoId, 4, "Copa");

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                ManoTerminada = false,
                PartidaTerminada = false,
                TurnoActual = jugadorHumanoId
            };

            // ...pero J1 sólo tiene un Ancho de Bastos en la mano
            manoSimulada.Posicion1 = new Jugador
            {
                Id = jugadorHumanoId,
                Mano = new List<Carta> { new Carta { Numero = 1, Palo = "Basto" } }
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.JugarCarta(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //cantar envido
        [Fact]
        public void CantarEnvido_CuandoElCantoEsValido_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3EnvidoRequest(manoId, "Envido");
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = false,
                EnvidoResuelto = false,
                FaseEnvido = "inicial",
                NumeroDeMano = 1
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.CantarEnvido(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        //responder envido
        [Fact]
        public void ResponderEnvido_CuandoEsUnaRespuestaSimpleValida_DeDeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3ResponderEnvidoRequest(manoId, true, null);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = true,
                EnvidoResuelto = false,
                EnvidoPendienteRespuestaDe = jugadorHumanoId,
                FaseEnvido = "pendiente_respuesta"
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderEnvido(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void ResponderEnvido_CuandoSeEscalaElEnvidoCorrectamente_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";

            var request = new Truco3v3ResponderEnvidoRequest(manoId, true, "RealEnvido");

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = true,
                EnvidoResuelto = false,
                TipoEnvidoCantado = "Envido",
                EnvidoPendienteRespuestaDe = jugadorHumanoId,
                FaseEnvido = "pendiente_respuesta"
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderEnvido(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void ResponderEnvido_CuandoLaAccionNoEsValida_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3ResponderEnvidoRequest(manoId, true, null);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                EnvidoCantado = true,// Ya terminó, responder ahora es ilegal
                FaseEnvido = "declarado_final"
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.ResponderEnvido(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //declara tantos 
        [Fact]
        public void DeclararTanto_CuandoFaseYTurnoSonValidos_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3TantoRequest(manoId, 28);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                FaseEnvido = "declarando_tantos",
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.DeclararTanto(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void DeclararTanto_CuandoLaFaseNoEsDeclarandoTantos_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3TantoRequest(manoId, 33);

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                FaseEnvido = "pendiente_respuesta",
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.DeclararTanto(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void DeclararTanto_CuandoNoEsElTurnoDelJugador_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3TantoRequest(manoId, 20);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                FaseEnvido = "declarando_tantos",
                EnvidoPendienteRespuestaDe = "J2"
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.DeclararTanto(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //son buenas
        [Fact]
        public void SonBuenas_CuandoFaseYTurnoSonValidos_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3Request(manoId);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                FaseEnvido = "declarando_tantos",
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.SonBuenas(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void SonBuenas_CuandoLaFaseNoEsDeclarandoTantos_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3Request(manoId);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                FaseEnvido = "inicial",
                EnvidoPendienteRespuestaDe = jugadorHumanoId
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.SonBuenas(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void SonBuenas_CuandoNoEsElTurnoDelJugador_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3Request(manoId);

            // La fase está bien, pero el turno de responder o cantar el tanto es de tu compañero (J3) o un rival
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                FaseEnvido = "declarando_tantos",
                EnvidoPendienteRespuestaDe = "J3" // No es J1
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

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
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3Request(manoId);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = false,
                TrucoResuelto = false,
                NivelTruco = 0,
                ManoTerminada = false
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.CantarTruco(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void CantarTruco_CuandoElCantoNoEsValido_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3Request(manoId);

            // Rompemos las precondiciones: El truco ya se cantó y se aceptó
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = true,
                TrucoResuelto = true,
                NivelTruco = 1 // Ya está en nivel Truco (vale 2 puntos)
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.CantarTruco(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //escalr truco
        [Fact]
        public void EscalarTruco_CuandoElCantoEsValido_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3Request(manoId);

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                ManoTerminada = false,
                TrucoCantado = true,
                TrucoResuelto = true,
                NivelTruco = 1,
                PuntosTrucoMano = 2,
                PuedeEscalarTruco = jugadorHumanoId
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.EscalarTruco(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void EscalarTruco_CuandoElCantoNoEsValido_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3Request(manoId);

            // Forzamos un estado inválido: la mano ya finalizó
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                ManoTerminada = true,      // Mano cerrada
                PartidaTerminada = false
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

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
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3ResponderTrucoRequest(manoId, true, null);

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = true,
                TrucoResuelto = false,
                TrucoPendienteRespuestaDe = jugadorHumanoId,
                ManoTerminada = false
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderTruco(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void ResponderTruco_CuandoLaRespuestaNoEsValida_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3ResponderTrucoRequest(manoId, true, null);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                TrucoCantado = true,
                TrucoResuelto = true,
                TrucoPendienteRespuestaDe = null
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.ResponderTruco(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //irse a mazo
        [Fact]
        public void IrseAlMazo_CuandoLaAccionEsValida_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3Request(manoId);

            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                ManoTerminada = false,
                PartidaTerminada = false,
                GanadorMano = null
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.IrseAlMazo(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        [Fact]
        public void IrseAlMazo_CuandoLaManoYaTermino_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3Request(manoId);

            // Forzamos un estado inválido: la mano o partida ya finalizó
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                ManoTerminada = true, // Mano finalizada
                PartidaTerminada = false
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.IrseAlMazo(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //nueva mano
        [Fact]
        public void NuevaMano_CuandoLaManoAnteriorTerminoYLaPartidaSigue_DebeCrearLaSiguienteYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3Request(manoId);
            var manoAnteriorSimulada = new ManoTruco3v3
            {
                Id = manoId,
                NumeroDeMano = 2,
                PuntosEquipoA = 18,
                PuntosEquipoB = 22,
                GanadorMano = "EquipoB",
                PartidaTerminada = false,
                PicaPicaSlot = 1
            };

            Truco3v3MemoriaServicio.Guardar(manoAnteriorSimulada);

            // When
            var resultado = controller.NuevaMano(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var nuevaMano = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(nuevaMano);
            Assert.Equal(3, nuevaMano.NumeroDeMano);
        }

        [Fact]
        public void NuevaMano_CuandoLaManoActualAunNoTermino_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3Request(manoId);

            // Dejamos GanadorMano en null para simular que la mano sigue en juego
            var manoAnteriorSimulada = new ManoTruco3v3
            {
                Id = manoId,
                GanadorMano = null, // Sigue jugándose
                PartidaTerminada = false
            };

            Truco3v3MemoriaServicio.Guardar(manoAnteriorSimulada);

            // When
            Action action = () => controller.NuevaMano(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void NuevaMano_CuandoLaPartidaYaTermino_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3Request(manoId);
            var manoAnteriorSimulada = new ManoTruco3v3
            {
                Id = manoId,
                GanadorMano = "EquipoA",
                PartidaTerminada = true
            };

            Truco3v3MemoriaServicio.Guardar(manoAnteriorSimulada);

            // When
            Action action = () => controller.NuevaMano(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        // responder consulta envido
        [Fact]
        public void ResponderConsultaEnvido_CuandoSeProcesaLaRespuesta_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string jugadorHumanoId = "J1";
            var request = new Truco3v3ConsultaEnvidoRequest(manoId, true);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = jugadorHumanoId,
                CompaConsultaEnvido = true,
                CompaEnvidoConsultado = false,
                CompaPista = "Tengo 29 para el tanto"
            };

            manoSimulada.Posicion1 = new Jugador { Id = jugadorHumanoId, EsMaquina = false };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderConsultaEnvido(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        //responder consulta truco
        [Fact]
        public void ResponderConsultaTruco_CuandoSeProcesaLaRespuesta_DebeActualizarLaMemoriaYRetornarOk()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3ConsultaTrucoRequest(manoId, true);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                CompaConsultaTruco = true,
                CompaTrucoConsultado = false
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.ResponderConsultaTruco(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var manoRetornada = Assert.IsType<ManoTruco3v3>(okResult.Value);

            Assert.NotNull(manoRetornada);
        }

        //ordenar por mayor
        [Fact]
        public void OrdenarMayor_CuandoElJugadorEsRivalONoEsMaquina_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string rivalId = "J2";
            var request = new Truco3v3OrdenMayorRequest(manoId, rivalId);

            var manoSimulada = new ManoTruco3v3 { Id = manoId };
            manoSimulada.Posicion2 = new Jugador
            {
                Id = rivalId,
                EsMaquina = true,
                Mano = new List<Carta> { new Carta { Numero = 7, Palo = "Basto" } }
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            Action action = () => controller.OrdenarMayor(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void OrdenarMayor_CuandoElBotNoTieneCartas_DebeLanzarInvalidOperationException()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            string botCompaneroId = "J3";
            var request = new Truco3v3OrdenMayorRequest(manoId, botCompaneroId);

            var manoSimulada = new ManoTruco3v3 { Id = manoId };
            manoSimulada.Posicion3 = new Jugador
            {
                Id = botCompaneroId,
                EsMaquina = true,
                Mano = new List<Carta>()
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            //When
            Action action = () => controller.OrdenarMayor(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //avanzar maquina
        [Fact]
        public void AvanzarMaquina_CuandoSeEjecutaElPaso_DebeActualizarLaMemoriaYRetornarOkConLaRespuesta()
        {
            // Given
            var controller = new Truco3v3Controller();
            var manoId = Guid.NewGuid();
            var request = new Truco3v3Request(manoId);
            var manoSimulada = new ManoTruco3v3
            {
                Id = manoId,
                TurnoActual = "J2",
                ManoTerminada = false,
                PartidaTerminada = false
            };
            manoSimulada.Posicion2 = new Jugador
            {
                Id = "J2",
                Nombre = "Bot Rival 1",
                EsMaquina = true,
                Mano = new List<Carta> { new Carta { Numero = 1, Palo = "Basto" } }
            };

            Truco3v3MemoriaServicio.Guardar(manoSimulada);

            // When
            var resultado = controller.AvanzarMaquina(request);

            // Then
            var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
            var pasoResponse = Assert.IsType<Truco3v3PasoResponse>(okResult.Value);
            Assert.NotNull(pasoResponse);
            Assert.NotNull(pasoResponse.Mano);
        }
    }
}
