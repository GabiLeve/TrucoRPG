using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Infraestructura.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Heroe> Heroes { get; set; } = null!;
        public DbSet<Rival> Rivales { get; set; } = null!;
        public DbSet<ProgresoPartida> ProgresoPartida { get; set; } = null!;
        public DbSet<ItemTienda> Items { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Identity
            builder.Entity<ApplicationUser>().ToTable("aspnetusers");
            builder.Entity<IdentityRole>().ToTable("aspnetroles");
            builder.Entity<IdentityUserRole<string>>().ToTable("aspnetuserroles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("aspnetuserclaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("aspnetuserlogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("aspnetroleclaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("aspnetusertokens");

            // Heroes
            builder.ApplyConfiguration(new Configurations.HeroeConfiguration());
            builder.ApplyConfiguration(new Configurations.RivalConfiguration());
            builder.ApplyConfiguration(new Configurations.ProgresoPartidaConfiguration());
            builder.ApplyConfiguration(new Configurations.ItemTiendaConfiguration());

            builder.Entity<ApplicationUser>()
                   .HasOne(u => u.HeroeSeleccionado)
                   .WithMany()
                   .HasForeignKey(u => u.HeroeSeleccionadoId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
