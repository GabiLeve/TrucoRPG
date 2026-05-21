using TrucoRPG.Logica.Repositorios;
using TrucoRPG.Logica.Servicios;

namespace TrucoRPG.Logica.UseCases
{
    /// <summary>
    /// Caso de uso: registrar un nuevo usuario y devolver un JWT.
    /// </summary>
    public class RegisterUseCase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly TokenService        _tokenService;

        public RegisterUseCase(IUsuarioRepositorio usuarioRepo, TokenService tokenService)
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
