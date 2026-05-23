using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrucoRPG.Infraestructura.Entities;
using TrucoRPG.Entidades;

namespace TrucoRPG.Infraestructura.Data
{
    /// <summary>
    /// Contexto de EF Core. Hereda de IdentityDbContext para que
    /// las tablas de ASP.NET Identity se creen automáticamente.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet de Héroes
        public DbSet<Entidades.Heroe> Heroes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Aplicar configuración de Heroes
            builder.ApplyConfiguration(new Configurations.HeroeConfiguration());

            // Configuración explícita de la relación ApplicationUser.HeroeSeleccionado (opcional)
            builder.Entity<ApplicationUser>()
                   .HasOne(u => u.HeroeSeleccionado)
                   .WithMany() // no navegación inversa en Heroe
                   .HasForeignKey(u => u.HeroeSeleccionadoId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
