using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.Dominio.DTOs;

namespace TrucoRPG.Dominio.DTOs
{
<<<<<<< Updated upstream:src/TrucoRPG.Dominio/DTOs/ComprarItemDto.cs
    public class ComprarItemDto
    {
        public int ItemTiendaId { get; set; }
=======
    public class HistoriaControllerTest
    {
        private readonly Mock<IUsuarioActual> _usuarioActualMock;
        private readonly Mock<ICrearPersonaje> _crearPersonajeMock;
        private readonly PersonajeController _controller;

        public PersonajeControllerTests()
        {
            _usuarioActualMock = new Mock<IUsuarioActual>();
            _crearPersonajeMock = new Mock<ICrearPersonaje>();

            // Inyectamos los mocks en el controlador
            _controller = new PersonajeController(_usuarioActualMock.Object, _crearPersonajeMock.Object);
        }

        [Fact]
        public async Task CrearPersonaje_CuandoDatosSonValidos_DebeRetornarOk()
        {
            // GIVEN (Arrange)
            var usuarioIdEsperado = Guid.NewGuid();
            var personajeDto = new PersonajeDto
            {
                HeroeId = Guid.NewGuid(),
                SpriteKey = "mago_fuego"
            };

            _usuarioActualMock
                .Setup(x => x.ObtenerId())
                .Returns(usuarioIdEsperado);

            _crearPersonajeMock
                .Setup(x => x.Ejecutar(usuarioIdEsperado, personajeDto.SpriteKey, personajeDto.HeroeId))
                .Returns(Task.CompletedTask);

            // WHEN (Act)
            var resultado = await _controller.CrearPersonaje(personajeDto);

            // THEN (Assert)
            var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;

            // Verificamos que el mensaje sea el correcto de manera anónima
            okResult.Value.Should().BeEquivalentTo(new { mensaje = "Personaje guardado correctamente!" });

            // Verificamos que el caso de uso realmente se ejecutó con los parámetros correctos
            _crearPersonajeMock.Verify(x => x.Ejecutar(usuarioIdEsperado, personajeDto.SpriteKey, personajeDto.HeroeId), Times.Once);
        }

        [Fact]
        public async Task CrearPersonaje_CuandoElCasoDeUsoFalla_DebeLanzarExcepcionParaElMiddleware()
        {
            // GIVEN (Arrange)
            // Probamos que si el caso de uso lanza InvalidOperationException, el controlador NO la atrapa
            // (permitiendo que viaje hasta tu Middleware de excepciones)
            var usuarioIdEsperado = Guid.NewGuid();
            var personajeDto = new PersonajeDto
            {
                HeroeId = Guid.NewGuid(),
                SpriteKey = "orco_guerrero"
            };

            _usuarioActualMock
                .Setup(x => x.ObtenerId())
                .Returns(usuarioIdEsperado);

            _crearPersonajeMock
                .Setup(x => x.Ejecutar(usuarioIdEsperado, personajeDto.SpriteKey, personajeDto.HeroeId))
                .ThrowsAsync(new InvalidOperationException("El héroe seleccionado no existe."));

            // WHEN (Act) & THEN (Assert)
            // Al haber sacado el try-catch, el controlador debe dejar fluir la excepción
            Func<Task> accion = async () => await _controller.CrearPersonaje(personajeDto);

            await accion.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("El héroe seleccionado no existe.");
        }
>>>>>>> Stashed changes:tests/TrucoRPG.Tests.API/HistoriaControllerTest.cs
    }
}
