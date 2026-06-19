using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.API.Hubs;
/// <summary>
/// Estado de sala 1v1 en el hub: SOLO el mapeo de conexiones. Todo el estado de juego
/// vive en <see cref="EstadoTrucoMulti1v1"/> (Dominio) y lo muta TrucoMulti1v1Servicio.
/// </summary>
public class TrucoMultiState : EstadoTrucoMulti1v1
{
    public string Jugador1Id { get; set; } = ""; // Host  → rol "Humano"
    public string Jugador2Id { get; set; } = ""; // Guest → rol "Maquina"
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

/// <summary>Estado completo del juego 3v3 en el hub.</summary>
public class TrucoMultiState3v3
{
    // Conexiones de los 6 jugadores (en orden de posicion 1-6)
    public string[] JugadoresIds { get; set; } = new string[6];

    public ManoTruco3v3 Mano { get; set; } = new();

    // Mapeo connectionId → posicion (1-6)
    public Dictionary<string, int> Posiciones { get; set; } = new();

    public string GetJugadorId(string connectionId) =>
        Posiciones.TryGetValue(connectionId, out var pos)
            ? $"J{pos}"
            : "";
}
