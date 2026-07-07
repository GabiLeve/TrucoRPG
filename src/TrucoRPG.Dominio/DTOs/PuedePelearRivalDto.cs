namespace TrucoRPG.Dominio.DTOs
{
    public record PuedePelearRivalDto(
        int RivalNivel,
        bool PuedePelear,
        string? Motivo
    );
}
