namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Estado completo del juego en modo 3v3.
    /// EquipoA contiene jugadores en posiciones 1, 3 y 5.
    /// EquipoB contiene jugadores en posiciones 2, 4 y 6.
    /// Orden de turnos (horario): Pos1 → Pos2 → Pos3 → Pos4 → Pos5 → Pos6.
    /// </summary>
    public class ManoTruco3v3
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int NumeroDeMano { get; set; } = 1;

        // ─── Jugadores (orden de mesa) ────────────────────────────────────
        public Jugador Posicion1 { get; set; } = new();
        public Jugador Posicion2 { get; set; } = new();
        public Jugador Posicion3 { get; set; } = new();
        public Jugador Posicion4 { get; set; } = new();
        public Jugador Posicion5 { get; set; } = new();
        public Jugador Posicion6 { get; set; } = new();

        public Equipo3v3 EquipoA { get; set; } = new() { Id = "EquipoA", Nombre = "Equipo A" };
        public Equipo3v3 EquipoB { get; set; } = new() { Id = "EquipoB", Nombre = "Equipo B" };

        // ─── Turno ────────────────────────────────────────────────────────
        public string TurnoActual { get; set; } = "";
        public string JugadorMano { get; set; } = "";
        public string EquipoMano { get; set; } = "";

        // ─── Vueltas ─────────────────────────────────────────────────────
        public List<Vuelta3v3> Vueltas { get; set; } = new();
        public Vuelta3v3? VueltaActual { get; set; }

        public string? GanadorMano { get; set; }   // "EquipoA" o "EquipoB"
        public bool ManoTerminada { get; set; } = false;

        // ─── Envido ───────────────────────────────────────────────────────
        public bool EnvidoCantado { get; set; } = false;
        public bool EnvidoResuelto { get; set; } = false;
        public string? CantorEnvido { get; set; }
        public string? TipoEnvidoCantado { get; set; }
        public string? GanadorEnvido { get; set; }
        public int PuntosEnvido { get; set; } = 0;
        public int PuntosEnvidoNoQuiero { get; set; } = 1;
        public string? EstadoEnvido { get; set; }
        public string? EnvidoPendienteRespuestaDe { get; set; }
        public string? FaseEnvido { get; set; }
        public int IndiceDeclaracionTanto { get; set; } = 0;
        public Dictionary<string, int?> TantosDeclarados { get; set; } = new();
        public Dictionary<string, int> TantosReales { get; set; } = new();
        public bool SonBuenasDeclarado { get; set; } = false;
        public string? JugadorQueDijoSonBuenas { get; set; }

        // ─── Truco ────────────────────────────────────────────────────────
        public bool TrucoCantado { get; set; } = false;
        public bool TrucoResuelto { get; set; } = false;
        public string? CantorTruco { get; set; }
        public string? EquipoCantorTruco { get; set; }
        public int NivelTruco { get; set; } = 0;
        public int PuntosTrucoMano { get; set; } = 1;
        public string? EstadoTruco { get; set; }
        public string? TrucoPendienteRespuestaDe { get; set; }
        public string? PuedeEscalarTruco { get; set; }

        // ─── Puntos partida ───────────────────────────────────────────────
        public int PuntosEquipoA { get; set; } = 0;
        public int PuntosEquipoB { get; set; } = 0;
        public bool PartidaTerminada { get; set; } = false;
        public string? GanadorPartida { get; set; }

        // ─── Pica-Pica ─────────────────────────────────────────────────────
        /// <summary>Jugadores que participan de esta mano (en orden de mesa). En 3v3 normal
        /// son los 6; en Pica-Pica solo J1 y J4 (1 vs 1 contra el de enfrente).</summary>
        public List<string> JugadoresActivos { get; set; } = new();

        /// <summary>True cuando la mano se juega en modo Pica-Pica (1 vs 1).</summary>
        public bool PicaPica { get; set; } = false;

        /// <summary>
        /// Posición dentro del ciclo Pica-Pica (0,1,2 = duelo; 3 = mano redonda).
        /// -1 indica que el modo Pica-Pica aún no fue activado.
        /// </summary>
        public int PicaPicaSlot { get; set; } = -1;

        // ─── Consulta de los compañeros bot (modo solo 3v3) ───────────────
        public bool CompaConsultaEnvido { get; set; } = false;
        public bool CompaEnvidoConsultado { get; set; } = false;
        public bool CompaConsultaTruco { get; set; } = false;
        public bool CompaTrucoConsultado { get; set; } = false;
        /// <summary>Pista del tanto/fuerza de un compañero ("Tengo mucho/algo/poco").</summary>
        public string? CompaPista { get; set; }
        /// <summary>Id del compañero que está preguntando (J3 o J5).</summary>
        public string? CompaConsultor { get; set; }
        /// <summary>
        /// Si no es null, este jugador bot debe jugar su carta de mayor ValorTruco
        /// en su próximo turno (orden del humano vía botón Acciones).
        /// Se limpia automáticamente al ejecutar la acción.
        /// </summary>
        public string? OrdenJugarMayor { get; set; }

        // ─── Helpers ─────────────────────────────────────────────────────
        public List<Jugador> OrdenJugadores =>
            new() { Posicion1, Posicion2, Posicion3, Posicion4, Posicion5, Posicion6 };

        public Jugador? ObtenerJugador(string jugadorId) =>
            OrdenJugadores.FirstOrDefault(j => j.Id == jugadorId);

        public string ObtenerEquipoDeJugador(string jugadorId) =>
            EquipoA.ContieneJugador(jugadorId) ? "EquipoA" : "EquipoB";

        public Equipo3v3 ObtenerEquipo(string equipoId) =>
            equipoId == "EquipoA" ? EquipoA : EquipoB;

        public Equipo3v3 ObtenerEquipoContrario(string equipoId) =>
            equipoId == "EquipoA" ? EquipoB : EquipoA;

        public int ObtenerPuntosEquipo(string equipoId) =>
            equipoId == "EquipoA" ? PuntosEquipoA : PuntosEquipoB;
    }
}
