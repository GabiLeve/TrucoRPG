using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class RegistrarVictoriaHistoriaUseCase
    {
        private readonly IProgresoPartidaRepositorio _progreso;
        private readonly HistoriaValidacionServicio _validacion;

        public RegistrarVictoriaHistoriaUseCase(
            IProgresoPartidaRepositorio progreso,
            HistoriaValidacionServicio validacion)
        {
            _progreso = progreso;
            _validacion = validacion;
        }

        public async Task EjecutarAsync(string? usuarioId, int rivalNivel, int diferenciaPuntos)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new UnauthorizedAccessException("Debés iniciar sesión para guardar el progreso.");

            _ = await _validacion.ObtenerRivalOErrorAsync(rivalNivel);

            await _progreso.RegistrarVictoriaAsync(usuarioId, rivalNivel, diferenciaPuntos);
        }
    }
}
