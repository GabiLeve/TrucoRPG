using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Repositorios
{
    public interface IProgresoPartidaRepositorio
    {
        Task<ProgresoPartida?> ObtenerPorUsuarioIdAsync(string usuarioId);
        Task<ProgresoPartida> ObtenerOCrearAsync(string usuarioId);
        Task RegistrarVictoriaAsync(string usuarioId, int rivalNivelDerrotado, int diferenciaPuntos);
        Task ReiniciarRivalesAsync(string usuarioId);
    }
}
