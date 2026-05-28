using Microsoft.AspNetCore.Mvc;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Logica.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReglasController : ControllerBase
    {
        private readonly ReglasUseCase _reglasUseCase;

        public ReglasController(ReglasUseCase reglasUseCase)
        {
            _reglasUseCase = reglasUseCase;
        }

        [HttpGet("cartas")]
        public async Task<ActionResult<IEnumerable<Carta>>> MostrarCartas()
        {
            try
            {
                var cartas = await _reglasUseCase.GetCartas();
                return Ok(cartas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

    }
}
