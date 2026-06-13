using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Controllers;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Logica.UseCases;

namespace TrucoRPG.Tests.API
{
    public class TutorialControllerTest
    {
        [Fact]
        public async Task MostrarCartas_DevuelveOkConCartas()
        {
            // Given
            var useCase = new ReglasUseCase();
            var controller = new TutorialController(useCase);

            // When
            var result = await controller.MostrarCartas();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var cartas = Assert.IsAssignableFrom<IEnumerable<Carta>>(ok.Value);

            Assert.NotEmpty(cartas);
        }

        [Fact]
        public async Task MostrarReglasGenerales_DevuelveOkConReglas()
        {
            // Given
            var useCase = new ReglasUseCase();
            var controller = new TutorialController(useCase);

            // When
            var result = await controller.MostrarReglasGenerales();

            // Then
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            var reglas = Assert.IsAssignableFrom<IEnumerable<CategoriaReglasDto>>(ok.Value);

            Assert.NotEmpty(reglas);
        }
    }


}
