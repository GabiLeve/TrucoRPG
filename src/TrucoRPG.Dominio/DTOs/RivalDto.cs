using TrucoRPG.Dominio.Entities;
using System.Diagnostics.CodeAnalysis;

namespace TrucoRPG.Dominio.DTOs
{
    [ExcludeFromCodeCoverage]
    public record RivalDto(
        Guid Id,
        int Nivel,
        string Nombre,
        string Descripcion,
        string NombreHabilidad,
        string DescripcionHabilidad,
        ClaseRival TipoRival,
        TipoHabilidadRival TipoHabilidad
    );
}
