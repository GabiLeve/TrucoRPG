using System.Security.Claims;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Infraestructura.Provider
{
    public class UsuarioActualServicio : IUsuarioActualServicio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuarioActualServicio(IHttpContextAccessor httpContextAccessor) =>
            _httpContextAccessor = httpContextAccessor;

        public string? ObtenerId() =>
            _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
