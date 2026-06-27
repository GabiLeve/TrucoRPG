using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    /// <summary>
    /// Modo historia: rivales disponibles, progreso del jugador y registro de victorias.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HistoriaController : ControllerBase
    {
        private readonly ObtenerRivalesHistoriaUseCase _obtenerRivales;
        private readonly ObtenerProgresoHistoriaUseCase _obtenerProgreso;
        private readonly PuedePelearConRivalUseCase _puedePelear;
        private readonly RegistrarVictoriaHistoriaUseCase _registrarVictoria;
        private readonly IUsuarioActualServicio _usuarioActual;

        public HistoriaController(
            ObtenerRivalesHistoriaUseCase obtenerRivales,
            ObtenerProgresoHistoriaUseCase obtenerProgreso,
            PuedePelearConRivalUseCase puedePelear,
            RegistrarVictoriaHistoriaUseCase registrarVictoria,
            IUsuarioActualServicio usuarioActual)
        {
            _obtenerRivales = obtenerRivales;
            _obtenerProgreso = obtenerProgreso;
            _puedePelear = puedePelear;
            _registrarVictoria = registrarVictoria;
            _usuarioActual = usuarioActual;
        }

        /// <summary>Devuelve la lista completa de rivales del modo historia, ordenados por nivel.</summary>
        /// <response code="200">Lista de rivales.</response>
        [HttpGet("rivales")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerRivales() =>
            Ok(await _obtenerRivales.EjecutarAsync());

        /// <summary>Devuelve un rival puntual por su nivel.</summary>
        /// <param name="nivel">Nivel del rival (1, 2, 3, …).</param>
        /// <response code="200">Datos del rival.</response>
        /// <response code="404">No existe un rival con ese nivel.</response>
        [HttpGet("rivales/{nivel:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObtenerRivalPorNivel(int nivel)
        {
            var rival = await _obtenerRivales.EjecutarPorNivelAsync(nivel);
            return rival is null ? NotFound() : Ok(rival);
        }

        /// <summary>Devuelve el progreso del jugador actual (último nivel derrotado y puntos).</summary>
        /// <response code="200">Progreso del jugador.</response>
        [HttpGet("progreso")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerProgreso() =>
            Ok(await _obtenerProgreso.EjecutarAsync(_usuarioActual.ObtenerId()));

        /// <summary>Indica si el jugador actual puede pelear contra el rival de ese nivel.</summary>
        /// <param name="nivel">Nivel del rival a evaluar.</param>
        /// <response code="200">Resultado de la validación (puede pelear y motivo).</response>
        [HttpGet("rivales/{nivel:int}/puede-pelear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PuedePelear(int nivel) =>
            Ok(await _puedePelear.EjecutarAsync(_usuarioActual.ObtenerId(), nivel));

        /// <summary>Registra una victoria del jugador y devuelve su progreso actualizado. Requiere JWT.</summary>
        /// <param name="request">Nivel del rival vencido y la diferencia de puntos.</param>
        /// <response code="200">Progreso actualizado.</response>
        /// <response code="401">No autenticado.</response>
        [Authorize(Roles = "Jugador")]
        [HttpPost("registrar-victoria")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegistrarVictoria(
            [FromBody] RegistrarVictoriaHistoriaRequest request)
        {
            await _registrarVictoria.EjecutarAsync(
                _usuarioActual.ObtenerId(),
                request.RivalNivel,
                request.DiferenciaPuntos);

            return Ok(await _obtenerProgreso.EjecutarAsync(_usuarioActual.ObtenerId()));
        }
    }
}
