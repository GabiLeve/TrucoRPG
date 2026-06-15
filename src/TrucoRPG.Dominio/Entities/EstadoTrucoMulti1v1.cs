namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Estado de juego de una partida 1v1 MULTIJUGADOR (dos humanos reales).
    /// <see cref="ManoTruco"/> fue pensada para humano-vs-máquina, así que los campos
    /// que le faltan al segundo jugador real (J2) se trackean acá.
    /// La capa API (Hub) extiende esta clase solo para agregar los connection ids.
    /// </summary>
    public class EstadoTrucoMulti1v1
    {
        public ManoTruco Mano { get; set; } = new();

        /// <summary>Carta que el primer jugador de una baza ya jugó, esperando al segundo.</summary>
        public Carta? CartaPendienteJ1 { get; set; }

        // Truco/Envido: el campo equivalente para J2 no existe en ManoTruco, va acá.
        public bool TrucoPendienteRespuestaJ2 { get; set; } = false;
        public bool EnvidoPendienteRespuestaJ2 { get; set; } = false;

        // Cadena de cantos del envido (se acumulan: Envido + Real Envido = 5, etc.)
        /// <summary>Puntos que vale el envido si se acepta (0 = Falta Envido, se calcula al resolver).</summary>
        public int PuntosEnvidoEnJuego { get; set; } = 0;
        /// <summary>Puntos que paga rechazar el último canto (lo apostado ANTES de la última suba).</summary>
        public int PuntosEnvidoNoQuiero { get; set; } = 1;
    }
}
