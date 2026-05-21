using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TrucoRPG.Infraestructura.Entities;

namespace TrucoRPG.Infraestructura.Data
{
    /// <summary>
    /// Contexto de EF Core. Hereda de IdentityDbContext para que
    /// las tablas de ASP.NET Identity se creen automáticamente.
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
