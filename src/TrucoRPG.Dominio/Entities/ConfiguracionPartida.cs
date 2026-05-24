namespace TrucoRPG.Dominio.Entities
{
    /// <summary>
    /// Opciones de la partida que no cambian entre manos (modo, héroe elegido, etc.).
    /// </summary>
    public class ConfiguracionPartida
    {
        public ModoJuego Modo { get; set; } = ModoJuego.Tradicional;

        /// <summary>Héroe del jugador humano (historia) o del jugador local (multijugador futuro).</summary>
        public ClaseHeroe? HeroeDelHumano { get; set; }

        /// <summary>Truco puro: sin pasivas ni activas.</summary>
        public bool HabilidadesActivas =>
            Modo is ModoJuego.Historia or ModoJuego.Multijugador && HeroeDelHumano.HasValue;
    }
}
