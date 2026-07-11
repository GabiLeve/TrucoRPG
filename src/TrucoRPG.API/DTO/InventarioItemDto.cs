using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.DTO
{
    /// <summary>
    /// Ítem del inventario del jugador. Proyecta la entidad Inventario
    /// sin exponer el usuario dueño (navegación a ApplicationUser).
    /// </summary>
    public record InventarioItemDto(
        int ItemTiendaId,
        int Cantidad,
        bool Equipado,
        ItemTiendaDto? ItemTienda
    )
    {
        public static InventarioItemDto FromDomain(Inventario inventario) => new(
            inventario.ItemTiendaId,
            inventario.Cantidad,
            inventario.Equipado,
            inventario.ItemTienda is null ? null : ItemTiendaDto.FromDomain(inventario.ItemTienda)
        );
    }
}
