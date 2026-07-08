using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TrucoRPG.API.Controllers;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;
using Xunit;

namespace TrucoRPG.Tests.API
{
    /// <summary>
    /// Los use cases son clases concretas, así que se instancian reales
    /// con los repositorios mockeados (mock en el borde de infraestructura).
    /// </summary>
    public class HistoriaControllerTest
    {
        private readonly Mock<IRivalRepositorio> _rivalesMock = new();
        private readonly Mock<IProgresoPartidaRepositorio> _progresoMock = new();
        private readonly Mock<IUsuarioRepositorio> _usuariosMock = new();
        private readonly Mock<IInventarioRepositorio> _inventarioMock = new();
        private readonly Mock<IUsuarioActualServicio> _usuarioActualMock = new();
        private readonly HistoriaController _controller;

        private const string UserId = "user-123";

        public HistoriaControllerTest()
        {
            var validacion = new HistoriaValidacionServicio(_rivalesMock.Object, _progresoMock.Object);

            _controller = new HistoriaController(
                new ObtenerRivalesHistoriaUseCase(_rivalesMock.Object),
                new ObtenerProgresoHistoriaUseCase(_progresoMock.Object),
                new PuedePelearConRivalUseCase(validacion),
                new RegistrarVictoriaHistoriaUseCase(_progresoMock.Object, validacion),
                new ReiniciarRivalesHistoriaUseCase(_progresoMock.Object),
                _usuarioActualMock.Object,
                new CrearPersonajeUseCase(_usuariosMock.Object),
                new VerificarPersonajeUseCase(_usuariosMock.Object),
                new ObtenerPersonajeDelUsuarioUseCase(_usuariosMock.Object),
                new EquiparAvatarUseCase(_inventarioMock.Object, _usuariosMock.Object));

            _usuarioActualMock.Setup(x => x.ObtenerId()).Returns(UserId);
        }

        private static Rival RivalDeNivel(int nivel) =>
            new() { Nivel = nivel, Nombre = $"Rival {nivel}" };

        // ── Personaje ─────────────────────────────────────────────

        [Fact]
        public async Task CrearPersonaje_ConDatosValidos_RetornaOkYPersiste()
        {
            var dto = new PersonajeDto { HeroeId = Guid.NewGuid(), SpriteKey = "gaucho" };

            var resultado = await _controller.CrearPersonaje(dto);

            Assert.IsType<OkObjectResult>(resultado);
            _usuariosMock.Verify(r => r.CrearPersonaje(UserId, "gaucho", dto.HeroeId), Times.Once);
        }

        [Fact]
        public async Task VerificarPersonaje_CuandoExiste_RetornaOk()
        {
            _usuariosMock.Setup(r => r.PersonajeExistente(UserId)).ReturnsAsync(true);

            var resultado = await _controller.VerificarPersonaje();

            var ok = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        }

        [Fact]
        public async Task ObtenerPersonaje_ConUsuarioValido_RetornaOkConPersonaje()
        {
            var personaje = new Personaje { HeroeId = Guid.NewGuid(), SpriteKey = "gaucho" };
            _usuariosMock.Setup(r => r.ObtenerPersonajeDelUsuario(UserId)).ReturnsAsync(personaje);

            var resultado = await _controller.ObtenerPersonaje();

            var ok = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(personaje, ok.Value);
        }

        [Fact]
        public async Task ObtenerPersonaje_SinUsuario_RetornaUnauthorized()
        {
            _usuarioActualMock.Setup(x => x.ObtenerId()).Returns((string?)null);

            var resultado = await _controller.ObtenerPersonaje();

            Assert.IsType<UnauthorizedObjectResult>(resultado);
        }

        // ── Rivales ───────────────────────────────────────────────

        [Fact]
        public async Task ObtenerRivales_RetornaOkConListaOrdenadaPorNivel()
        {
            _rivalesMock.Setup(r => r.ObtenerTodosAsync())
                .ReturnsAsync(new List<Rival> { RivalDeNivel(2), RivalDeNivel(1) });

            var resultado = await _controller.ObtenerRivales();

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var lista = Assert.IsAssignableFrom<IReadOnlyList<RivalDto>>(ok.Value);
            Assert.Equal(2, lista.Count);
            Assert.Equal(1, lista[0].Nivel);
        }

        [Fact]
        public async Task ObtenerRivalPorNivel_CuandoExiste_RetornaOk()
        {
            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(3)).ReturnsAsync(RivalDeNivel(3));

            var resultado = await _controller.ObtenerRivalPorNivel(3);

            Assert.IsType<OkObjectResult>(resultado);
        }

        [Fact]
        public async Task ObtenerRivalPorNivel_CuandoNoExiste_RetornaNotFound()
        {
            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(99)).ReturnsAsync((Rival?)null);

            var resultado = await _controller.ObtenerRivalPorNivel(99);

            Assert.IsType<NotFoundResult>(resultado);
        }

        // ── Progreso ──────────────────────────────────────────────

        [Fact]
        public async Task ObtenerProgreso_ConProgresoExistente_RetornaOk()
        {
            _progresoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(UserId))
                .ReturnsAsync(new ProgresoPartida { UsuarioId = UserId, UltimoRivalDerrotadoNivel = 2, PuntosAcumulados = 10 });

            var resultado = await _controller.ObtenerProgreso();

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var dto = Assert.IsType<ProgresoPartidaDto>(ok.Value);
            Assert.Equal(2, dto.UltimoRivalDerrotadoNivel);
        }

        [Fact]
        public async Task ObtenerProgreso_SinUsuario_RetornaProgresoVacio()
        {
            _usuarioActualMock.Setup(x => x.ObtenerId()).Returns((string?)null);

            var resultado = await _controller.ObtenerProgreso();

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var dto = Assert.IsType<ProgresoPartidaDto>(ok.Value);
            Assert.Equal(0, dto.UltimoRivalDerrotadoNivel);
        }

        // ── Puede pelear ──────────────────────────────────────────

        [Fact]
        public async Task PuedePelear_ContraNivel1_SiemprePuede()
        {
            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(1)).ReturnsAsync(RivalDeNivel(1));

            var resultado = await _controller.PuedePelear(1);

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var dto = Assert.IsType<PuedePelearRivalDto>(ok.Value);
            Assert.True(dto.PuedePelear);
        }

        [Fact]
        public async Task PuedePelear_SinDerrotarAlAnterior_NoPuede()
        {
            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(3)).ReturnsAsync(RivalDeNivel(3));
            _progresoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(UserId))
                .ReturnsAsync(new ProgresoPartida { UsuarioId = UserId, UltimoRivalDerrotadoNivel = 1 });

            var resultado = await _controller.PuedePelear(3);

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var dto = Assert.IsType<PuedePelearRivalDto>(ok.Value);
            Assert.False(dto.PuedePelear);
            Assert.NotNull(dto.Motivo);
        }

        [Fact]
        public async Task PuedePelear_ContraRivalInexistente_NoPuede()
        {
            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(99)).ReturnsAsync((Rival?)null);

            var resultado = await _controller.PuedePelear(99);

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var dto = Assert.IsType<PuedePelearRivalDto>(ok.Value);
            Assert.False(dto.PuedePelear);
        }

        // ── Registrar victoria ────────────────────────────────────

        [Fact]
        public async Task RegistrarVictoria_ConDatosValidos_PersisteYRetornaProgreso()
        {
            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(2)).ReturnsAsync(RivalDeNivel(2));
            _progresoMock.Setup(r => r.ObtenerPorUsuarioIdAsync(UserId))
                .ReturnsAsync(new ProgresoPartida { UsuarioId = UserId, UltimoRivalDerrotadoNivel = 2 });

            var request = new RegistrarVictoriaHistoriaRequest { RivalNivel = 2, DiferenciaPuntos = 15 };

            var resultado = await _controller.RegistrarVictoria(request);

            Assert.IsType<OkObjectResult>(resultado);
            _progresoMock.Verify(r => r.RegistrarVictoriaAsync(UserId, 2, 15), Times.Once);
        }

        [Fact]
        public async Task RegistrarVictoria_SinUsuario_LanzaUnauthorized()
        {
            _usuarioActualMock.Setup(x => x.ObtenerId()).Returns((string?)null);
            var request = new RegistrarVictoriaHistoriaRequest { RivalNivel = 1, DiferenciaPuntos = 5 };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _controller.RegistrarVictoria(request));
        }

        [Fact]
        public async Task RegistrarVictoria_ContraRivalInexistente_LanzaKeyNotFound()
        {
            _rivalesMock.Setup(r => r.ObtenerPorNivelAsync(99)).ReturnsAsync((Rival?)null);
            var request = new RegistrarVictoriaHistoriaRequest { RivalNivel = 99, DiferenciaPuntos = 5 };

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.RegistrarVictoria(request));
        }

        // ── Reiniciar rivales (rejugar historia) ──────────────────

        [Fact]
        public async Task ReiniciarRivales_ConUsuario_ReiniciaYRetornaProgreso()
        {
            var resultado = await _controller.ReiniciarRivales();

            Assert.IsType<OkObjectResult>(resultado);
            _progresoMock.Verify(r => r.ReiniciarRivalesAsync(UserId), Times.Once);
        }

        [Fact]
        public async Task ReiniciarRivales_SinUsuario_LanzaUnauthorized()
        {
            _usuarioActualMock.Setup(x => x.ObtenerId()).Returns((string?)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _controller.ReiniciarRivales());
        }
    }
}
