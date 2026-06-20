namespace TrucoRPG.Dominio.Entities
{
    public class ConfiguracionPartida
    {
        public ModoJuego Modo { get; set; } = ModoJuego.Tradicional;

        public ClaseHeroe? HeroeDelHumano { get; set; }

        public ClaseRival? RivalDeLaMaquina { get; set; }

        public int? RivalNivel { get; set; }

        public bool HabilidadesActivas =>
            Modo is ModoJuego.Historia or ModoJuego.Multijugador && HeroeDelHumano.HasValue;

        public bool HabilidadesRivalActivas =>
            Modo is ModoJuego.Historia && RivalDeLaMaquina.HasValue;
    }
}
