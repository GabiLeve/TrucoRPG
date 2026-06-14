using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Infraestructura.Data.Configurations
{
    public class RivalConfiguration : IEntityTypeConfiguration<Rival>
    {
        public void Configure(EntityTypeBuilder<Rival> builder)
        {
            builder.ToTable("Rivales");

            builder.HasKey(r => r.Id);

            builder.HasIndex(r => r.Nivel).IsUnique();

            builder.Property(r => r.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(r => r.Descripcion).IsRequired().HasMaxLength(1000);
            builder.Property(r => r.NombreHabilidad).IsRequired().HasMaxLength(100);
            builder.Property(r => r.DescripcionHabilidad).IsRequired().HasMaxLength(1000);
            builder.Property(r => r.TipoRival).IsRequired();
            builder.Property(r => r.TipoHabilidad).IsRequired();

            builder.HasData(
                new Rival
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                    Nivel = 1,
                    Nombre = "Nahuelito",
                    Descripcion = "Primer jefe de la historia. Habita las orillas del lago.",
                    NombreHabilidad = "Salpicadura",
                    DescripcionHabilidad =
                        "Cada 2 manos, cambia los palos de tus cartas (ej. Espada se ve/vuelve Copa).",
                    TipoRival = ClaseRival.Nahuelito,
                    TipoHabilidad = TipoHabilidadRival.Salpicadura
                },
                new Rival
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                    Nivel = 2,
                    Nombre = "El Pomberito",
                    Descripcion = "Segundo jefe de la historia. Guarda la entrada de la cueva.",
                    NombreHabilidad = "Travesura",
                    DescripcionHabilidad =
                        "Cada 2 manos, te muestra tus 3 cartas 5 segundos y luego oculta 2 al azar.",
                    TipoRival = ClaseRival.Pomberito,
                    TipoHabilidad = TipoHabilidadRival.Travesura
                });
        }
    }
}
