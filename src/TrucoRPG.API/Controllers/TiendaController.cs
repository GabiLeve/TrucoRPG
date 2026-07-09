using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.DTO;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    /// <summary>Tienda del juego: catálogo de objetos disponibles, agrupados por categoría.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TiendaController : ControllerBase
    {
        private readonly ObtenerTiendaUseCase _obtenerTiendaUseCase;
        private readonly ComprarItemUseCase _comprarItemUseCase;
        private readonly ObtenerMonedasUseCase _obtenerMonedasUseCase;

        public TiendaController(ObtenerTiendaUseCase obtenerTiendaUseCase, ComprarItemUseCase comprarItemUseCase,ObtenerMonedasUseCase obtenerMonedasUseCase)
        {
            _obtenerTiendaUseCase = obtenerTiendaUseCase;
            _comprarItemUseCase = comprarItemUseCase;
            _obtenerMonedasUseCase = obtenerMonedasUseCase;
        }

        /// <summary>Devuelve el catálogo de la tienda agrupado por categoría.</summary>
        /// <response code="200">Lista de categorías con sus objetos.</response>
        [HttpGet]
        [Authorize] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerTienda()
        {
            try
            {
                var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var datosTienda = await _obtenerTiendaUseCase.EjecutarAsync();

                int monedas = 0;
                if (!string.IsNullOrEmpty(idUsuario))
                {
                    monedas = await _obtenerMonedasUseCase.Ejecutar(idUsuario);
                }

                return Ok(new
                {
                    Tienda = datosTienda,
                    MonedasUsuario = monedas
                });
            }
            catch
            {
                return BadRequest(new { mensaje = "Error al cargar la tienda." });
            }
        }

        [HttpPost("comprar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> Comprar([FromBody] ComprarItemDto dto)
        {
            try
            {
                var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(idUsuario))
                {
                    return Unauthorized(new { mensaje = "Usuario no autorizado." });
                }

                var item = dto.ToDomain();
                await _comprarItemUseCase.Ejecutar(idUsuario, item.Id);
                return Ok(new { mensaje = "Compra realizada con éxito." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

    }
}
