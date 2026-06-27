using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Mapeos;
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
                return ProgresoPartidaMapper.VacioDto();

            var progreso = await _progreso.ObtenerPorUsuarioIdAsync(usuarioId);
            return progreso?.ToDto() ?? ProgresoPartidaMapper.VacioDto();
        }
    }
}
