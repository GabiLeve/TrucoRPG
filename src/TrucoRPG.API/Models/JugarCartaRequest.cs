namespace TrucoRPG.API.Models
{
    public class JugarCartaRequest
    {
        public Guid ManoId { get; set; }
        public int Numero { get; set; }
        public string Palo { get; set; } = string.Empty;
    }
}
