using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.DTO
{
    /// <summary>Proyección de un rival del modo historia para exponer por la API.</summary>
    public record RivalDto(
        int Nivel,
        string Nombre,
        string Descripcion,
        string NombreHabilidad,
        string DescripcionHabilidad,
        ClaseRival TipoRival,
        TipoHabilidadRival TipoHabilidad
    )
    {
        public static RivalDto FromDomain(Rival rival) => new(
            rival.Nivel,
            rival.Nombre,
            rival.Descripcion,
            rival.NombreHabilidad,
            rival.DescripcionHabilidad,
            rival.TipoRival,
            rival.TipoHabilidad
        );
    }
}
