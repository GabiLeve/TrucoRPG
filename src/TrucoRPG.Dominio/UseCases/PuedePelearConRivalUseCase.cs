using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class PuedePelearConRivalUseCase
    {
        private readonly HistoriaValidacionServicio _validacion;

        public PuedePelearConRivalUseCase(HistoriaValidacionServicio validacion) =>
            _validacion = validacion;

        public Task<(bool PuedePelear, string? Motivo)> EjecutarAsync(string? usuarioId, int rivalNivel) =>
            _validacion.EvaluarPuedePelearAsync(usuarioId, rivalNivel);
    }
}
