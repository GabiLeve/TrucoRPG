using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class CambiarPasswordUseCase : ICambiarPasswordUseCase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;

        public CambiarPasswordUseCase(IUsuarioRepositorio usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        public async Task EjecutarAsync(string userId, string passwordActual, string passwordNueva)
        {
            if (string.IsNullOrWhiteSpace(passwordNueva) || passwordNueva.Length < 6)
                throw new InvalidOperationException("La nueva contraseña debe tener al menos 6 caracteres.");

            await _usuarioRepo.CambiarPasswordAsync(userId, passwordActual, passwordNueva);
        }
    }
}
