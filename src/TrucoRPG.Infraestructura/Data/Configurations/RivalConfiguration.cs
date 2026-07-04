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
                        "Cada 2 manos, cambia el palo de 2 cartas. Pasiva Remolino: 50% de cambiar el palo de tu primera carta en la 1.ª baza.",
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
                        "Cada 2 manos, muestra tus cartas 5s y oculta 2. Pasiva Trampa del monte: +1 pt si nadie cantó envido ni truco.",
                    TipoRival = ClaseRival.Pomberito,
                    TipoHabilidad = TipoHabilidadRival.Travesura
                },
                new Rival
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                    Nivel = 3,
                    Nombre = "El Lobizón",
                    Descripcion = "Tercer jefe de la historia. Acecha en las profundidades de la cueva.",
                    NombreHabilidad = "Rasguño",
                    DescripcionHabilidad =
                        "Rasguño: te cambia una carta aleatoria por una de menor valor (puede ocurrir en cualquier momento de la ronda)." +
                        "\nAullido: su aullido te asusta y te manda al mazo.",
                    TipoRival = ClaseRival.Lobizon,
                    TipoHabilidad = TipoHabilidadRival.Rasguno
                },
                new Rival
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                    Nivel = 4,
                    Nombre = "La Luz Mala",
                    Descripcion = "Cuarto jefe de la historia. Una presencia luminosa que desorienta al viajero.",
                    NombreHabilidad = "Destello",
                    DescripcionHabilidad =
                        "Destello: cada 2 turnos en bazas 1 o 2, te obliga a jugar una carta al azar. " +
                        "Espejismo (pasiva): si es mano y abre la baza 1, muestra una carta falsa en pantalla hasta que respondas.",
                    TipoRival = ClaseRival.LuzMala,
                    TipoHabilidad = TipoHabilidadRival.Destello
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
