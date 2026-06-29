using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Logica.UseCases;

namespace TrucoRPG.API.Controllers
{
    /// <summary>
    /// Tutorial y reglas del truco: mazo de cartas con sus valores y reglas generales.
    /// Todos los endpoints requieren JWT (rol Jugador).
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Jugador")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TutorialController : ControllerBase
    {
        private readonly ReglasUseCase _reglasUseCase;

        public TutorialController(ReglasUseCase reglasUseCase)
        {
            _reglasUseCase = reglasUseCase;
        }

        /// <summary>Devuelve el mazo completo ordenado por valor de truco (de mayor a menor).</summary>
        /// <response code="200">Lista de cartas.</response>
        /// <response code="401">No autenticado.</response>
        [HttpGet("cartas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<Carta>>> ObtenerCartas()
        {
                var cartas = await _reglasUseCase.ObtenerCartas();
                return Ok(cartas);           
        }

        /// <summary>Devuelve las reglas generales del truco agrupadas por categoría.</summary>
        /// <response code="200">Lista de categorías con sus reglas.</response>
        /// <response code="401">No autenticado.</response>
        [HttpGet("generales")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<CategoriaReglasDto>>> ObtenerReglasGenerales()
        {
            var reglasInternas = await _reglasUseCase.ObtenerReglasGenerales();

                var reglasDto = reglasInternas.Select(c => new CategoriaReglasDto
                {
                    Categoria = c.Categoria,
                    Detalle = c.Detalle.Select(d => new ReglasDetalleDto
                    {
                        Titulo = d.Titulo,
                        Descripcion = d.Descripcion,
                        Puntos = d.Puntos
                    }).ToList()
                }).ToList();

                return Ok(reglasDto);
            
        }

    }
}
