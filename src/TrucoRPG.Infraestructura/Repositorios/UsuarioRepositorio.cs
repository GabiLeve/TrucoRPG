using Microsoft.AspNetCore.Identity;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Infraestructura.Entities;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Infraestructura.Repositorios
{
    /// <summary>
    /// Implementación concreta de IUsuarioRepositorio usando ASP.NET Core Identity.
    /// </summary>
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioRepositorio(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            if (appUser is null) return null;

            return new Usuario
            {
                Id       = appUser.Id,
                UserName = appUser.UserName ?? string.Empty,
                Email    = appUser.Email    ?? string.Empty
            };
        }

        public async Task<bool> ExisteEmailAsync(string email)
            => await _userManager.FindByEmailAsync(email) is not null;

        public async Task CrearAsync(string userName, string email, string password)
        {
            var appUser = new ApplicationUser
            {
                UserName = userName,
                Email    = email
            };

            var result = await _userManager.CreateAsync(appUser, password);

            if (!result.Succeeded)
            {
                var errores = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Error al crear usuario: {errores}");
            }
        }

        public async Task<bool> ValidarPasswordAsync(string email, string password)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            if (appUser is null) return false;

            return await _userManager.CheckPasswordAsync(appUser, password);
        }
    }
}
