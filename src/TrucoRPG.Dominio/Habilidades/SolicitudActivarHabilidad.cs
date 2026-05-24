namespace TrucoRPG.Dominio.Habilidades
{
    /// <summary>
    /// Datos para activar una habilidad (fase posterior: carta a cambiar, etc.).
    /// </summary>
    public class SolicitudActivarHabilidad
    {
        public string IdJugador { get; set; } = "";
        public int? NumeroCarta { get; set; }
        public string? PaloCarta { get; set; }
    }
}
