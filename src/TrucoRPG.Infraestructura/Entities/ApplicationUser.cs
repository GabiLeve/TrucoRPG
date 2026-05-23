using Microsoft.AspNetCore.Identity;
using System;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Infraestructura.Entities
{
    /// <summary>
    /// Usuario de Identity. Extendible con campos RPG.
    /// Se añade la referencia al héroe seleccionado (opcional).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public Guid? HeroeSeleccionadoId { get; set; }

        public Heroe? HeroeSeleccionado { get; set; }
    }
}
