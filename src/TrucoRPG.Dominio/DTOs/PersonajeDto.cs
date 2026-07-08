using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace TrucoRPG.Dominio.DTOs
{
    [ExcludeFromCodeCoverage]
    public class PersonajeDto
    {
        public Guid HeroeId { get; set; }
        public string SpriteKey { get; set; }
    }
}
