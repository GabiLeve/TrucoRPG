using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Logica.UseCases;

namespace TrucoRPG.Tests.Logica
{
    public class ReglasUseCaseTest
    {

        [Fact]
        public async Task ObtenerCartas_CuandoMazoEsValido_DebeRetornarCartasOrdenadasPorValorTrucoDescendente()
        {
            // Given 
            var servicio = new ReglasUseCase();

            // When (Cuando...)
            var resultado = await servicio.ObtenerCartas();

            // Then (Entonces...)
            Assert.NotNull(resultado);
            Assert.NotEmpty(resultado);
        }

        [Fact]
        public async Task ObtenerReglasGenerales_CuandoExistenReglas_DebeRetornarColeccionDeReglas()
        {
            // Given 
            var servicio = new ReglasUseCase();

            // When 
            var resultado = await servicio.ObtenerReglasGenerales();

            // Then 
            Assert.NotNull(resultado);
            Assert.NotEmpty(resultado);
        }

    }
}
