namespace TrucoRPG.Dominio.Entities
{
    public class Usuario
    {
        public string Id       { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public int Monedas { get; set; }
        public string? SpriteKey { get; set; }
        public Guid? HeroeSeleccionadoId { get; set; }
    }
}
