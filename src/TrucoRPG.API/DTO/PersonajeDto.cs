using System;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.DTO
{
    public class PersonajeDto
    {
        public Guid HeroeId { get; set; }
        public string SpriteKey { get; set; }

        /// <summary>Convierte el DTO recibido en la entidad de dominio Personaje.</summary>
        public Personaje ToDomain() => new Personaje
        {
            HeroeId = HeroeId,
            SpriteKey = SpriteKey
        };
    }
}
