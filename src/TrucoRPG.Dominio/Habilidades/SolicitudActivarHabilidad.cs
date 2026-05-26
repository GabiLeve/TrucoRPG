namespace TrucoRPG.Dominio.Habilidades
{
    public class SolicitudActivarHabilidad
    {
        public string IdJugador { get; set; } = "";
        public int? NumeroCarta { get; set; }
        public string? PaloCarta { get; set; }
    }
}
