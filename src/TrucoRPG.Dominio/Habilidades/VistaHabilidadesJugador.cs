using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    /// <summary>
    /// Resumen para UI: qué héroe tiene el jugador y si puede usar la activa.
    /// </summary>
    public class VistaHabilidadesJugador
    {
        public bool HabilidadesActivasEnPartida { get; set; }
        public ClaseHeroe? ClaseHeroe { get; set; }
        public bool ActivaDisponible { get; set; }
        public bool ActivaUsadaEnEstaMano { get; set; }
        public int ManosDesdeUltimaActiva { get; set; }
        public string? UltimoMensajeHabilidad { get; set; }
        public int? SumaValorTrucoMano { get; set; }
        public string? NombreHeroe { get; set; }
        public ModoJuego? ModoPartida { get; set; }
    }
}
