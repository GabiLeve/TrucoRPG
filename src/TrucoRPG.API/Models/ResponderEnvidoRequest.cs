namespace TrucoRPG.API.Models
{
    public class ResponderEnvidoRequest
    {
        public Guid ManoId { get; set; }
        public bool Aceptar { get; set; }
        public string? EscalarA { get; set; }
    }
}
