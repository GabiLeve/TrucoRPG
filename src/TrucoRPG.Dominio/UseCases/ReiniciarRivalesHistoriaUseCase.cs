using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    /// <summary>
    /// Permite rejugar el modo historia: vuelve a 0 únicamente el estado de
    /// rivales derrotados. Los puntos, monedas, habilidades y ropa se conservan.
    /// </summary>
    public class ReiniciarRivalesHistoriaUseCase
    {
        private readonly IProgresoPartidaRepositorio _progreso;

        public ReiniciarRivalesHistoriaUseCase(IProgresoPartidaRepositorio progreso) =>
            _progreso = progreso;

        public async Task EjecutarAsync(string? usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new UnauthorizedAccessException("Debés iniciar sesión para reiniciar el progreso.");

            await _progreso.ReiniciarRivalesAsync(usuarioId);
        }
    }
}
