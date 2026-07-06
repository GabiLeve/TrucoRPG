using Xunit;
using System;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class ConfirmarTravesuraUseCaseTest
    {
        [Fact]
        public void Ejecutar_CuandoLaManoNoExisteEnMemoria_DebeLanzarKeyNotFoundException()
        {
            // Given
            var _useCase = new ConfirmarTravesuraUseCase();
            Guid manoIdInexistente = Guid.NewGuid();

            // When
            Action accion = () => _useCase.Ejecutar(manoIdInexistente);

            // Then
            var excepcion = Assert.Throws<KeyNotFoundException>(accion);
            Assert.Equal("No se encontró la mano.", excepcion.Message);
        }

        [Fact]
        public void Ejecutar_CuandoTravesuraNoEstaBloqueando_DebeRetornarLaManoSinModificaciones()
        {
            // Given
            var _useCase = new ConfirmarTravesuraUseCase();
            Guid manoId = Guid.NewGuid();
            var manoSimulada = new ManoTruco
            {
                Id = manoId,
                TravesuraBloqueando = false,
                UltimoMensajeHabilidadRival = "Mensaje original"
            };

            PartidaMemoriaServicio.Actualizar(manoSimulada);

            // When
            var resultado = _useCase.Ejecutar(manoId);

            // Then
            Assert.NotNull(resultado);
            Assert.False(resultado.TravesuraBloqueando);
            Assert.Equal("Mensaje original", resultado.UltimoMensajeHabilidadRival); // No cambió
        }

        [Fact]
        public void Ejecutar_CuandoTravesuraEstaBloqueando_DebeQuitarElBloqueoYColocarMensajeDelPomberito()
        {
            // Given
            var _useCase = new ConfirmarTravesuraUseCase();
            Guid manoId = Guid.NewGuid();
            var manoSimulada = new ManoTruco
            {
                Id = manoId,
                TravesuraBloqueando = true,
                ManoIniciadaPor = IdJugador.Humano, // Evitamos que entre al bloque condicional de la máquina
                UltimoMensajeHabilidadRival = ""
            };

            PartidaMemoriaServicio.Actualizar(manoSimulada);

            // When
            var resultado = _useCase.Ejecutar(manoId);

            // Then
            Assert.NotNull(resultado);
            Assert.False(resultado.TravesuraBloqueando); // Ahora se desbloqueó
            Assert.Equal("¡Travesura! El Pomberito ocultó 2 de tus cartas. ¡Recordalas bien!", resultado.UltimoMensajeHabilidadRival);
        }
    }
}
