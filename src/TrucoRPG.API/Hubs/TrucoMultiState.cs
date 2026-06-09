using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.Hubs;
public class TrucoMultiState
{
    public string Jugador1Id { get; set; } = ""; // Host  → rol "Humano"
    public string Jugador2Id { get; set; } = ""; // Guest → rol "Maquina"

    public ManoTruco Mano { get; set; } = new();

    // Carta que el primer jugador de una baza ya jugó, esperando al segundo
    public Carta? CartaPendienteJ1 { get; set; }

    // Truco: el campo equivalente para J2 no existe en ManoTruco, lo rastreamos aquí
    public bool TrucoPendienteRespuestaJ2 { get; set; } = false;
    public bool EnvidoPendienteRespuestaJ2 { get; set; } = false;
}

/// <summary>Estado completo del juego 2v2 en el hub.</summary>
public class TrucoMultiState2v2
{
    // Conexiones de los 4 jugadores (en orden de posicion 1-4)
    public string[] JugadoresIds { get; set; } = new string[4];

    public ManoTruco2v2 Mano { get; set; } = new();

    // Mapeo connectionId → posicion (1-4)
    public Dictionary<string, int> Posiciones { get; set; } = new();

    public string GetJugadorId(string connectionId) =>
        Posiciones.TryGetValue(connectionId, out var pos)
            ? $"J{pos}"
            : "";
}
