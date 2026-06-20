using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.DTOs
{
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
