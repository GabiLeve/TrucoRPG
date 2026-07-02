namespace TrucoRPG.Dominio.Entities
{
    /// <summary>Contrato común del envido por equipos que comparten ManoTruco2v2 y ManoTruco3v3.</summary>
    public interface IManoEnvidoEquipos
    {
        bool PartidaTerminada { get; }
        string? GanadorMano { get; }
        string EquipoMano { get; }
        int PuntosEquipoA { get; }
        int PuntosEquipoB { get; }

        bool EnvidoCantado { get; set; }
        bool EnvidoResuelto { get; set; }
        string? CantorEnvido { get; set; }
        string? TipoEnvidoCantado { get; set; }
        string? GanadorEnvido { get; set; }
        int PuntosEnvido { get; set; }
        int PuntosEnvidoNoQuiero { get; set; }
        string? EstadoEnvido { get; set; }
        string? EnvidoPendienteRespuestaDe { get; set; }
        string? FaseEnvido { get; set; }
        int IndiceDeclaracionTanto { get; set; }
        Dictionary<string, int?> TantosDeclarados { get; set; }
        Dictionary<string, int> TantosReales { get; set; }
        bool SonBuenasDeclarado { get; set; }
        string? JugadorQueDijoSonBuenas { get; set; }

        bool TrucoCantado { get; }
        bool TrucoResuelto { get; }
        int NivelTruco { get; }
        string? EquipoCantorTruco { get; }

        List<Jugador> OrdenJugadores { get; }
        Jugador? ObtenerJugador(string jugadorId);
        string ObtenerEquipoDeJugador(string jugadorId);
        List<string> JugadoresActivos { get; }
        int VueltasJugadas();
        IEnumerable<Jugador> JugadoresDelEquipo(string equipoId);
    }
}
