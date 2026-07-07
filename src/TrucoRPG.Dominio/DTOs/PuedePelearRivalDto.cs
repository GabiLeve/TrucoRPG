using System.Diagnostics.CodeAnalysis;

namespace TrucoRPG.Dominio.DTOs
{
    [ExcludeFromCodeCoverage]
    public record PuedePelearRivalDto(
        int RivalNivel,
        bool PuedePelear,
        string? Motivo
    );
}
