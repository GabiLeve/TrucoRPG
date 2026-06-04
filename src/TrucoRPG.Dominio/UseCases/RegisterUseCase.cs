using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class RegisterUseCase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly ITokenService       _tokenService;

        public RegisterUseCase(IUsuarioRepositorio usuarioRepo, ITokenService tokenService)
        {
            _usuarioRepo  = usuarioRepo;
            _tokenService = tokenService;
        }

        public async Task<string> EjecutarAsync(string userName, string email, string password)
        {
            if (await _usuarioRepo.ExisteEmailAsync(email))
                throw new InvalidOperationException("El email ya está en uso.");

            await _usuarioRepo.CrearAsync(userName, email, password);

            var usuario = await _usuarioRepo.ObtenerPorEmailAsync(email)
                ?? throw new InvalidOperationException("Error al obtener el usuario recién creado.");

            return _tokenService.GenerarToken(usuario);
        }
    }
}
