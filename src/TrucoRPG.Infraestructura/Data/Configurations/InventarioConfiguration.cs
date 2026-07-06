using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Infraestructura.Data.Configurations
{
    public class InventarioConfiguration : IEntityTypeConfiguration<Inventario>
    {
        public void Configure(EntityTypeBuilder<Inventario> builder)
        {
            builder.HasKey(i => new { i.UsuarioId, i.ItemTiendaId });

            builder.HasOne(i => i.Usuario)
                   .WithMany()
                   .HasForeignKey(i => i.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.ItemTienda)
                   .WithMany()
                   .HasForeignKey(i => i.ItemTiendaId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(i => i.Cantidad)
                   .IsRequired()
                   .HasDefaultValue(1);

            builder.Property(i => i.Equipado)
                   .HasDefaultValue(false);
        }
    }
}
