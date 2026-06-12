using Microsoft.AspNetCore.Mvc;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoriaController : ControllerBase
    {
        private readonly ObtenerRivalesHistoriaUseCase _obtenerRivales;
        private readonly ObtenerProgresoHistoriaUseCase _obtenerProgreso;
        private readonly PuedePelearConRivalUseCase _puedePelear;
        private readonly IUsuarioActualServicio _usuarioActual;

        public HistoriaController(
            ObtenerRivalesHistoriaUseCase obtenerRivales,
            ObtenerProgresoHistoriaUseCase obtenerProgreso,
            PuedePelearConRivalUseCase puedePelear,
            IUsuarioActualServicio usuarioActual)
        {
            _obtenerRivales = obtenerRivales;
            _obtenerProgreso = obtenerProgreso;
            _puedePelear = puedePelear;
            _usuarioActual = usuarioActual;
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
    }
}
