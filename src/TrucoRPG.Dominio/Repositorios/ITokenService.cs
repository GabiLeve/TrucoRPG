using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Repositorios
{
    public interface ITokenService
    {
        string GenerarToken(Usuario usuario);
    }
}
