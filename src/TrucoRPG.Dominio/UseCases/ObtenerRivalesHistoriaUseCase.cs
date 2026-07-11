using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerRivalesHistoriaUseCase
    {
        private readonly IRivalRepositorio _rivales;

        public ObtenerRivalesHistoriaUseCase(IRivalRepositorio rivales) => _rivales = rivales;

        public async Task<IReadOnlyList<Rival>> EjecutarAsync()
        {
            var rivales = await _rivales.ObtenerTodosAsync();
            return rivales
                .OrderBy(r => r.Nivel)
                .ToList();
        }

        public async Task<Rival?> EjecutarPorNivelAsync(int nivel) =>
            await _rivales.ObtenerPorNivelAsync(nivel);
    }
}
