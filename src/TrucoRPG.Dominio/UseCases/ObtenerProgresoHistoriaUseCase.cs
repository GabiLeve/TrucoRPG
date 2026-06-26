using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerProgresoHistoriaUseCase
    {
        private readonly IProgresoPartidaRepositorio _progreso;

        public ObtenerProgresoHistoriaUseCase(IProgresoPartidaRepositorio progreso) =>
            _progreso = progreso;

        public async Task<ProgresoPartidaDto> EjecutarAsync(string? usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return new ProgresoPartidaDto(0, 0);

            var progreso = await _progreso.ObtenerPorUsuarioIdAsync(usuarioId);
            if (progreso is null)
                return new ProgresoPartidaDto(0, 0);

            return new ProgresoPartidaDto(
                progreso.UltimoRivalDerrotadoNivel,
                progreso.PuntosAcumulados);
        }
    }
}
