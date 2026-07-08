using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Repositorios
{
    public interface IRivalRepositorio
    {
        Task<IReadOnlyList<Rival>> ObtenerTodosAsync();
        Task<Rival?> ObtenerPorNivelAsync(int nivel);
        Task<Rival?> ObtenerPorTipoAsync(ClaseRival tipo);
    }
}
