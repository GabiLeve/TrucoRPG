namespace TrucoRPG.API.DTO
{
    public record PuedePelearRivalDto(
        int RivalNivel,
        bool PuedePelear,
        string? Motivo
    )
    {
        public static PuedePelearRivalDto FromResultado(int rivalNivel, bool puede, string? motivo) =>
            new(rivalNivel, puede, motivo);
    }
}
