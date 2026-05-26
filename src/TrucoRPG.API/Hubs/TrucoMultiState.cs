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
