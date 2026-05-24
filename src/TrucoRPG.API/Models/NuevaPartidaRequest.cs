using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.Models
{
    public class NuevaPartidaRequest
    {
        public ModoJuego Modo { get; set; } = ModoJuego.Tradicional;
        public ClaseHeroe? ClaseHeroe { get; set; }
    }
}
