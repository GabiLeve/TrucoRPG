using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.Servicios
{
    public class HistoriaValidacionServicio
    {
        private readonly IRivalRepositorio _rivales;
        private readonly IProgresoPartidaRepositorio _progreso;

        public HistoriaValidacionServicio(
            IRivalRepositorio rivales,
            IProgresoPartidaRepositorio progreso)
        {
            _rivales = rivales;
            _progreso = progreso;
        }

        public static bool PuedePelearConRival(int ultimoRivalDerrotadoNivel, int rivalNivelSolicitado) =>
            rivalNivelSolicitado >= 1 && rivalNivelSolicitado <= ultimoRivalDerrotadoNivel + 1;

        public async Task<Rival> ObtenerRivalOErrorAsync(int rivalNivel)
        {
            var rival = await _rivales.ObtenerPorNivelAsync(rivalNivel);
            if (rival is null)
                throw new KeyNotFoundException($"No existe un rival con nivel {rivalNivel}.");
            return rival;
        }

        public async Task ValidarPuedeIniciarPartidaAsync(string? usuarioId, int rivalNivel)
        {
            _ = await ObtenerRivalOErrorAsync(rivalNivel);

            if (rivalNivel == 1)
                return;

            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new UnauthorizedAccessException(
                    "Debés iniciar sesión para pelear contra este rival.");

            var progreso = await _progreso.ObtenerPorUsuarioIdAsync(usuarioId);
            var ultimo = progreso?.UltimoRivalDerrotadoNivel ?? 0;

            if (!PuedePelearConRival(ultimo, rivalNivel))
                throw new InvalidOperationException(
                    $"Debés derrotar al rival nivel {rivalNivel - 1} antes de enfrentar al nivel {rivalNivel}.");
        }

        public async Task<(bool PuedePelear, string? Motivo)> EvaluarPuedePelearAsync(
            string? usuarioId,
            int rivalNivel)
        {
            try
            {
                await ValidarPuedeIniciarPartidaAsync(usuarioId, rivalNivel);
                return (true, null);
            }
            catch (KeyNotFoundException ex)
            {
                return (false, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return (false, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
