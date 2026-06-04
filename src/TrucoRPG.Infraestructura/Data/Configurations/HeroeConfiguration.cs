using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Infraestructura.Data.Configurations
{
    public class HeroeConfiguration : IEntityTypeConfiguration<Heroe>
    {
        public void Configure(EntityTypeBuilder<Heroe> builder)
        {
            builder.ToTable("Heroes");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.Nombre)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(h => h.DescripcionHabilidadPasiva)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(h => h.DescripcionHabilidadActiva)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(h => h.TipoHeroe)
                   .IsRequired();

            _ = builder.HasData(
                new Heroe
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Nombre = "Manipulador",
                    DescripcionHabilidadPasiva = "10% más de probabilidad de recibir carta de valor alto.",
                    DescripcionHabilidadActiva = "Cada 3 manos, puede reemplazar una carta a eleccion de su mano por otra aleatoria del mazo. La nueva carta nunca puede ser de menor valor que la descartada.",
                    TipoHeroe = ClaseHeroe.Manipulador
                },
                new Heroe
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Nombre = "Timbero",
                    DescripcionHabilidadPasiva = "El Timbero lanza una moneda:Cara → obtiene +1 punto. Cruz → no ocurre nada",
                    DescripcionHabilidadActiva = "Debe activarse antes de comenzar la ronda. Si gana el truco, duplica los puntos de la ronda. Si pierde, el rival gana +2 puntos extra.",
                    TipoHeroe = ClaseHeroe.Timbero
                },
                new Heroe
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Nombre = "Fanfarron",
                    DescripcionHabilidadPasiva = "En caso de empate de envido, en vez de definirse el ganador por quien es mano, el Fanfarrón gana automáticamente el empate.",
                    DescripcionHabilidadActiva = "El próximo Truco / Retruco / Vale 4 o Envido cantado por el Fanfarrón vale +1 punto adicional si es aceptado.",
                    TipoHeroe = ClaseHeroe.Fanfarron
                },
                new Heroe
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Nombre = "Mentiroso",
                    DescripcionHabilidadPasiva = "El rival no puede ver cuándo El Mentiroso tiene habilidad disponible. Cuando usa su habilidad, el rival no recibe ninguna notificación visual.",
                    DescripcionHabilidadActiva = "Cada 2 manos revela UNA carta aleatoria del rival durante toda la ronda. Solo puede usarse al comienzo de la mano.",
                    TipoHeroe = ClaseHeroe.Mentiroso
                }
            );
        }
    }
}
