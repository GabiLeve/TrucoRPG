using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace TrucoRPG.Infraestructura.Data
{
    public static class InicializadorDatosIdentity
    {
        public static async Task InicializarRolesAsync(IServiceProvider proveedorServicios)
        {
            using var alcance = proveedorServicios.CreateScope();
            var administradorRoles = alcance.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string rolJugador = "Jugador";

            if (!await administradorRoles.RoleExistsAsync(rolJugador))
            {
                await administradorRoles.CreateAsync(new IdentityRole(rolJugador));
            }
        }
    }
}
