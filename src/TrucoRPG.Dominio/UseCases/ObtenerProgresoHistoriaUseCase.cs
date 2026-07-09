using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerProgresoHistoriaUseCase
    {
        private readonly IProgresoPartidaRepositorio _progreso;

        public ObtenerProgresoHistoriaUseCase(IProgresoPartidaRepositorio progreso) =>
            _progreso = progreso;

        public async Task<ProgresoPartida> EjecutarAsync(string? usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                return new ProgresoPartida();

            var progreso = await _progreso.ObtenerPorUsuarioIdAsync(usuarioId);
            return progreso ?? new ProgresoPartida();
        }
    }
}
