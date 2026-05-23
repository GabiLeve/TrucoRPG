namespace TrucoRPG.Entidades
{
    /// <summary>
    /// Modelo de dominio del usuario (independiente de Identity).
    /// </summary>
    public class Usuario
    {
        public string Id       { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
    }
}
