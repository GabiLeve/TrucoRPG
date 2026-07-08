using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica
{
    public class ConfirmarSalpicaduraUseCaseTest
    {
        [Fact]
        public void Ejecutar_CuandoLaManoNoExisteEnMemoria_DebeLanzarKeyNotFoundException()
        {
            // Given 
            var _useCase = new ConfirmarSalpicaduraUseCase();
            Guid manoIdInexistente = Guid.NewGuid();

            // When
            Action accion = () => _useCase.Ejecutar(manoIdInexistente);

            // Then
            var excepcion = Assert.Throws<KeyNotFoundException>(accion);
            Assert.Equal("No se encontró la mano.", excepcion.Message);
        }

        [Fact]
        public void Ejecutar_CuandoSalpicaduraNoEstaBloqueando_DebeRetornarLaManoSinModificaciones()
        {
            // Given 
            var _useCase = new ConfirmarSalpicaduraUseCase();
            Guid manoId = Guid.NewGuid();
            var manoSimulada = new ManoTruco
            {
                Id = manoId,
                SalpicaduraBloqueando = false,
                UltimoMensajeHabilidadRival = "Mensaje original"
            };
            PartidaMemoriaServicio.Actualizar(manoSimulada); 

            // When
            var resultado = _useCase.Ejecutar(manoId);

            // Then
            Assert.NotNull(resultado);
            Assert.False(resultado.SalpicaduraBloqueando);
            Assert.Equal("Mensaje original", resultado.UltimoMensajeHabilidadRival); 
        }

        [Fact]
        public void Ejecutar_CuandoSalpicaduraEstaBloqueando_DebeQuitarElBloqueoYColocarMensajeDeNahuelito()
        {
            // Given 
            var _useCase = new ConfirmarSalpicaduraUseCase();
            Guid manoId = Guid.NewGuid();
            var manoSimulada = new ManoTruco
            {
                Id = manoId,
                SalpicaduraBloqueando = true,
                ManoIniciadaPor = IdJugador.Humano, 
                UltimoMensajeHabilidadRival = ""
            };

            PartidaMemoriaServicio.Actualizar(manoSimulada);

            // When
            var resultado = _useCase.Ejecutar(manoId);

            // Then
            Assert.NotNull(resultado);
            Assert.False(resultado.SalpicaduraBloqueando); 
            Assert.Equal("¡Salpicadura! Nahuelito cambió el palo de 2 de tus cartas.", resultado.UltimoMensajeHabilidadRival);
        }
    }
}
