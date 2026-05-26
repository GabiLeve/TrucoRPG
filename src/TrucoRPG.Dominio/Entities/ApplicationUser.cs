using Microsoft.AspNetCore.Identity;

namespace TrucoRPG.Dominio.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? HeroeSeleccionadoId { get; set; }

        public Heroe? HeroeSeleccionado { get; set; }
    }
}
