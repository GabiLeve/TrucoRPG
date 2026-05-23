using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Repositorios
{
    /// <summary>
    /// Contrato para la generación de tokens de autenticación.
    /// La implementación concreta vive en la capa de Infraestructura.
    /// </summary>
    public interface ITokenService
    {
        string GenerarToken(Usuario usuario);
    }
}
