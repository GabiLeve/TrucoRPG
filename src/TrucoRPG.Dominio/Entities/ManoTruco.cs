using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Entities
{
    public class ManoTruco
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public ConfiguracionPartida Configuracion { get; set; } = new();
        public EstadoHabilidadesPartida EstadoHabilidades { get; set; } = new();
        public VistaHabilidadesJugador? VistaHabilidadesHumano { get; set; }
        public string? UltimoMensajeHabilidad { get; set; }

        /// <summary>Cartas no repartidas; usadas por la activa del Manipulador (fase posterior).</summary>
        public List<Carta> CartasRestantesMazo { get; set; } = new();

        /// <summary>Configuración temporal de reparto; se limpia al terminar Repartir.</summary>
        public RepartoContext? RepartoContext { get; set; }

        public Jugador Humano { get; set; } = new();
        public Jugador Maquina { get; set; } = new();
        public List<Baza> Bazas { get; set; } = new();
        public string TurnoActual { get; set; } = "Humano";
        public string ManoIniciadaPor { get; set; } = "Humano";
        public int NumeroDeMano { get; set; } = 1;
        public string? GanadorMano { get; set; }
        public Carta? CartaMaquinaEnMesa { get; set; }

        public bool EnvidoCantado { get; set; } = false;
        public bool EnvidoResuelto { get; set; } = false;
        public int PuntosEnvido { get; set; } = 0;
        public string? GanadorEnvido { get; set; }
        public int? TantoHumano { get; set; }
        public int? TantoMaquina { get; set; }
        public int? TantoCantadoMaquina { get; set; }
        public string? EstadoEnvido { get; set; }

        public int NivelMentiraEnvidoMaquina { get; set; } = 0;
        public int NivelMentiraTrucoMaquina { get; set; } = 0;
        public bool MaquinaMintioEnvido { get; set; } = false;
        public string? TipoCantoEnvidoMaquina { get; set; }

        public string? CantorEnvido { get; set; }
        public string? TipoEnvidoCantado { get; set; }
        public bool EnvidoPendienteRespuestaHumano { get; set; } = false;
        public bool EnvidoPendienteRespuestaMaquina { get; set; } = false;

        public bool TrucoCantado { get; set; } = false;
        public bool TrucoResuelto { get; set; } = false;
        public bool TrucoPendienteRespuestaHumano { get; set; } = false;
        public int NivelTruco { get; set; } = 0;
        public int PuntosTrucoMano { get; set; } = 1;
        public string? EstadoTruco { get; set; }
        public string? CantorTruco { get; set; }

        public int PuntosHumano { get; set; } = 0;
        public int PuntosMaquina { get; set; } = 0;
        public bool PartidaTerminada { get; set; } = false;
        public string? GanadorPartida { get; set; }
    }
}
