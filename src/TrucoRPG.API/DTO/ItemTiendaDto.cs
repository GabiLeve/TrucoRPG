using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.DTO
{
    /// <summary>Proyección de un ítem de la tienda para exponer por la API.</summary>
    public record ItemTiendaDto(
        int Id,
        string Nombre,
        string Descripcion,
        int Precio,
        string Categoria,
        string Img,
        string? SpriteKey,
        bool Acumulable
    )
    {
        public static ItemTiendaDto FromDomain(ItemTienda item) => new(
            item.Id,
            item.Nombre,
            item.Descripcion,
            item.Precio,
            item.Categoria,
            item.Img,
            item.SpriteKey,
            item.Acumulable
        );
    }
}
