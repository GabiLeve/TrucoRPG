using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerRivalesHistoriaUseCase
    {
        private readonly IRivalRepositorio _rivales;

        public ObtenerRivalesHistoriaUseCase(IRivalRepositorio rivales) => _rivales = rivales;

        public async Task<IReadOnlyList<RivalDto>> EjecutarAsync()
        {
            var rivales = await _rivales.ObtenerTodosAsync();
            return rivales
                .OrderBy(r => r.Nivel)
                .Select(r => new RivalDto(
                    r.Id,
                    r.Nivel,
                    r.Nombre,
                    r.Descripcion,
                    r.NombreHabilidad,
                    r.DescripcionHabilidad,
                    r.TipoRival,
                    r.TipoHabilidad))
                .ToList();
        }

        public async Task<RivalDto?> EjecutarPorNivelAsync(int nivel)
        {
            var rival = await _rivales.ObtenerPorNivelAsync(nivel);
            if (rival is null) return null;

            return new RivalDto(
                rival.Id,
                rival.Nivel,
                rival.Nombre,
                rival.Descripcion,
                rival.NombreHabilidad,
                rival.DescripcionHabilidad,
                rival.TipoRival,
                rival.TipoHabilidad);
        }
    }
}
