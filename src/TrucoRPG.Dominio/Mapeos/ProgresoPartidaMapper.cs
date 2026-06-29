using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Mapeos
{
    public static class ProgresoPartidaMapper
    {
        public static ProgresoPartidaDto ToDto(this ProgresoPartida progreso) => new(
            progreso.UltimoRivalDerrotadoNivel,
            progreso.PuntosAcumulados);

        public static ProgresoPartidaDto VacioDto() => new(0, 0);
    }
}
