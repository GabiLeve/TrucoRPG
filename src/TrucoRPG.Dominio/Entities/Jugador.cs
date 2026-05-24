namespace TrucoRPG.Dominio.Entities
{
    public class Jugador
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public List<Carta> Mano { get; set; } = new();
        public List<Carta> Jugadas { get; set; } = new();
        public bool EsMaquina { get; set; }
    }
}
