namespace TrucoRPG.API.Models
{
    public record ResetPasswordDto(string Email, string Token, string NuevaPassword);
}
