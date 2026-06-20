using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Infraestructura.Data.Configurations
{
    public class ProgresoPartidaConfiguration : IEntityTypeConfiguration<ProgresoPartida>
    {
        public void Configure(EntityTypeBuilder<ProgresoPartida> builder)
        {
            builder.ToTable("ProgresoPartida");

            builder.HasKey(p => p.Id);

            builder.HasIndex(p => p.UsuarioId).IsUnique();

            builder.Property(p => p.UsuarioId).IsRequired().HasMaxLength(450);
            builder.Property(p => p.UltimoRivalDerrotadoNivel).IsRequired();
            builder.Property(p => p.PuntosAcumulados).IsRequired();

            builder.HasOne(p => p.Usuario)
                   .WithMany()
                   .HasForeignKey(p => p.UsuarioId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
