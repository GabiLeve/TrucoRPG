using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Infraestructura.Repositorios;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoriaController : ControllerBase
    {
        private readonly ObtenerRivalesHistoriaUseCase _obtenerRivales;
        private readonly ObtenerProgresoHistoriaUseCase _obtenerProgreso;
        private readonly PuedePelearConRivalUseCase _puedePelear;
        private readonly RegistrarVictoriaHistoriaUseCase _registrarVictoria;
        private readonly IUsuarioActualServicio _usuarioActual;
        private readonly CrearPersonajeUseCase _crearPersonaje;
        private readonly VerificarPersonajeUseCase _verificarPersonaje;
        private readonly ObtenerPersonajeDelUsuarioUseCase _obtenerPersonaje;

        public HistoriaController(
            ObtenerRivalesHistoriaUseCase obtenerRivales,
            ObtenerProgresoHistoriaUseCase obtenerProgreso,
            PuedePelearConRivalUseCase puedePelear,
            RegistrarVictoriaHistoriaUseCase registrarVictoria,
            IUsuarioActualServicio usuarioActual,
            CrearPersonajeUseCase crearPersonaje,
            VerificarPersonajeUseCase verificarPersonaje,
            ObtenerPersonajeDelUsuarioUseCase obtenerPersonaje)
        {
            _obtenerRivales = obtenerRivales;
            _obtenerProgreso = obtenerProgreso;
            _puedePelear = puedePelear;
            _registrarVictoria = registrarVictoria;
            _usuarioActual = usuarioActual;
            _crearPersonaje = crearPersonaje;
            _verificarPersonaje = verificarPersonaje;
            _obtenerPersonaje = obtenerPersonaje;
        }

        [HttpPost("crearPersonaje")]
        public async Task<IActionResult> CrearPersonaje([FromBody] PersonajeDto personaje)
        {
            try
            {
                var usuarioId = _usuarioActual.ObtenerId();
                await _crearPersonaje.Ejecutar(usuarioId, personaje.SpriteKey, personaje.HeroeId);
                return Ok(new { mensaje = "Personaje guardado correctamente!" });
            }
            catch (InvalidOperationException ex){
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("verificarPersonaje")]
        public async Task<IActionResult> VerificarPersonaje()
        {
            var usuarioId = _usuarioActual.ObtenerId();
            var tienePersonaje = await _verificarPersonaje.Ejecutar(usuarioId);
            return Ok(new { tienePersonaje });
        }


        [HttpGet("obtenerPersonaje")]
        public async Task<IActionResult> ObtenerPersonaje()
        {
            try
            {
                var usuarioId = _usuarioActual.ObtenerId();

                if (string.IsNullOrEmpty(usuarioId))
                    return Unauthorized("Usuario no válido.");

                var personaje = await _obtenerPersonaje.Ejecutar(usuarioId);

                return Ok(personaje);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        [HttpGet("rivales")]
        public async Task<IActionResult> ObtenerRivales() =>
            Ok(await _obtenerRivales.EjecutarAsync());

        [HttpGet("rivales/{nivel:int}")]
        public async Task<IActionResult> ObtenerRivalPorNivel(int nivel)
        {
            var rival = await _obtenerRivales.EjecutarPorNivelAsync(nivel);
            return rival is null ? NotFound() : Ok(rival);
        }

        [HttpGet("progreso")]
        public async Task<IActionResult> ObtenerProgreso() =>
            Ok(await _obtenerProgreso.EjecutarAsync(_usuarioActual.ObtenerId()));

        [HttpGet("rivales/{nivel:int}/puede-pelear")]
        public async Task<IActionResult> PuedePelear(int nivel) =>
            Ok(await _puedePelear.EjecutarAsync(_usuarioActual.ObtenerId(), nivel));

        [Authorize(Roles = "Jugador")]
        [HttpPost("registrar-victoria")]
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
