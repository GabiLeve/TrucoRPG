namespace TrucoRPG.API.Models
{
    public class CantarEnvidoTipoRequest
    {
        public Guid ManoId { get; set; }
        public string Tipo { get; set; } = "Envido";
    }
}
