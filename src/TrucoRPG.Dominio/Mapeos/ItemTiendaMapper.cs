using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Mapeos
{
    public static class ItemTiendaMapper
    {
        public static ObjetoTiendaDto ToDto(this ItemTienda item) => new()
        {
            Id = item.Id,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            Precio = item.Precio,
            Img = item.Img,
            SpriteKey = item.SpriteKey,
            Acumulable = item.Acumulable
        };

        /// <summary>Agrupa los ítems por categoría (en mayúsculas) y los mapea a DTOs.</summary>
        public static List<CategoriaTiendaDto> ToCategoriasDto(this IEnumerable<ItemTienda> items) =>
            items
                .GroupBy(i => i.Categoria.ToUpper())
                .Select(grupo => new CategoriaTiendaDto
                {
                    Categoria = grupo.Key,
                    Objetos = grupo.Select(o => o.ToDto()).ToList()
                })
                .ToList();
    }
}
