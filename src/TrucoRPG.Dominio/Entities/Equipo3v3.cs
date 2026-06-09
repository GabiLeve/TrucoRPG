namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Representa uno de los dos equipos en una partida 3v3.
    /// EquipoA = posiciones 1, 3 y 5 (índices 0, 2, 4 en el array de jugadores).
    /// EquipoB = posiciones 2, 4 y 6 (índices 1, 3, 5).
    /// </summary>
    public class Equipo3v3
    {
        public string Id { get; set; } = "";       // "EquipoA" o "EquipoB"
        public string Nombre { get; set; } = "";

        /// <summary>Primer jugador del equipo en orden de mesa (pos 1 o 2).</summary>
        public Jugador Jugador1 { get; set; } = new();

        /// <summary>Segundo jugador del equipo en orden de mesa (pos 3 o 4).</summary>
        public Jugador Jugador2 { get; set; } = new();

        /// <summary>Tercer jugador del equipo en orden de mesa (pos 5 o 6).</summary>
        public Jugador Jugador3 { get; set; } = new();

        public int PuntosEquipo { get; set; } = 0;

        public List<Jugador> Jugadores => new() { Jugador1, Jugador2, Jugador3 };

        public bool ContieneJugador(string jugadorId) =>
            Jugador1.Id == jugadorId || Jugador2.Id == jugadorId || Jugador3.Id == jugadorId;

        public Jugador? ObtenerJugador(string jugadorId) =>
            Jugador1.Id == jugadorId ? Jugador1 :
            Jugador2.Id == jugadorId ? Jugador2 :
            Jugador3.Id == jugadorId ? Jugador3 : null;
    }
}
