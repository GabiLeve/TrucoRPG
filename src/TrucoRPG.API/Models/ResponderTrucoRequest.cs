namespace TrucoDemo.Models
{
    public class ResponderTrucoRequest
    {
        public Guid ManoId { get; set; }
        public bool Aceptar { get; set; }

        public string? EscalarA { get; set; }
    }
}
