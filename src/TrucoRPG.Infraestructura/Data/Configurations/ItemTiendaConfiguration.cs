using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Infraestructura.Data.Configurations
{
    public class ItemTiendaConfiguration : IEntityTypeConfiguration<ItemTienda>
    {
        public void Configure(EntityTypeBuilder<ItemTienda> builder)
        {
            builder.ToTable("items");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Nombre)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.Descripcion)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(i => i.Precio)
                   .IsRequired();

            builder.Property(i => i.Categoria)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(i => i.Img)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(i => i.SpriteKey)
                   .HasMaxLength(100)
                   .IsRequired(false);

            builder.HasData(
                new ItemTienda { Id = 1, Nombre = "BOLEADORAS", Descripcion = "Te otorga la habilidad del manipulador en una partida", Precio = 150, Categoria = "HABILIDADES", Img = "/assets/objetos/objeto.png" },
                new ItemTienda { Id = 2, Nombre = "BOLEADORAS", Descripcion = "Te otorga la habilidad del timbero en una partida", Precio = 150, Categoria = "HABILIDADES", Img = "/assets/objetos/objeto.png" },
                new ItemTienda { Id = 3, Nombre = "BOLEADORAS", Descripcion = "Te otorga la habilidad del fanfarron en una partida", Precio = 150, Categoria = "HABILIDADES", Img = "/assets/objetos/objeto.png" },
                new ItemTienda { Id = 4, Nombre = "BOLEADORAS", Descripcion = "Te otorga la habilidad del mentiroso en una partida", Precio = 150, Categoria = "HABILIDADES", Img = "/assets/objetos/objeto.png" },

                // SpriteKey es solo el sufijo de color: el front lo compone con el personaje base del usuario (ej. "personaje2" + "azul").
                new ItemTienda { Id = 5, Nombre = "Poncho rosa", Descripcion = "Cambia el color de tu Poncho a rosa", Precio = 150, Categoria = "ARMARIO", Img = "/assets/objetos/GotaRosa.png", SpriteKey = "rosa" },
                new ItemTienda { Id = 6, Nombre = "Poncho marrón", Descripcion = "Cambia el color de tu Poncho a marrón", Precio = 150, Categoria = "ARMARIO", Img = "/assets/objetos/GotaMarron.png", SpriteKey = "marron" },
                new ItemTienda { Id = 7, Nombre = "Poncho rojo", Descripcion = "Cambia el color de tu Poncho a rojo", Precio = 150, Categoria = "ARMARIO", Img = "/assets/objetos/GotaRoja.png", SpriteKey = "rojo" },
                new ItemTienda { Id = 8, Nombre = "Poncho azul", Descripcion = "Cambia el color de tu Poncho a azul", Precio = 150, Categoria = "ARMARIO", Img = "/assets/objetos/GotaAzul.png", SpriteKey = "azul" }
            );
        }
    }
}
