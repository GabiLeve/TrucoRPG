namespace TrucoRPG.Dominio.Habilidades
{
    /// <summary>
    /// Opciones de reparto configuradas por habilidades antes de repartir cartas.
    /// </summary>
    public class RepartoContext
    {
        public const double ManipuladorProbMejorarCarta = 0.10;

        public Dictionary<string, double> ProbMejorarCartaPorJugador { get; } = new();

        /// <summary>Solo para tests; en producción se usa Random() nuevo.</summary>
        public Random? Random { get; set; }

        public double ObtenerProbMejorar(string idJugador) =>
            ProbMejorarCartaPorJugador.TryGetValue(idJugador, out var prob) ? prob : 0;
    }
}
