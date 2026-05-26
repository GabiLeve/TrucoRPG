using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    public class EstadoHabilidadesJugador
    {
        public string IdJugador { get; set; } = "";
        public ClaseHeroe? ClaseHeroe { get; set; }

        public int ManosDesdeUltimaActiva { get; set; }
        public bool ActivaDisponible { get; set; }
        public bool ActivaUsadaEnEstaMano { get; set; }

        public bool TimberoApuestaActiva { get; set; }
        public bool FanfarronBonusPendiente { get; set; }

        public Entities.Carta? CartaReveladaRival { get; set; }
    }
}
