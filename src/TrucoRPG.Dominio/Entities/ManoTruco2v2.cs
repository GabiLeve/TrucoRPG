namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Estado completo del juego en modo 2v2.
    /// EquipoA contiene jugadores en posiciones 1 y 3.
    /// EquipoB contiene jugadores en posiciones 2 y 4.
    /// Orden de turnos: posicion1 → posicion2 → posicion3 → posicion4 (horario).
    /// </summary>
    public class ManoTruco2v2
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int NumeroDeMano { get; set; } = 1;

        // ─── Jugadores (orden de mesa) ────────────────────────────────────
        // Posicion1 y Posicion3 → EquipoA; Posicion2 y Posicion4 → EquipoB
        public Jugador Posicion1 { get; set; } = new();
        public Jugador Posicion2 { get; set; } = new();
        public Jugador Posicion3 { get; set; } = new();
        public Jugador Posicion4 { get; set; } = new();

        public Equipo2v2 EquipoA { get; set; } = new() { Id = "EquipoA", Nombre = "Equipo A" };
        public Equipo2v2 EquipoB { get; set; } = new() { Id = "EquipoB", Nombre = "Equipo B" };

        // ─── Turno ────────────────────────────────────────────────────────
        /// <summary>Id del jugador que debe jugar ahora.</summary>
        public string TurnoActual { get; set; } = "";

        /// <summary>Id del jugador que es "mano" en esta ronda (tiene la ventaja).</summary>
        public string JugadorMano { get; set; } = "";

        /// <summary>Id del equipo que es "mano" ("EquipoA" o "EquipoB").</summary>
        public string EquipoMano { get; set; } = "";

        // ─── Vueltas ─────────────────────────────────────────────────────
        public List<Vuelta2v2> Vueltas { get; set; } = new();

        /// <summary>Vuelta actual en progreso (puede ser null si terminó).</summary>
        public Vuelta2v2? VueltaActual { get; set; }

        public string? GanadorMano { get; set; }   // "EquipoA" o "EquipoB"
        public bool ManoTerminada { get; set; } = false;

        // ─── Envido ───────────────────────────────────────────────────────
        public bool EnvidoCantado { get; set; } = false;
        public bool EnvidoResuelto { get; set; } = false;
        public string? CantorEnvido { get; set; }   // jugadorId del que cantó
        public string? TipoEnvidoCantado { get; set; }
        public string? GanadorEnvido { get; set; }  // "EquipoA" o "EquipoB"
        public int PuntosEnvido { get; set; } = 0;
        /// <summary>Puntos que se pagan si el envido se rechaza ("no quiero"): el valor de la
        /// apuesta ANTERIOR a la última (Envido→1, Envido Envido→2, etc.).</summary>
        public int PuntosEnvidoNoQuiero { get; set; } = 1;
        public string? EstadoEnvido { get; set; }

        /// <summary>jugadorId del jugador que debe responder el envido.</summary>
        public string? EnvidoPendienteRespuestaDe { get; set; }

        /// <summary>Fase del envido: null, "pendiente_respuesta", "declarando_tantos".</summary>
        public string? FaseEnvido { get; set; }

        /// <summary>Índice del próximo jugador que debe declarar su tanto (en orden de turno empezando por el no-mano).</summary>
        public int IndiceDeclaracionTanto { get; set; } = 0;

        /// <summary>Tantos declarados por cada jugador. Key = jugadorId.</summary>
        public Dictionary<string, int?> TantosDeclarados { get; set; } = new();

        /// <summary>Tantos reales de cada jugador. Key = jugadorId.</summary>
        public Dictionary<string, int> TantosReales { get; set; } = new();

        /// <summary>Si alguien dijo "son buenas", se resuelve inmediatamente.</summary>
        public bool SonBuenasDeclarado { get; set; } = false;
        public string? JugadorQueDijoSonBuenas { get; set; }

        // ─── Truco ────────────────────────────────────────────────────────
        public bool TrucoCantado { get; set; } = false;
        public bool TrucoResuelto { get; set; } = false;
        public string? CantorTruco { get; set; }    // jugadorId
        public string? EquipoCantorTruco { get; set; } // "EquipoA" o "EquipoB"
        public int NivelTruco { get; set; } = 0;
        public int PuntosTrucoMano { get; set; } = 1;
        public string? EstadoTruco { get; set; }

        /// <summary>jugadorId del jugador que debe responder el truco.</summary>
        public string? TrucoPendienteRespuestaDe { get; set; }

        /// <summary>Último jugador del equipo contrario que puede escalar el truco.</summary>
        public string? PuedeEscalarTruco { get; set; }

        // ─── Puntos partida ───────────────────────────────────────────────
        public int PuntosEquipoA { get; set; } = 0;
        public int PuntosEquipoB { get; set; } = 0;
        public bool PartidaTerminada { get; set; } = false;
        public string? GanadorPartida { get; set; }  // "EquipoA" o "EquipoB"

        // ─── Consulta de envido del compañero (modo solo 2v2) ─────────────
        public bool CompaConsultaEnvido { get; set; } = false;
        public bool CompaEnvidoConsultado { get; set; } = false;
        public bool CompaConsultaTruco { get; set; } = false;
        public bool CompaTrucoConsultado { get; set; } = false;
        public string? CompaPista { get; set; }  // pista de envido del compañero ("Tengo poco/algo/mucho")
        /// <summary>
        /// Si no es null, este jugador bot (el compañero del humano) debe jugar su carta de
        /// mayor ValorTruco en su próximo turno (orden del humano vía botón Acciones).
        /// Se limpia automáticamente al ejecutar la acción. Espejo del modo 3v3.
        /// </summary>
        public string? OrdenJugarMayor { get; set; }

        // ─── Helpers ─────────────────────────────────────────────────────
        public List<Jugador> OrdenJugadores => new() { Posicion1, Posicion2, Posicion3, Posicion4 };

        public Jugador? ObtenerJugador(string jugadorId) =>
            OrdenJugadores.FirstOrDefault(j => j.Id == jugadorId);

        public string ObtenerEquipoDeJugador(string jugadorId) =>
            EquipoA.ContieneJugador(jugadorId) ? "EquipoA" : "EquipoB";

        public Equipo2v2 ObtenerEquipo(string equipoId) =>
            equipoId == "EquipoA" ? EquipoA : EquipoB;

        public Equipo2v2 ObtenerEquipoContrario(string equipoId) =>
            equipoId == "EquipoA" ? EquipoB : EquipoA;

        public int ObtenerPuntosEquipo(string equipoId) =>
            equipoId == "EquipoA" ? PuntosEquipoA : PuntosEquipoB;
    }
}
