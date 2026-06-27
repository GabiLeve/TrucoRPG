using Microsoft.AspNetCore.Identity;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Infraestructura.Repositorios
{
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
                Email    = appUser.Email    ?? string.Empty,
                Monedas = appUser.Monedas,
                SpriteKey = appUser.SpriteKey,
                HeroeSeleccionadoId = appUser.HeroeSeleccionadoId
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

            var resultadoRol = await _userManager.AddToRoleAsync(appUser, "Jugador");

            if (!resultadoRol.Succeeded)
            {
                throw new InvalidOperationException("El usuario se creó pero no se le pudo asignar el rol de Jugador.");
            }
        }

        public async Task<bool> ValidarPasswordAsync(string email, string password)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            if (appUser is null) return false;

            return await _userManager.CheckPasswordAsync(appUser, password);
        }

        public async Task CambiarPasswordAsync(string userId, string passwordActual, string passwordNueva)
        {
            var appUser = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("Usuario no encontrado.");

            var result = await _userManager.ChangePasswordAsync(appUser, passwordActual, passwordNueva);

            if (!result.Succeeded)
            {
                var errores = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errores);
            }
        }

        public async Task<string> GenerarTokenResetPasswordAsync(string email)
        {
            var appUser = await _userManager.FindByEmailAsync(email)
                ?? throw new InvalidOperationException("No existe una cuenta con ese email.");

            return await _userManager.GeneratePasswordResetTokenAsync(appUser);
        }

        public async Task ResetPasswordConTokenAsync(string email, string token, string nuevaPassword)
        {
            var appUser = await _userManager.FindByEmailAsync(email)
                ?? throw new InvalidOperationException("Usuario no encontrado.");

            var result = await _userManager.ResetPasswordAsync(appUser, token, nuevaPassword);

            if (!result.Succeeded)
            {
                var errores = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errores);
            }
        }

        public async Task CrearPersonaje(string userId, string spriteKey, Guid idHabilidad)
        {
            var appUser = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("Usuario no encontrado.");

            if (appUser.HeroeSeleccionadoId != null || appUser.SpriteKey != null) {
                throw new InvalidOperationException("El usuario ya tiene personaje");
            }

            appUser.SpriteKey = spriteKey;
            appUser.HeroeSeleccionadoId = idHabilidad;


            await _userManager.UpdateAsync(appUser);
        }

        public async Task<bool> PersonajeExistente(string userId)
        {
            var appUser = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("Usuario no encontrado.");

            return appUser.HeroeSeleccionadoId != null && !string.IsNullOrEmpty(appUser.SpriteKey);
        }
    }
}
