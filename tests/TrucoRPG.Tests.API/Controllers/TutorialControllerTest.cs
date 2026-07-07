using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Controllers;
using TrucoRPG.API.Models;
using TrucoRPG.Logica.UseCases;
using Xunit;

namespace TrucoRPG.Tests.API
{
    /// <summary>ReglasUseCase no tiene dependencias, se usa la instancia real.</summary>
    public class TutorialControllerTest
    {
        private readonly TutorialController _controller = new(new ReglasUseCase());

        [Fact]
        public async Task ObtenerCartas_RetornaOkConMazoOrdenadoPorValorTruco()
        {
            var resultado = await _controller.ObtenerCartas();

            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task ObtenerReglasGenerales_RetornaOkConCategorias()
        {
            var resultado = await _controller.ObtenerReglasGenerales();

            var ok = Assert.IsType<OkObjectResult>(resultado.Result);
            var categorias = Assert.IsAssignableFrom<List<CategoriaReglasDto>>(ok.Value);
            Assert.NotEmpty(categorias);
        }
    }
}
