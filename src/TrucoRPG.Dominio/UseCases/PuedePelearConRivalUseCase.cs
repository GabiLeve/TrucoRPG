using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class PuedePelearConRivalUseCase
    {
        private readonly HistoriaValidacionServicio _validacion;

        public PuedePelearConRivalUseCase(HistoriaValidacionServicio validacion) =>
            _validacion = validacion;

        public async Task<PuedePelearRivalDto> EjecutarAsync(string? usuarioId, int rivalNivel)
        {
            var (puede, motivo) = await _validacion.EvaluarPuedePelearAsync(usuarioId, rivalNivel);
            return new PuedePelearRivalDto(rivalNivel, puede, motivo);
        }
    }
}
