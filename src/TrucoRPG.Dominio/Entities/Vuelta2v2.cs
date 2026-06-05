namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Una vuelta (ronda de cartas) en el juego 2v2.
    /// Cada vuelta contiene las 4 cartas jugadas (una por jugador) y el ganador (equipo).
    /// </summary>
    public class Vuelta2v2
    {
        /// <summary>Cartas jugadas en esta vuelta. Key = jugadorId, Value = carta jugada.</summary>
        public Dictionary<string, Carta> CartasJugadas { get; set; } = new();

        /// <summary>
        /// "EquipoA", "EquipoB" o "Parda".
        /// Null si la vuelta aún no está completa.
        /// </summary>
        public string? GanadorVuelta { get; set; }

        /// <summary>Mejor carta del EquipoA en esta vuelta.</summary>
        public Carta? MejorCartaEquipoA { get; set; }

        /// <summary>Mejor carta del EquipoB en esta vuelta.</summary>
        public Carta? MejorCartaEquipoB { get; set; }

        /// <summary>Devuelve true si los 4 jugadores ya jugaron su carta.</summary>
        public bool EstaCompleta(IEnumerable<string> ordenJugadores) =>
            ordenJugadores.All(id => CartasJugadas.ContainsKey(id));
    }
}
