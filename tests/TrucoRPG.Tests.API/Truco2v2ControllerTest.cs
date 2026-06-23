using System;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Controllers;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using Xunit;

namespace TrucoRPG.Tests.API
{
    public class Truco2v2ControllerTest
    {
        private readonly Truco2v2Controller _controller = new();

        private ManoTruco2v2 ObtenerManoNueva()
        {
            var result = _controller.NuevaPartida(null);

            var ok = Assert.IsType<OkObjectResult>(result.Result);

            return Assert.IsType<ManoTruco2v2>(ok.Value);
        }

        // NUEVA PARTIDA --------------------------------------------------------
        [Fact]
        public void NuevaPartida_SinRequest_DevuelveOkConManoValida()
        {
            // Given
            var controller = new Truco2v2Controller();

            // When
            var mano = ObtenerManoNueva;

            // Then
            Assert.NotNull(mano);
        }

        [Fact]
        public void NuevaPartida_RequestNull_UsaValoresPorDefecto()
        {
            // Given
            var controller = new Truco2v2Controller();

            // When
            var mano = ObtenerManoNueva();

            // Then
            Assert.Equal(1, mano.NumeroDeMano);
        }

        [Fact]
        public void NuevaPartida_RequestConValores_UsaLosValoresRecibidos()
        {
            // Given
            var request = new Truco2v2NuevaPartidaRequest
            {
                NumeroDeMano = 5,
                PuntosA = 10,
                PuntosB = 8
            };

            // When
            var result = _controller.NuevaPartida(request);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var mano = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.Equal(5, mano.NumeroDeMano);
            Assert.Equal(10, mano.PuntosEquipoA);
            Assert.Equal(8, mano.PuntosEquipoB);
        }

        [Fact]
        public void NuevaPartida_CreaLosCuatroJugadores()
        {
            // Given
            var controller = new Truco2v2Controller();

            // When
            var mano = ObtenerManoNueva();

            // Then

            Assert.Equal("Vos", mano.Posicion1.Nombre);
            Assert.Equal("Rival 1", mano.Posicion2.Nombre);
            Assert.Equal("Compañero", mano.Posicion3.Nombre);
            Assert.Equal("Rival 2", mano.Posicion4.Nombre);
        }

        // JUGAR CARTA  ----------------------------------------------------------------------
        [Fact]
        public void JugarCarta_CartaValidaDevuelveOk()
        {
            // Given
            var mano = ObtenerManoNueva();

            Truco2v2MemoriaServicio.Guardar(mano);

            var carta = mano.ObtenerJugador("J1")!.Mano[0];

            var request = new Truco2v2CartaRequest(
                   mano.Id,
                   carta.Numero,
                   carta.Palo
     );

            // When
            var result = _controller.JugarCarta(request);

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<ManoTruco2v2>(ok.Value);
        }

        [Fact]
        public void JugarCarta_CartaNoExisteLanzaExcepcion()
        {
            // Given
            var mano = ObtenerManoNueva();

            Truco2v2MemoriaServicio.Guardar(mano);

            var request = new Truco2v2CartaRequest(
                 mano.Id,
                 99,
                 "Espada"
            );
            // When
            Action act = () => _controller.JugarCarta(request);

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void JugarCarta_ManoInexistenteLanzaKeyNotFoundException()
        {
            // Given
            var request = new Truco2v2CartaRequest
            (
                Guid.NewGuid(),
                1,
                "Espada"
            );

            // When
            Action act = () => _controller.JugarCarta(request);

            // Then
            Assert.Throws<KeyNotFoundException>(act);
        }


        [Fact]
        public void JugarCarta_NoEsTurnoHumanoLanzaExcepcion()
        {
            // Given
            var mano = ObtenerManoNueva();
            mano.TurnoActual = "J2";

            Truco2v2MemoriaServicio.Guardar(mano);

            var carta = mano.ObtenerJugador("J1")!.Mano[0];

            var request = new Truco2v2CartaRequest
            (
                mano.Id,
                carta.Numero,
                carta.Palo
            );

            // When
            Action action = () => _controller.JugarCarta(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }
        [Fact]
        public void JugarCarta_TrucoPendienteLanzaExcepcion()
        {
            // Given
            var mano = ObtenerManoNueva();

            mano.TrucoPendienteRespuestaDe = "J1";

            Truco2v2MemoriaServicio.Guardar(mano);

            var carta = mano.ObtenerJugador("J1")!.Mano[0];

            var request = new Truco2v2CartaRequest
            (
                mano.Id,
                carta.Numero,
                carta.Palo
            );

            // When
            Action act = () => _controller.JugarCarta(request);
            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void JugarCarta_EnvidoPendienteLanzaExcepcion()
        {
            // Given
            var mano = ObtenerManoNueva();

            mano.EnvidoPendienteRespuestaDe = "J1";

            Truco2v2MemoriaServicio.Guardar(mano);

            var carta = mano.ObtenerJugador("J1")!.Mano[0];

            var request = new Truco2v2CartaRequest(
                mano.Id,
                carta.Numero,
                carta.Palo
            );

            // When
            Action action = () => _controller.JugarCarta(request);
            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        //   ENVIDO CANTAR---------------------------------------------------------
        [Fact]
        public void CantarEnvido_ManoInexistenteLanzaKeyNotFoundException()
        {
            // Given
            var request = new Truco2v2EnvidoRequest(
                Guid.NewGuid(),
                "Envido"
            );

            // When
            Action action = () => _controller.CantarEnvido(request);

            // Then
            Assert.Throws<KeyNotFoundException>(action);
        }

        [Fact]
        public void CantarEnvido_CantoExitosoLanzaOk()
        {
            //Given
            var mano = ObtenerManoNueva();

            var request = new Truco2v2EnvidoRequest(
                mano.Id,
                "Envido"
                );

            //When
            var result = _controller.CantarEnvido(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);
        }

        [Fact]
        public void Cantarenvido_EnvidoInvalidoLanzarInvalidOperationException()
        {
            //Given
            var mano = ObtenerManoNueva();

            _controller.CantarEnvido(new Truco2v2EnvidoRequest(mano.Id, "Envido"));

            var request = new Truco2v2EnvidoRequest(
                mano.Id,
                "Envido"
                );

            // When
            Action action = () => _controller.CantarEnvido(request);

            // Then
            Assert.Throws<InvalidOperationException>(action);
        }

        // RESPONDER ENVIDO -------------------------------------------
        [Fact]
        public void ResponderEnvido_RespuestaExitosaRealEnvidoLanzarOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.EnvidoCantado = true;
            mano.EnvidoResuelto = false;
            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J1";
            mano.TipoEnvidoCantado = "Envido";

            var request = new Truco2v2ResponderEnvidoRequest(
                mano.Id,
                true,
                "RealEnvido"
                );

            //When
            var result = _controller.ResponderEnvido(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.Equal("RealEnvido", manoResultado.TipoEnvidoCantado);

        }

        [Fact]
        public void ResponderEnvido_RespuestaExitosaAceptaElEnvidoLanzarOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.EnvidoCantado = true;
            mano.EnvidoResuelto = false;
            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J1";
            mano.TipoEnvidoCantado = "Envido";

            var request = new Truco2v2ResponderEnvidoRequest(
                mano.Id,
                true,
                null
                );

            //When
            var result = _controller.ResponderEnvido(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);
        }

        [Fact]
        public void ResponderEnvido_RespuestaExitosaRechazaEnvidoLanzaOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.EnvidoCantado = true;
            mano.EnvidoResuelto = false;
            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J1";
            mano.TipoEnvidoCantado = "Envido";

            var request = new Truco2v2ResponderEnvidoRequest(
                mano.Id,
                false,
                null
                );

            //When
            var result = _controller.ResponderEnvido(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);
        }

        [Fact]
        public void RespuestaEnvido_RespuestaFallaSinEscalarLanzarExcepcion()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.EnvidoCantado = true;
            mano.EnvidoResuelto = false;
            mano.FaseEnvido = "aceptado";
            mano.EnvidoPendienteRespuestaDe = "J1";
            mano.TipoEnvidoCantado = "Envido";

            var request = new Truco2v2ResponderEnvidoRequest(
                mano.Id,
                true,
                "Envido"
                );

            //When
            Action action = () => _controller.ResponderEnvido(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ResponderEnvido_RespuestaFallaLazarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.EnvidoCantado = false;

            Truco2v2MemoriaServicio.Guardar(mano);

            var request = new Truco2v2ResponderEnvidoRequest(
                mano.Id,
                false,
                "Envido"
                );

            //When
            Action action = () => _controller.ResponderEnvido(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);

        }

        // ENVIDO DECLARA TANTOS -------------------------------------------------------------------------
        [Fact]
        public void DeclararTantos_ManoInexistenteLanzarException()
        {
            // Given
            var request = new Truco2v2TantoRequest(
                Guid.NewGuid(),
                2
            );

            // When
            Action action = () => _controller.DeclararTanto(request);

            // Then
            Assert.Throws<KeyNotFoundException>(action);
        }

        [Fact]
        public void DeclaraTantos_FaseEnvidoIncorrectaLanzarInvalidOperationException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.FaseEnvido = "aceptado";

            var request = new Truco2v2TantoRequest(
                mano.Id,
                2
                );

            //When
            Action action = () => _controller.DeclararTanto(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void DeclararTantos_EnvidoPendienteRespuestaIncorrectaLanzarInvalidOperationException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.EnvidoPendienteRespuestaDe = "J2";

            var request = new Truco2v2TantoRequest(
                mano.Id,
                2
                );

            //When
            Action action = () => _controller.DeclararTanto(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);

        }

        [Fact]
        public void DeclaraTantos_CaminoCorrectaLanzarOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J1";

            var request = new Truco2v2TantoRequest(
                mano.Id,
                2
                );

            //When
            var result = _controller.DeclararTanto(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);
        }

        // ENVIDO BUENAS -------------------------------------------------------------------------------------
        [Fact]
        public void SonBuenas_ManoInexisteteLanzarError()
        {
            // Given
            var request = new Truco2v2Request(Guid.NewGuid());

            // When
            Action action = () => _controller.SonBuenas(request);

            // Then
            Assert.Throws<KeyNotFoundException>(action);
        }

        [Fact]
        public void SonBuenas_ManoFaseEnvidoMensajeErrorLazarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.FaseEnvido = "aceptar";

            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.SonBuenas(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void SonBuenas_EnvidoPendienteRespuestaMensajeErrorLanzarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.SonBuenas(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);

        }

        [Fact]
        public void SonBuenas_CaminoCorrectoLanzarOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J1";
            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.SonBuenas(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);
        }

        // TRUCO CANTAR ---------------------------------------------------------------------------------
        [Fact]
        public void CantarTruco_ManoInexistenteLanzarExceptio()
        {
            // Given
            var request = new Truco2v2Request(Guid.NewGuid());

            // When
            Action action = () => _controller.CantarTruco(request);

            // Then
            Assert.Throws<KeyNotFoundException>(action);
        }

        [Fact]
        public void CantarTruco_NosePuedeCantarMensajeErrorLanzarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TrucoCantado = true;

            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.CantarTruco(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void CantarTruco_CaminoCorrectoLazarOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TrucoCantado = false;

            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.CantarTruco(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);

        }

        // TRUCO ESCALAR ----------------------------------------------------------------------------------
        [Fact]
        public void EscalarTruco_ManoInvalidaLanzarException()
        {
            // Given
            var request = new Truco2v2Request(Guid.NewGuid());

            // When
            Action action = () => _controller.EscalarTruco(request);

            // Then
            Assert.Throws<KeyNotFoundException>(action);
        }

        [Fact]
        public void EscalarTruco_NoSePuedoEscalarNivelTrucoSuperiorMensajeErrorLanzarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.NivelTruco = 3;
            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.EscalarTruco(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void EscalarTruco_NoEsTurnoDelJugadorlMensajeErrorLanazarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TrucoCantado = true;
            mano.TrucoPendienteRespuestaDe = null;
            mano.NivelTruco = 1;
            mano.GanadorMano = null;
            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.EquipoCantorTruco = "EquipoB";
            mano.TurnoActual = "J2";

            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.EscalarTruco(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void EscalarTruco_CaminoExitosoLanzarOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TrucoCantado = true;
            mano.TrucoPendienteRespuestaDe = null;
            mano.NivelTruco = 1;
            mano.GanadorMano = null;
            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.EquipoCantorTruco = "EquipoB";
            mano.TurnoActual = "J1";

            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.EscalarTruco(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);

        }

        // TRUCO RESPONDER -------------------------------------------------------------------------------
        [Fact]
        public void ResponderTruco_CuandoAceptaTrucoRetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TrucoCantado = true;
            mano.TrucoPendienteRespuestaDe = "J1";

            var request = new Truco2v2ResponderTrucoRequest(
                mano.Id,
                true,
                null
            );

            //When
            var result = _controller.ResponderTruco(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.True(manoResultado.TrucoResuelto);
        }

        [Fact]
        public void ResponderTruco_CuandoNoAceptaRetornaOkYFinalizaLaMano()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TrucoCantado = true;
            mano.TrucoPendienteRespuestaDe = "J1";
            mano.NivelTruco = 1;
            mano.EquipoCantorTruco = "Equipo2";

            var request = new Truco2v2ResponderTrucoRequest(
                mano.Id,
                false,
                null
            );

            //When
            var result = _controller.ResponderTruco(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.True(manoResultado.ManoTerminada);
            Assert.True(manoResultado.TrucoResuelto);
            Assert.Equal("Equipo2", manoResultado.GanadorMano);
        }

        [Fact]
        public void ResponderTruco_CuandoNoHayTrucoCantadoLanzaException()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.TrucoCantado = false;

            var request = new Truco2v2ResponderTrucoRequest(
                mano.Id,
                true,
                null
            );

            //When
            Action action = () => _controller.ResponderTruco(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ResponderTruco_CuandoNoLeCorrespondeResponderLanzaException()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.TrucoCantado = true;
            mano.TrucoPendienteRespuestaDe = "J2";

            var request = new Truco2v2ResponderTrucoRequest(
                mano.Id,
                true,
                null
            );

            //When
            Action action = () => _controller.ResponderTruco(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ResponderTruco_CuandoAceptaYEscala_RetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.TrucoCantado = true;
            mano.TrucoPendienteRespuestaDe = "J1";
            mano.NivelTruco = 1;

            var request = new Truco2v2ResponderTrucoRequest(
                mano.Id,
                true,
                "retruco"
            );

            //When
            var result = _controller.ResponderTruco(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            Assert.NotNull(ok.Value);
        }

        // IRSE AL MAZO
        [Fact]
        public void IrseAlMazo_CuandoEsSuTurnoRetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.TurnoActual = "J1";

            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.IrseAlMazo(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.True(manoResultado.ManoTerminada);
            Assert.True(manoResultado.TrucoResuelto);
            Assert.NotNull(manoResultado.GanadorMano);
        }

        [Fact]
        public void IrseAlMazo_CuandoTienePendienteRespuestaDeTrucoRetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.TurnoActual = "J2";
            mano.TrucoPendienteRespuestaDe = "J1";

            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.IrseAlMazo(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.True(manoResultado.ManoTerminada);
        }

        [Fact]
        public void IrseAlMazo_CuandoNoEsSuTurnoNiDebeResponderLanzaException()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.TurnoActual = "J2";
            mano.TrucoPendienteRespuestaDe = "J3";

            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.IrseAlMazo(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void IrseAlMazo_CuandoLaManoYaTerminoLanzaException()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.ManoTerminada = true;

            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.IrseAlMazo(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void IrseAlMazo_CuandoYaHayGanadorLanzaException()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.GanadorMano = "EquipoA";

            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.IrseAlMazo(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        // NUEVA MANO -----------------------------------------------------------------------------------------
        [Fact]
        public void NuevaMano_CuandoElManoAnteriorNoTerminoLanzarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.GanadorMano = null;
            mano.PartidaTerminada = false;

            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.NuevaMano(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void NuevaMano_CuandoAPrtidaYaTerminoIniciarManoLanzarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.PartidaTerminada = true;

            var request = new Truco2v2Request(mano.Id);

            //When
            Action action = () => _controller.NuevaMano(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void NuevaMano_CuandoLaManoAnteriorTerminoRetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.GanadorMano = "EquipoA";
            mano.PartidaTerminada = false;
            mano.NumeroDeMano = 1;
            mano.PuntosEquipoA = 5;
            mano.PuntosEquipoB = 3;

            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.NuevaMano(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var nuevaMano = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.Equal(2, nuevaMano.NumeroDeMano);
            Assert.Equal(5, nuevaMano.PuntosEquipoA);
            Assert.Equal(3, nuevaMano.PuntosEquipoB);
        }

        // COMPAÑERO : CANTO TANTOS ---------------------------------------------------------------------------------
        [Fact]
        public void ResponderConsultaEnvido_CompañeroNoPideEnvidoLanzarExceptio()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.CompaConsultaEnvido = false;

            var request = new Truco2v2ConsultaEnvidoRequest(
                mano.Id,
                false
                );

            //When
            Action action = () => _controller.ResponderConsultaEnvido(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ResponderConsultaEnvido_CuandoRechazaRetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.CompaConsultaEnvido = true;

            var request = new Truco2v2ConsultaEnvidoRequest(
                mano.Id,
                false
            );

            //When
            var result = _controller.ResponderConsultaEnvido(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.False(manoResultado.CompaConsultaEnvido);
            Assert.True(manoResultado.CompaEnvidoConsultado);
        }

        [Fact]
        public void ResponderConsultaEnvido_CuandoAcepta_RetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.CompaConsultaEnvido = true;

            var request = new Truco2v2ConsultaEnvidoRequest(
                mano.Id,
                true
            );

            //When
            var result = _controller.ResponderConsultaEnvido(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.False(manoResultado.CompaConsultaEnvido);
            Assert.True(manoResultado.CompaEnvidoConsultado);
        }

        // COMPAÑERO : VOY
        [Fact]
        public void ResponderConsultaTruco_CompañeroConsultaTrucoMensajeErrorLanzarException()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.CompaConsultaTruco = false;

            var request = new Truco2v2ConsultaTrucoRequest(
                mano.Id,
                false
                );

            //When
            Action action = () => _controller.ResponderConsultaTruco(request);

            //Then
            Assert.Throws<InvalidOperationException>(action);
        }

        [Fact]
        public void ResponderConsultaTruco_CuandoVoyRetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.CompaConsultaTruco = true;

            var request = new Truco2v2ConsultaTrucoRequest(
                mano.Id,
                true
            );

            //When
            var result = _controller.ResponderConsultaTruco(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.False(manoResultado.CompaConsultaTruco);
            Assert.True(manoResultado.CompaTrucoConsultado);
        }

        [Fact]
        public void ResponderConsultaTruco_CuandoPongoRetornaOk()
        {
            //Given
            var mano = ObtenerManoNueva();

            mano.CompaConsultaTruco = true;

            var request = new Truco2v2ConsultaTrucoRequest(
                mano.Id,
                false
            );

            //When
            var result = _controller.ResponderConsultaTruco(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var manoResultado = Assert.IsType<ManoTruco2v2>(ok.Value);

            Assert.False(manoResultado.CompaConsultaTruco);
            Assert.True(manoResultado.CompaTrucoConsultado);
        }

        //AVANZAR MAQUINA ------------------------------------------------
        [Fact]
        public void AvanzarMaquina_ConRequestValidoDevuelveOkYActualizaMemoria()
        {
            //Given
            var mano = ObtenerManoNueva();
            var request = new Truco2v2Request(mano.Id);

            //When
            var resultado = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
            Assert.NotNull(response);
        }

        [Fact]
        public void AvanzarMaquina_CuandoPartidaTerminadaNoHaceNada()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.PartidaTerminada = true;
            var request = new Truco2v2Request(mano.Id);

            //When
            var response = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(response.Result);

            var resultado = Assert.IsType<Truco2v2PasoResponse>(ok.Value);

            Assert.Null(resultado.Evento);
        }

        [Fact]
        public void AvanzarMaquina_CuandoTurnoHumano_NoHaceNada()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TurnoActual = "J1";
            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
            Assert.Null(data.Evento);
        }

        //[Fact]
        //public void AvanzarMaquina_MaquinaRespondeTrucoQuiero()
        //{
        //    //Givem
        //    var mano = ObtenerManoNueva();
        //    mano.TurnoActual = "J2";
        //    mano.TrucoPendienteRespuestaDe = "J2";
        //    mano.TrucoCantado = true;
        //    mano.NivelTruco = 1;
        //    var maquina = mano.ObtenerJugador("J2");

        //    maquina.Mano =
        //    [
        //        new Carta { Numero = 1, Palo = "Espada" },
        //        new Carta { Numero = 7, Palo = "Oro" },
        //        new Carta { Numero = 3, Palo = "Espada" }
        //    ];

        //    var request = new Truco2v2Request(mano.Id);

        //    //When
        //    var result = _controller.AvanzarMaquina(request);

        //    //Then
        //    var ok = Assert.IsType<OkObjectResult>(result.Result);
        //    var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
        //    Assert.Equal("truco-resp", response.Evento.Tipo);
        //    Assert.Equal("¡Quiero!", response.Evento.Texto);
        //}

        //[Fact]
        //public void AvanzarMaquina_MaquinaRespondeTrucoNoQuiero()
        //{
        //    //Given
        //    var mano = ObtenerManoNueva();
        //    mano.TurnoActual = "J2";
        //    mano.TrucoPendienteRespuestaDe = "J2";
        //    mano.TrucoCantado = true;
        //    mano.NivelTruco = 1;
        //    var maquina = mano.ObtenerJugador("J2");

        //    maquina.Mano =
        //    [
        //        new Carta { Numero = 4, Palo = "Copa" },
        //        new Carta { Numero = 5, Palo = "Oro" },
        //        new Carta { Numero = 6, Palo = "Basto" }
        //    ];

        //    var request = new Truco2v2Request(mano.Id);

        //    //When
        //    var result = _controller.AvanzarMaquina(request);

        //    //Then
        //    var ok = Assert.IsType<OkObjectResult>(result.Result);
        //    var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
        //    Assert.Equal("¡No quiero!", response.Evento.Texto);
        //}

        //[Fact]
        //public void AvanzarMaquina_MaquinaRespondeTrucoEscalarReTruco()
        //{
        //    //Given
        //    var mano = ObtenerManoNueva();
        //    mano.TurnoActual = "J2";
        //    mano.TrucoPendienteRespuestaDe = "J2";
        //    mano.TrucoCantado = true;
        //    mano.NivelTruco = 1;
        //    var maquina = mano.ObtenerJugador("J2");

        //    maquina.Mano =
        //    [
        //        new Carta { Numero = 1, Palo = "Espada" },
        //        new Carta { Numero = 7, Palo = "Oro" },
        //        new Carta { Numero = 3, Palo = "Espada" }
        //    ];
        //    var request = new Truco2v2Request(mano.Id);

        //    //When
        //    var result = _controller.AvanzarMaquina(request);

        //    //Then
        //    var ok = Assert.IsType<OkObjectResult>(result.Result);
        //    var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
        //    Assert.Equal("¡Retruco!", response.Evento.Texto);
        //}

        [Fact]
        public void AvanzarMaquina_MaquinaRespondeRespondeEnvido()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TurnoActual = "J2";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.FaseEnvido = "pendiente_respuesta";

            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
            Assert.Equal("envido-resp", response.Evento.Tipo);
        }

        [Fact]
        public void AvanzarMaquina_MaquinaRespondeDeclararTantos()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TurnoActual = "J2";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.FaseEnvido = "declarando_tantos";
            mano.TantosDeclarados["J2"] = 28;
            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
            Assert.NotNull(response.Evento.Texto);
            Assert.NotEmpty(response.Evento.Texto);
        }

        [Fact]
        public void AvanzarMaquina_MaquinaRespondeDeclararTantosCantaSonBuenas()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TurnoActual = "J2";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.FaseEnvido = "declarando_tantos";
            mano.TantosDeclarados["J2"] = 28;
            mano.JugadorQueDijoSonBuenas = "J2";
            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
            Assert.Equal("¡Son buenas!", response.Evento.Texto);
        }

        [Fact]
        public void AvanzarMaquina_MaquinaRespondeConsultaDeenvidoJ3()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TurnoActual = "J3";
            mano.Vueltas.Clear();
            mano.CompaEnvidoConsultado = false;
            mano.EnvidoCantado = false;
            mano.EnvidoResuelto = false;
            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
            Assert.Equal("consulta-envido", response.Evento.Tipo);
        }

        //[Fact]
        //public void AvanzarMaquina_MaquinaRespondeConsultaDeTrucoJ3()
        //{
        //    // Given
        //    var mano = ObtenerManoNueva();
        //    mano.TurnoActual = "J3";
        //    mano.CompaEnvidoConsultado = true;
        //    mano.CompaTrucoConsultado = false;
        //    mano.TrucoCantado = false;
        //    mano.TrucoResuelto = false;
        //    var request = new Truco2v2Request(mano.Id);

        //    //When
        //    var result = _controller.AvanzarMaquina(request);

        //    //Then
        //    var ok = Assert.IsType<OkObjectResult>(result.Result);
        //    var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
        //    Assert.Equal("consulta-truco", response.Evento.Tipo);
        //}


        [Fact]
        public void AvanzarMaquina_MaquinaRespondeTurnoActualJuegaCarta()
        {
            // Given
            var mano = ObtenerManoNueva();
            mano.TurnoActual = "J3";
            mano.TrucoCantado = true;
            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
            Assert.Equal("carta",response.Evento.Tipo);
        }

        [Fact]
        public void AvanzarMaquina_MaquinaRespondeTurnoActualCantaEnvidoYJ4EsPie()
        {
            //Given
            var mano = ObtenerManoNueva();
            mano.TurnoActual = "J4";
            mano.EnvidoCantado = false;
            mano.EnvidoResuelto = false;
            mano.Vueltas.Clear();
            mano.TrucoCantado = false;
            mano.TrucoPendienteRespuestaDe = null;

            var maquina = mano.ObtenerJugador("J4");
            maquina.Mano = [
                new Carta{Numero = 6,Palo = "Oro"},
                new Carta{Numero = 7,Palo = "Oro"}
            ];

            var request = new Truco2v2Request(mano.Id);

            //When
            var result = _controller.AvanzarMaquina(request);

            //Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<Truco2v2PasoResponse>(ok.Value);
            Assert.Equal("envido", response.Evento.Tipo);
        }

        //ordenar mayor
        [Fact]
        public void OrdenarMayor_SiRequestEsValido_DevuelveOkConManoActualizada()
        {
            // Given 
            var manoId = Guid.NewGuid();
            var req = new Truco2v2OrdenMayorRequest(manoId, "J3");             
            var manoFake = new ManoTruco2v2
            {
                Id = manoId
            };

            var botCompañero = new Jugador { Id = "J3", EsMaquina = true, Mano = new List<Carta> { new Carta { ValorTruco = 10 } } };
            manoFake.Posicion3 = botCompañero;
            manoFake.EquipoA.Jugador2 = botCompañero; 
            Truco2v2MemoriaServicio.Actualizar(manoFake);

            // When 
            var resultado = _controller.OrdenarMayor(req);

            // Then 
            var actionResult = Assert.IsType<ActionResult<ManoTruco2v2>>(resultado);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var manoRetornada = Assert.IsType<ManoTruco2v2>(okResult.Value);

            Assert.Equal(manoId, manoRetornada.Id);
            Assert.Equal("J3", manoRetornada.OrdenJugarMayor); 
        }

        [Fact]
        public void OrdenarMayor_SiElJugadorNoEsBotAliado_PropagaInvalidOperationException()
        {
            // Given 
            var manoId = Guid.NewGuid();
            var req = new Truco2v2OrdenMayorRequest(manoId, "J2");
            var manoFake = new ManoTruco2v2 { Id = manoId };
            var botRival = new Jugador { Id = "J2", EsMaquina = true };
            manoFake.Posicion2 = botRival;
            manoFake.EquipoB.Jugador1 = botRival;

            Truco2v2MemoriaServicio.Actualizar(manoFake);

            // When
            Action act = () => _controller.OrdenarMayor(req);

            // Then 
            Assert.Throws<InvalidOperationException>(act);
        }

    }
}
