using Xunit;
using System;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class ConfirmarRasgunoUseCaseTest
    {
        [Fact]
        public void Ejecutar_CuandoLaManoNoExisteEnMemoria_DebeLanzarKeyNotFoundException()
        {
            // Given
            var _useCase = new ConfirmarRasgunoUseCase();
            Guid manoIdInexistente = Guid.NewGuid();

            // When
            Action accion = () => _useCase.Ejecutar(manoIdInexistente);

            // Then
            var excepcion = Assert.Throws<KeyNotFoundException>(accion);
            Assert.Equal("No se encontró la mano.", excepcion.Message);
        }

        [Fact]
        public void Ejecutar_CuandoRasgunoNoEstaBloqueando_DebeRetornarLaManoSinModificaciones()
        {
            // Given
            var _useCase = new ConfirmarRasgunoUseCase();
            Guid manoId = Guid.NewGuid();
            var manoSimulada = new ManoTruco
            {
                Id = manoId,
                RasgunoBloqueando = false,
                UltimoMensajeHabilidadRival = "Mensaje previo"
            };

            PartidaMemoriaServicio.Actualizar(manoSimulada);

            // When
            var resultado = _useCase.Ejecutar(manoId);

            // Then
            Assert.NotNull(resultado);
            Assert.False(resultado.RasgunoBloqueando);
            Assert.Equal("Mensaje previo", resultado.UltimoMensajeHabilidadRival); // No se modificó
        }

        [Fact]
        public void Ejecutar_CuandoRasgunoEstaBloqueando_DebeQuitarElBloqueoYColocarMensajeDelLobizon()
        {
            // Given
            var _useCase = new ConfirmarRasgunoUseCase();
            Guid manoId = Guid.NewGuid();
            var manoSimulada = new ManoTruco
            {
                Id = manoId,
                RasgunoBloqueando = true,
                ManoIniciadaPor = IdJugador.Humano, 
                UltimoMensajeHabilidadRival = ""
            };

            PartidaMemoriaServicio.Actualizar(manoSimulada);

            // When
            var resultado = _useCase.Ejecutar(manoId);

            // Then
            Assert.NotNull(resultado);
            Assert.False(resultado.RasgunoBloqueando); // Ahora se desbloqueó
            Assert.Equal("¡Rasguño! El Lobizón debilitó 1 de tus cartas.", resultado.UltimoMensajeHabilidadRival);
        }
    }
}
