using Microsoft.AspNetCore.Identity;

namespace TrucoRPG.Dominio.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? HeroeSeleccionadoId { get; set; }

        public Heroe? HeroeSeleccionado { get; set; }

        public int Monedas { get; set; } = 500;

        public string? SpriteKey { get; set; }

    }
}
