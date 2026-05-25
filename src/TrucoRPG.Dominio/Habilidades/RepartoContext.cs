namespace TrucoRPG.Dominio.Habilidades
{
    public class RepartoContext
    {
        public const double ManipuladorProbMejorarCarta = 0.10;

        public Dictionary<string, double> ProbMejorarCartaPorJugador { get; } = new();

        public Random? Random { get; set; }

        public double ObtenerProbMejorar(string idJugador) =>
            ProbMejorarCartaPorJugador.TryGetValue(idJugador, out var prob) ? prob : 0;
    }
}
