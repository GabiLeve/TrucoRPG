namespace TrucoRPG.Dominio.Entities
{
    public class ProgresoPartida
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string UsuarioId { get; set; } = string.Empty;

        public ApplicationUser? Usuario { get; set; }

        public int UltimoRivalDerrotadoNivel { get; set; }

        public int PuntosAcumulados { get; set; }
    }
}
