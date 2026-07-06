using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ResetPasswordUseCase : IResetPasswordUseCase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;

        public ResetPasswordUseCase(IUsuarioRepositorio usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        public async Task EjecutarAsync(string email, string token, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
                throw new InvalidOperationException("La nueva contraseña debe tener al menos 6 caracteres.");

            await _usuarioRepo.ResetPasswordConTokenAsync(email, token, nuevaPassword);
        }
    }
}
