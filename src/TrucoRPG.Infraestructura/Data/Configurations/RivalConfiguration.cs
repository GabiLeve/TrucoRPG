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
                },
                new Rival
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                    Nivel = 3,
                    Nombre = "El Lobizón",
                    Descripcion = "Tercer jefe de la historia. Acecha en las profundidades de la cueva.",
                    NombreHabilidad = "Sin habilidad",
                    DescripcionHabilidad = "Combate sin habilidades especiales.",
                    TipoRival = ClaseRival.Lobizon,
                    TipoHabilidad = TipoHabilidadRival.Ninguna
                },
                new Rival
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                    Nivel = 4,
                    Nombre = "La Luz Mala",
                    Descripcion = "Cuarto jefe de la historia. Una presencia luminosa que desorienta al viajero.",
                    NombreHabilidad = "Destello",
                    DescripcionHabilidad =
                        "Emite una luz radiante que te confunde y te hace jugar una carta al azar " +
                        "(puede ocurrir en cualquier momento de la ronda).",
                    TipoRival = ClaseRival.LuzMala,
                    TipoHabilidad = TipoHabilidadRival.Ninguna
                },
                new Rival
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"),
                    Nivel = 5,
                    Nombre = "Mandinga",
                    Descripcion = "Jefe final de la historia. Domina el trono con tres fases de combate.",
                    NombreHabilidad = "Fases",
                    DescripcionHabilidad =
                        "Jefe final con 3 fases y distintas habilidades según los puntos que le quedan para ganar. (Próximamente.)",
                    TipoRival = ClaseRival.Mandinga,
                    TipoHabilidad = TipoHabilidadRival.Ninguna
                });
        }
    }
}
