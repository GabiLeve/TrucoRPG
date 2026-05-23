namespace TrucoRPG.API.Models
{
    public class ResponderEnvidoRequest
    {
        public Guid ManoId { get; set; }
        public bool Aceptar { get; set; }

        /// <summary>
        /// Opcional. Si se envía, el humano escala el envido en lugar de solo aceptar.
        /// Valores válidos: "Real Envido", "Falta Envido"
        /// </summary>
        public string? EscalarA { get; set; }
    }
}
