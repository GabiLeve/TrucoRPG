namespace TrucoDemo.Clases
{
    public class Jugador
    {
        public string Nombre { get; set; } = "";
        public List<Carta> Mano { get; set; } = new();
        public List<Carta> Jugadas { get; set; } = new();
        public bool EsMaquina { get; set; }
    }
}
