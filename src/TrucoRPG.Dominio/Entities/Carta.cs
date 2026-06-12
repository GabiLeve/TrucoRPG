namespace TrucoRPG.Dominio.Entities
{
    public class Carta
    {
        public string Palo { get; set; } = "";
        public string? PaloVisual { get; set; }
        public int Numero { get; set; }
        public int ValorTruco { get; set; }
    }
}
