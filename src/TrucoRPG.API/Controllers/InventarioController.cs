using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.DTO;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InventarioController : ControllerBase
    {
        private readonly ObtenerInventarioDelUsuarioUseCase _obtenerInventario;
        private readonly ObtenerMonedasUseCase _obtenerMonedasUseCase;

        public InventarioController(ObtenerInventarioDelUsuarioUseCase obtenerInventario, ObtenerMonedasUseCase obtenerMonedasUseCase   )
        {
            _obtenerInventario = obtenerInventario;
            _obtenerMonedasUseCase = obtenerMonedasUseCase;
        }

        [HttpGet("miInventario")]
        [Authorize]
        public async Task<IActionResult> TraerInventario()
        {
            try
            {
                var idUsuario = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(idUsuario))
                {
                    return Unauthorized(new { mensaje = "Token inválido o usuario no encontrado." });
                }

                var inventario = await _obtenerInventario.Ejecutar(idUsuario);
                var monedas = await _obtenerMonedasUseCase.Ejecutar(idUsuario);

                return Ok(new
                {
                    Monedas = monedas,
                    Items = inventario.Select(InventarioItemDto.FromDomain).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "No se pudo obtener el inventario del usuario." });
            }
        }



    }
}
