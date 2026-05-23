using Microsoft.AspNetCore.Identity;
using TrucoRPG.Entidades;
using System;

namespace TrucoRPG.Infraestructura.Entities
{
    /// <summary>
    /// Usuario de Identity. Extendible con campos RPG.
    /// Se añade la referencia al héroe seleccionado (opcional).
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // FK al héroe seleccionado por el usuario (nullable: puede no tener héroe seleccionado)
        public Guid? HeroeSeleccionadoId { get; set; }

        // Navegación al héroe seleccionado (opcional)
        public Heroe? HeroeSeleccionado { get; set; }
    }
}
