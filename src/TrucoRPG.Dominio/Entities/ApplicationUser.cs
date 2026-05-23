using Microsoft.AspNetCore.Identity;

namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Usuario de Identity. Extiende IdentityUser con campos RPG.
    /// Se añade la referencia al héroe seleccionado (opcional).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public Guid? HeroeSeleccionadoId { get; set; }

        public Heroe? HeroeSeleccionado { get; set; }
    }
}
