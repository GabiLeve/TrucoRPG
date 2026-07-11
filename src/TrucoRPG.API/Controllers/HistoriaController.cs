using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.DTO;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Infraestructura.Repositorios;

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
        private readonly ReiniciarRivalesHistoriaUseCase _reiniciarRivales;
        private readonly IUsuarioActualServicio _usuarioActual;
        private readonly CrearPersonajeUseCase _crearPersonaje;
        private readonly VerificarPersonajeUseCase _verificarPersonaje;
        private readonly ObtenerPersonajeDelUsuarioUseCase _obtenerPersonaje;
        private readonly EquiparAvatarUseCase _equiparAvatarUseCase;

        public HistoriaController(
            ObtenerRivalesHistoriaUseCase obtenerRivales,
            ObtenerProgresoHistoriaUseCase obtenerProgreso,
            PuedePelearConRivalUseCase puedePelear,
            RegistrarVictoriaHistoriaUseCase registrarVictoria,
            ReiniciarRivalesHistoriaUseCase reiniciarRivales,
            IUsuarioActualServicio usuarioActual,
            CrearPersonajeUseCase crearPersonaje,
            VerificarPersonajeUseCase verificarPersonaje,
            ObtenerPersonajeDelUsuarioUseCase obtenerPersonaje,
            EquiparAvatarUseCase equiparAvatarUseCase)
        {
            _obtenerRivales = obtenerRivales;
            _obtenerProgreso = obtenerProgreso;
            _puedePelear = puedePelear;
            _registrarVictoria = registrarVictoria;
            _reiniciarRivales = reiniciarRivales;
            _usuarioActual = usuarioActual;
            _crearPersonaje = crearPersonaje;
            _verificarPersonaje = verificarPersonaje;
            _obtenerPersonaje = obtenerPersonaje;
            _equiparAvatarUseCase = equiparAvatarUseCase;
        }

        [HttpPost("crearPersonaje")]
        [Authorize]
        public async Task<IActionResult> CrearPersonaje([FromBody] PersonajeDto personaje)
        {
                var usuarioId = _usuarioActual.ObtenerId();
                var nuevoPersonaje = personaje.ToDomain();
                await _crearPersonaje.Ejecutar(usuarioId, nuevoPersonaje.SpriteKey, nuevoPersonaje.HeroeId);
                return Ok(new { mensaje = "Personaje guardado correctamente!" });
 
        }

        [HttpGet("verificarPersonaje")]
        [Authorize]
        public async Task<IActionResult> VerificarPersonaje()
        {
            var usuarioId = _usuarioActual.ObtenerId();
            var tienePersonaje = await _verificarPersonaje.Ejecutar(usuarioId);
            return Ok(new { tienePersonaje });
        }


        [HttpGet("obtenerPersonaje")]
        [Authorize]
        public async Task<IActionResult> ObtenerPersonaje()
        {
                var usuarioId = _usuarioActual.ObtenerId();

                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized("Usuario no válido.");

                var personaje = await _obtenerPersonaje.Ejecutar(usuarioId);

                return Ok(personaje);
            
        }

        /// <summary>Devuelve la lista completa de rivales del modo historia, ordenados por nivel.</summary>
        /// <response code="200">Lista de rivales.</response>
        [HttpGet("rivales")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerRivales()
        {
            var rivales = await _obtenerRivales.EjecutarAsync();
            return Ok(rivales.Select(RivalDto.FromDomain).ToList());
        }

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
            return rival is null ? NotFound() : Ok(RivalDto.FromDomain(rival));
        }

        /// <summary>Devuelve el progreso del jugador actual (último nivel derrotado y puntos).</summary>
        /// <response code="200">Progreso del jugador.</response>
        [HttpGet("progreso")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ObtenerProgreso() =>
            Ok(ProgresoPartidaDto.FromDomain(await _obtenerProgreso.EjecutarAsync(_usuarioActual.ObtenerId())));

        /// <summary>Indica si el jugador actual puede pelear contra el rival de ese nivel.</summary>
        /// <param name="nivel">Nivel del rival a evaluar.</param>
        /// <response code="200">Resultado de la validación (puede pelear y motivo).</response>
        [HttpGet("rivales/{nivel:int}/puedePelear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> PuedePelear(int nivel)
        {
            var (puede, motivo) = await _puedePelear.EjecutarAsync(_usuarioActual.ObtenerId(), nivel);
            return Ok(PuedePelearRivalDto.FromResultado(nivel, puede, motivo));
        }

        /// <summary>Registra una victoria del jugador y devuelve su progreso actualizado. Requiere JWT.</summary>
        /// <param name="request">Nivel del rival vencido y la diferencia de puntos.</param>
        /// <response code="200">Progreso actualizado.</response>
        /// <response code="401">No autenticado.</response>
        [Authorize(Roles = "Jugador")]
        [HttpPost("registrarVictoria")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegistrarVictoria(
            [FromBody] RegistrarVictoriaHistoriaRequest request)
        {
            await _registrarVictoria.EjecutarAsync(
                _usuarioActual.ObtenerId(),
                request.RivalNivel,
                request.DiferenciaPuntos);

            return Ok(ProgresoPartidaDto.FromDomain(await _obtenerProgreso.EjecutarAsync(_usuarioActual.ObtenerId())));
        }

        /// <summary>
        /// Reinicia solo el estado de rivales derrotados para poder rejugar la historia.
        /// Los puntos acumulados y todo lo obtenido (monedas, habilidades, ropa) se conservan.
        /// </summary>
        /// <response code="200">Progreso actualizado (rivales en 0).</response>
        /// <response code="401">No autenticado.</response>
        [Authorize(Roles = "Jugador")]
        [HttpPost("reiniciarRivales")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReiniciarRivales()
        {
            await _reiniciarRivales.EjecutarAsync(_usuarioActual.ObtenerId());
            return Ok(ProgresoPartidaDto.FromDomain(await _obtenerProgreso.EjecutarAsync(_usuarioActual.ObtenerId())));
        }

        [HttpPut("equiparAvatar")]
        [Authorize]
        public async Task<IActionResult> EquiparAvatar([FromBody] EquiparSpriteDto dto)
        {
            try
            {
                var idUsuario = _usuarioActual.ObtenerId();

                var personajeNuevo = dto.ToDomain();
                await _equiparAvatarUseCase.Ejecutar(idUsuario, personajeNuevo.SpriteKey);

                return Ok(new { mensaje = "¡Ropa actualizada con éxito!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error inesperado en el servidor." });
            }
        }
    }
}
