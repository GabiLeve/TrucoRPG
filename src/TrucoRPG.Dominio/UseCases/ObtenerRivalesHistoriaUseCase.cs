using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Mapeos;
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
                .ToDto();
        }

        public async Task<RivalDto?> EjecutarPorNivelAsync(int nivel)
        {
            var rival = await _rivales.ObtenerPorNivelAsync(nivel);
            return rival?.ToDto();
        }
    }
}
