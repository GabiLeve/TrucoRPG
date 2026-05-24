namespace TrucoRPG.API.Models
{
    public class ActivarHabilidadRequest
    {
        public Guid ManoId { get; set; }
        public int? NumeroCarta { get; set; }
        public string? PaloCarta { get; set; }
    }
}
