using System.Collections.Generic;
using System.Linq;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.DTO
{
    public class ObjetoTiendaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Precio { get; set; }
        public string Img { get; set; } = string.Empty;
        public string? SpriteKey { get; set; }
        public bool Acumulable { get; set; }

        public static ObjetoTiendaDto FromDomain(ItemTienda item) => new()
        {
            Id = item.Id,
            Nombre = item.Nombre,
            Descripcion = item.Descripcion,
            Precio = item.Precio,
            Img = item.Img,
            SpriteKey = item.SpriteKey,
            Acumulable = item.Acumulable
        };
    }

    public class CategoriaTiendaDto
    {
        public string Categoria { get; set; } = string.Empty;
        public List<ObjetoTiendaDto> Objetos { get; set; } = new();

        /// <summary>Agrupa los ítems por categoría (en mayúsculas) y los mapea a DTOs.</summary>
        public static List<CategoriaTiendaDto> FromItems(IEnumerable<ItemTienda> items) =>
            items
                .GroupBy(i => i.Categoria.ToUpper())
                .Select(grupo => new CategoriaTiendaDto
                {
                    Categoria = grupo.Key,
                    Objetos = grupo.Select(ObjetoTiendaDto.FromDomain).ToList()
                })
                .ToList();
    }
}
