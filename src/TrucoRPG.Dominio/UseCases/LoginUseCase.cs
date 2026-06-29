using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class LoginUseCase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly ITokenService       _tokenService;

        public LoginUseCase(IUsuarioRepositorio usuarioRepo, ITokenService tokenService)
        {
            _usuarioRepo  = usuarioRepo;
            _tokenService = tokenService;
        }

        public async Task<string> EjecutarAsync(string email, string password)
        {
            var valido = await _usuarioRepo.ValidarPasswordAsync(email, password);
            if (!valido)
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            var usuario = await _usuarioRepo.ObtenerPorEmailAsync(email)
                ?? throw new UnauthorizedAccessException("Usuario no encontrado.");

            return _tokenService.GenerarToken(usuario);
        }
    }
}
