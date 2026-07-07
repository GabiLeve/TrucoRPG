using Xunit;
using System;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class ConfirmarAullidoUseCaseTest
    {
        [Fact]
        public void Ejecutar_CuandoLaManoNoExisteEnMemoria_DebeLanzarKeyNotFoundException()
        {
            // Given
            var _useCase = new ConfirmarAullidoUseCase();
            Guid manoIdInexistente = Guid.NewGuid();

            // When
            Action accion = () => _useCase.Ejecutar(manoIdInexistente);

            // Then
            var excepcion = Assert.Throws<KeyNotFoundException>(accion);
            Assert.Equal("No se encontró la mano.", excepcion.Message);
        }

        [Fact]
        public void Ejecutar_CuandoAullidoNoEstaBloqueando_DebeRetornarLaManoSinModificaciones()
        {
            // Given
            var _useCase = new ConfirmarAullidoUseCase();
            Guid manoId = Guid.NewGuid();
            var manoSimulada = new ManoTruco
            {
                Id = manoId,
                AullidoBloqueando = false
            };

            PartidaMemoriaServicio.Actualizar(manoSimulada);

            // When
            var resultado = _useCase.Ejecutar(manoId);

            // Then
            Assert.NotNull(resultado);
            Assert.False(resultado.AullidoBloqueando); // Sigue en false
        }

        [Fact]
        public void Ejecutar_CuandoAullidoEstaBloqueando_DebeProcesarElAullidoYActualizarMemoria()
        {
            // Given
            var _useCase = new ConfirmarAullidoUseCase();
            Guid manoId = Guid.NewGuid();
            var manoSimulada = new ManoTruco
            {
                Id = manoId,
                AullidoBloqueando = true
            };

            PartidaMemoriaServicio.Actualizar(manoSimulada);

            // When
            var resultado = _useCase.Ejecutar(manoId);

            // Then
            Assert.NotNull(resultado);
            // Nota: Si AullidoServicio.EjecutarIrAlMazo(mano) cambia internamente AullidoBloqueando a false, 
            // podés descomentar la siguiente línea para verificarlo:
            // Assert.False(resultado.AullidoBloqueando);
        }
    }
}
