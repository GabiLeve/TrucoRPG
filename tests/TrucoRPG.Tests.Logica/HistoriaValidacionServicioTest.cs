using Moq;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.Servicios;
using Xunit;

namespace TrucoRPG.Tests.Logica
{
    public class HistoriaValidacionServicioTest
    {
        private readonly Mock<IRivalRepositorio> _rivalesMock;
        private readonly Mock<IProgresoPartidaRepositorio> _progresoMock;
        private readonly HistoriaValidacionServicio _servicio;

        public HistoriaValidacionServicioTest()
        {
            _rivalesMock = new Mock<IRivalRepositorio>();
            _progresoMock = new Mock<IProgresoPartidaRepositorio>();

            // Instanciamos el servicio pasando los mocks para cuando los necesitemos
            _servicio = new HistoriaValidacionServicio(_rivalesMock.Object, _progresoMock.Object);
        }

        //puede pelear con rival
        [Fact]
        public void PuedePelearConRival_CuandoElRivalEsElSiguienteAlDerrotado_DebeRetornarTrue()
        {
            // Given 
            int ultimoRivalDerrotadoNivel = 1;
            int rivalNivelSolicitado = 2;

            // When 
            bool resultado = HistoriaValidacionServicio.PuedePelearConRival(ultimoRivalDerrotadoNivel, rivalNivelSolicitado);

            // Then 
            Assert.True(resultado);
        }

        [Fact]
        public void PuedePelearConRival_CuandoElRivalEsDeNivelMenorODeMismoNivel_DebeRetornarTrue()
        {
            // Given
            int ultimoRivalDerrotadoNivel = 3;
            int rivalNivelSolicitado = 2;

            // When 
            bool resultado = HistoriaValidacionServicio.PuedePelearConRival(ultimoRivalDerrotadoNivel, rivalNivelSolicitado);

            // Then 
            Assert.True(resultado);
        }

        [Fact]
        public void PuedePelearConRival_CuandoElRivalEsDeNivelMuyAlto_DebeRetornarFalse()
        {
            // Given
            int ultimoRivalDerrotadoNivel = 1;
            int rivalNivelSolicitado = 3;

            // When
            bool resultado = HistoriaValidacionServicio.PuedePelearConRival(ultimoRivalDerrotadoNivel, rivalNivelSolicitado);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void PuedePelearConRival_CuandoElRivalSolicitadoEsMenorAUno_DebeRetornarFalse()
        {
            // Given 
            int ultimoRivalDerrotadoNivel = 1;
            int rivalNivelSolicitado = 0;

            // When
            bool resultado = HistoriaValidacionServicio.PuedePelearConRival(ultimoRivalDerrotadoNivel, rivalNivelSolicitado);

            // Then 
            Assert.False(resultado);
        }

        //obtener rival o error
        [Fact]
        public async Task ObtenerRivalOErrorAsync_CuandoElRivalExiste_DebeRetornarElRival()
        {
            // Given
            int nivelBuscado = 2;
            var rivalEsperado = new Rival { Nivel = nivelBuscado };

            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(nivelBuscado))
                        .ReturnsAsync(rivalEsperado);

            // When
            var resultado = await _servicio.ObtenerRivalOErrorAsync(nivelBuscado);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal(nivelBuscado, resultado.Nivel);
        }

        [Fact]
        public async Task ObtenerRivalOErrorAsync_CuandoElRivalNoExiste_DebeLanzarKeyNotFoundException()
        {
            // Given
            int nivelInexistente = 99;

            // Simulamos que el repositorio devuelve null
            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(nivelInexistente))
                        .ReturnsAsync((Rival?)null);

            // When
            Func<Task> accion = async () => await _servicio.ObtenerRivalOErrorAsync(nivelInexistente);

            // Then
            var excepcion = await Assert.ThrowsAsync<KeyNotFoundException>(accion);
            Assert.Contains($"No existe un rival con nivel {nivelInexistente}", excepcion.Message);
        }

        //validar puede iniciar partida
        [Fact]
        public async Task ValidarPuedeIniciarPartidaAsync_CuandoRivalEsNivelUno_DebeRetornarExitosamente()
        {
            // Given
            string? usuarioId = null; 
            int rivalNivel = 1;

            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(rivalNivel))
                        .ReturnsAsync(new Rival());

            // When & Then 
            await _servicio.ValidarPuedeIniciarPartidaAsync(usuarioId, rivalNivel);
        }

        [Fact]
        public async Task ValidarPuedeIniciarPartidaAsync_CuandoNivelMayorAUnoYUsuarioEsNulo_DebeLanzarUnauthorizedAccessException()
        {
            // Given
            string? usuarioId = "";
            int rivalNivel = 2;

            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(rivalNivel))
                        .ReturnsAsync(new Rival());

            // When
            Func<Task> accion = async () => await _servicio.ValidarPuedeIniciarPartidaAsync(usuarioId, rivalNivel);

            // Then
            await Assert.ThrowsAsync<UnauthorizedAccessException>(accion);
        }

        [Fact]
        public async Task ValidarPuedeIniciarPartidaAsync_CuandoElRivalEstaBloqueado_DebeLanzarInvalidOperationException()
        {
            // Given
            string usuarioId = "user-123";
            int rivalNivel = 3;

            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(rivalNivel))
                        .ReturnsAsync(new Rival());

            var progresoSimulado = new ProgresoPartida { UltimoRivalDerrotadoNivel = 1 };
            _progresoMock.Setup(p => p.ObtenerPorUsuarioIdAsync(usuarioId))
                         .ReturnsAsync(progresoSimulado);

            // When
            Func<Task> accion = async () => await _servicio.ValidarPuedeIniciarPartidaAsync(usuarioId, rivalNivel);

            // Then
            var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(accion);
            Assert.Contains("Debés derrotar al rival nivel 2", excepcion.Message);
        }

        [Fact]
        public async Task ValidarPuedeIniciarPartidaAsync_CuandoElRivalEstaDesbloqueado_DebeProcederExitosamente()
        {
            // Given
            string usuarioId = "user-123";
            int rivalNivel = 2;

            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(rivalNivel))
                        .ReturnsAsync(new Rival());

            // Progreso ideal: ya ganó el nivel 1, puede jugar el 2
            var progresoSimulado = new ProgresoPartida { UltimoRivalDerrotadoNivel = 1 };
            _progresoMock.Setup(p => p.ObtenerPorUsuarioIdAsync(usuarioId))
                         .ReturnsAsync(progresoSimulado);

            // When & Then
            await _servicio.ValidarPuedeIniciarPartidaAsync(usuarioId, rivalNivel);
        }

        //puede pelear
        [Fact]
        public async Task EvaluarPuedePelearAsync_CuandoLaValidacionEsExitosa_DebeRetornarTrueYMotivoNulo()
        {
            // Given
            string usuarioId = "user-123";
            int rivalNivel = 1; 

            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(rivalNivel))
                        .ReturnsAsync(new Rival());

            // When
            var (puedePelear, motivo) = await _servicio.EvaluarPuedePelearAsync(usuarioId, rivalNivel);

            // Then
            Assert.True(puedePelear);
            Assert.Null(motivo);
        }

        [Fact]
        public async Task EvaluarPuedePelearAsync_CuandoRivalNoExiste_DebeRetornarFalseYMensajeDeError()
        {
            // Given
            string usuarioId = "user-123";
            int rivalNivel = 99;

            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(rivalNivel))
                        .ReturnsAsync((Rival?)null); 

            // When
            var (puedePelear, motivo) = await _servicio.EvaluarPuedePelearAsync(usuarioId, rivalNivel);

            // Then
            Assert.False(puedePelear);
            Assert.Contains("No existe un rival con nivel 99", motivo);
        }

        [Fact]
        public async Task EvaluarPuedePelearAsync_CuandoUsuarioEsNuloYNivelSuperiorAUno_DebeRetornarFalseYMensajeDeAutorizacion()
        {
            // Given
            string? usuarioId = null;
            int rivalNivel = 2;

            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(rivalNivel))
                        .ReturnsAsync(new Rival());

            // When
            var (puedePelear, motivo) = await _servicio.EvaluarPuedePelearAsync(usuarioId, rivalNivel);

            // Then
            Assert.False(puedePelear);
            Assert.Contains("Debés iniciar sesión", motivo);
        }

    }
}
