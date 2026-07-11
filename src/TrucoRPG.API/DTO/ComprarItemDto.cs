using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.DTO
{
    public class ComprarItemDto
    {
        public int ItemTiendaId { get; set; }

        /// <summary>Convierte el DTO en la entidad de dominio ItemTienda a comprar.</summary>
        public ItemTienda ToDomain() => new ItemTienda
        {
            Id = ItemTiendaId
        };
    }
}
