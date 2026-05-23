using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Entities;

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
        public DbSet<Heroe> Heroes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new Configurations.HeroeConfiguration());

            builder.Entity<ApplicationUser>()
                   .HasOne(u => u.HeroeSeleccionado)
                   .WithMany()
                   .HasForeignKey(u => u.HeroeSeleccionadoId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
