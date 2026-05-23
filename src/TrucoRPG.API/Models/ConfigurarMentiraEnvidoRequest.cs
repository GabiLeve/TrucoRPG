namespace TrucoRPG.API.Models
{
    public class ConfigurarMentiraEnvidoRequest
    {
        public Guid ManoId { get; set; }
        public bool PermitirMentira { get; set; }
    }
}
