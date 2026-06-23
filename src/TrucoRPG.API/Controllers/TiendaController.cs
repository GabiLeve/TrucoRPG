using Microsoft.AspNetCore.Mvc;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiendaController : Controller
    {
        private readonly ObtenerTiendaUseCase _obtenerTiendaUseCase;

        public TiendaController(ObtenerTiendaUseCase obtenerTiendaUseCase)
        {
            _obtenerTiendaUseCase = obtenerTiendaUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetTienda()
        {
            var datosTienda = await _obtenerTiendaUseCase.EjecutarAsync();
            return Ok(datosTienda);
        }

    }
}
