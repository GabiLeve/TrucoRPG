namespace TrucoRPG.API.Models
{
    public class Truco1v1PasoResponse
    {
        public Dominio.Entities.ManoTruco Mano { get; set; } = null!;
        public Dominio.Servicios.Truco1v1EventoMaquina? Evento { get; set; }
    }
}
