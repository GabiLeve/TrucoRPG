namespace TrucoRPG.Dominio.Habilidades
{
    /// <summary>
    /// Payload mutable para el evento AntesDeSumarPuntos (fases posteriores).
    /// </summary>
    public class ModificadorPuntos
    {
        public string? GanadorId { get; set; }
        public int PuntosBase { get; set; }
        public int PuntosFinales { get; set; }
        public int BonusAlRival { get; set; }
        public bool DuplicarPuntosGanador { get; set; }

        public void AplicarBase(int puntos, string? ganadorId)
        {
            GanadorId = ganadorId;
            PuntosBase = puntos;
            PuntosFinales = puntos;
        }
    }
}
