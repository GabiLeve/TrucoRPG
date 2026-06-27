namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Representa uno de los dos equipos en una partida 2v2.
    /// EquipoA = posiciones 1 y 3 (índices 0 y 2 en el array de jugadores).
    /// EquipoB = posiciones 2 y 4 (índices 1 y 3).
    /// </summary>
    public class Equipo2v2
    {
        public string Id { get; set; } = "";       // "EquipoA" o "EquipoB"
        public string Nombre { get; set; } = "";

        /// <summary>Jugador en la posición 1 o 2 de la mesa (el primero del equipo en orden de turno).</summary>
        public Jugador Jugador1 { get; set; } = new();

        /// <summary>Jugador en la posición 3 o 4 de la mesa (el segundo del equipo en orden de turno).</summary>
        public Jugador Jugador2 { get; set; } = new();

        public int PuntosEquipo { get; set; } = 0;

        public List<Jugador> Jugadores => new() { Jugador1, Jugador2 };

        public bool ContieneJugador(string jugadorId) =>
            Jugador1.Id == jugadorId || Jugador2.Id == jugadorId;

        public Jugador? ObtenerJugador(string jugadorId) =>
            Jugador1.Id == jugadorId ? Jugador1 :
            Jugador2.Id == jugadorId ? Jugador2 : null;
    }
}
