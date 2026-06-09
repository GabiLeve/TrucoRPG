using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Logica.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutorialController : ControllerBase
    {
        private readonly ReglasUseCase _reglasUseCase;

        public TutorialController(ReglasUseCase reglasUseCase)
        {
            _reglasUseCase = reglasUseCase;
        }

        [HttpGet("cartas")]
        public async Task<ActionResult<IEnumerable<Carta>>> MostrarCartas()
        {
                var cartas = await _reglasUseCase.GetCartas();
                return Ok(cartas);           
        }

        [HttpGet("generales")]
        public async Task<ActionResult<IEnumerable<CategoriaReglasDto>>> MostrarReglasGenerales()
        {
            var reglasInternas = await _reglasUseCase.GetReglasGenerales();

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
