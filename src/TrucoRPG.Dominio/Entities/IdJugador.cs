namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Identificadores estables de jugador en partida. En 1v1 son Humano/Maquina;
    /// en multijugador se agregarán asientos (Jugador1..Jugador4) sin cambiar el motor de habilidades.
    /// </summary>
    public static class IdJugador
    {
        public const string Humano = "Humano";
        public const string Maquina = "Maquina";
    }
}
