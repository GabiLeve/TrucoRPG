using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Repositorios
{
    /// <summary>
    /// Contrato de dominio para persistencia de usuarios.
    /// La implementación concreta vive en Infraestructura.
    /// </summary>
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<bool>     ExisteEmailAsync(string email);
        Task           CrearAsync(string userName, string email, string password);
        Task<bool>     ValidarPasswordAsync(string email, string password);
    }
}
