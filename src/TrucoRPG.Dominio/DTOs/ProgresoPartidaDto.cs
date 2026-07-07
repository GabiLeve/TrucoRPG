using System.Diagnostics.CodeAnalysis;

namespace TrucoRPG.Dominio.DTOs
{
    [ExcludeFromCodeCoverage]
    public record ProgresoPartidaDto(
        int UltimoRivalDerrotadoNivel,
        int PuntosAcumulados
    );
}
