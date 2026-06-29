using Microsoft.AspNetCore.Mvc;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    /// <summary>Tienda del juego: catálogo de objetos disponibles, agrupados por categoría.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TiendaController : Controller
    {
        private readonly ObtenerTiendaUseCase _obtenerTiendaUseCase;

        public TiendaController(ObtenerTiendaUseCase obtenerTiendaUseCase)
        {
            _obtenerTiendaUseCase = obtenerTiendaUseCase;
        }

        /// <summary>Devuelve el catálogo de la tienda agrupado por categoría.</summary>
        /// <response code="200">Lista de categorías con sus objetos.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTienda()
        {
            var datosTienda = await _obtenerTiendaUseCase.EjecutarAsync();
            return Ok(datosTienda);
        }

    }
}
