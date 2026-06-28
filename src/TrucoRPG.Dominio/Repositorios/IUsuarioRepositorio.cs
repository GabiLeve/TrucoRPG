using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Repositorios
{
    public interface IUsuarioRepositorio
    {
        Task<Usuario?> ObtenerPorEmailAsync(string email);
        Task<bool>     ExisteEmailAsync(string email);
        Task           CrearAsync(string userName, string email, string password);
        Task<bool>     ValidarPasswordAsync(string email, string password);
        Task           CambiarPasswordAsync(string userId, string passwordActual, string passwordNueva);
        Task<string>   GenerarTokenResetPasswordAsync(string email);
        Task           ResetPasswordConTokenAsync(string email, string token, string nuevaPassword);
        Task CrearPersonaje(string userId, string spriteKey, Guid habilidad);
        Task<bool> PersonajeExistente(string userId);
        Task<Personaje> ObtenerPersonajeDelUsuario(string userId);
    }
}
