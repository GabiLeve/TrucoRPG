using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.DTO
{
    public record ProgresoPartidaDto(
        int UltimoRivalDerrotadoNivel,
        int PuntosAcumulados
    )
    {
        public ProgresoPartida ToDomain() => new()
        {
            UltimoRivalDerrotadoNivel = UltimoRivalDerrotadoNivel,
            PuntosAcumulados = PuntosAcumulados
        };

        public static ProgresoPartidaDto FromDomain(ProgresoPartida progreso) => new(
            progreso.UltimoRivalDerrotadoNivel,
            progreso.PuntosAcumulados
        );
    }
}
