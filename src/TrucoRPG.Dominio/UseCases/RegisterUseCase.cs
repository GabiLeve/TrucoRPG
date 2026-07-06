using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class RegisterUseCase : IRegisterUseCase
    {
        private readonly IUsuarioRepositorio _usuarioRepo;
        private readonly ITokenService       _tokenService;
        private readonly IProgresoPartidaRepositorio _progreso;

        public RegisterUseCase(
            IUsuarioRepositorio usuarioRepo,
            ITokenService tokenService,
            IProgresoPartidaRepositorio progreso)
        {
            _usuarioRepo  = usuarioRepo;
            _tokenService = tokenService;
            _progreso     = progreso;
        }

        public async Task<string> EjecutarAsync(string userName, string email, string password)
        {
            if (await _usuarioRepo.ExisteEmailAsync(email))
                throw new InvalidOperationException("El email ya está en uso.");

            await _usuarioRepo.CrearAsync(userName, email, password);

            var usuario = await _usuarioRepo.ObtenerPorEmailAsync(email)
                ?? throw new InvalidOperationException("Error al obtener el usuario recién creado.");

            await _progreso.ObtenerOCrearAsync(usuario.Id);

            return _tokenService.GenerarToken(usuario);
        }
    }
}
