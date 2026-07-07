using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Mapeos
{
    public static class RivalMapper
    {
        public static RivalDto ToDto(this Rival rival) => new(
            rival.Id,
            rival.Nivel,
            rival.Nombre,
            rival.Descripcion,
            rival.NombreHabilidad,
            rival.DescripcionHabilidad,
            rival.TipoRival,
            rival.TipoHabilidad);

        public static IReadOnlyList<RivalDto> ToDto(this IEnumerable<Rival> rivales) =>
            rivales.Select(r => r.ToDto()).ToList();
    }
}
