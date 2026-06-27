using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Mapeos
{
    public static class HeroeMapper
    {
        public static HeroeDto ToDto(this Heroe heroe) => new(
            heroe.Id,
            heroe.Nombre,
            heroe.DescripcionHabilidadPasiva,
            heroe.DescripcionHabilidadActiva,
            heroe.TipoHeroe);

        public static IReadOnlyList<HeroeDto> ToDto(this IEnumerable<Heroe> heroes) =>
            heroes.Select(h => h.ToDto()).ToList();
    }
}
