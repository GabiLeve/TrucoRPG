namespace TrucoRPG.Dominio.Entities;

/// <summary>
/// Reglas de configuración por modo de juego: cantidad de jugadores necesarios.
/// Centraliza estas constantes de dominio que antes vivían en la capa API.
/// </summary>
public static class ModoJuegoConfig
{
    /// <summary>Cantidad total de jugadores requeridos para iniciar una partida.</summary>
    public static int JugadoresRequeridos(string modo) => modo switch
    {
        "2v2" => 4,
        "3v3" => 6,
        _     => 2,
    };

    /// <summary>Jugadores por equipo (cada equipo tiene la mitad del total).</summary>
    public static int JugadoresPorEquipo(string modo) => modo == "3v3" ? 3 : 2;
}
