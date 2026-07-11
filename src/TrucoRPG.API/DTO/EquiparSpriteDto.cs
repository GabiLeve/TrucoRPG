using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.DTO
{
    public class EquiparSpriteDto
    {
        public string SpriteKeyNuevo { get; set; }

        /// <summary>Convierte el DTO en la entidad de dominio con el sprite a equipar.</summary>
        public Personaje ToDomain() => new Personaje
        {
            SpriteKey = SpriteKeyNuevo
        };
    }
}
