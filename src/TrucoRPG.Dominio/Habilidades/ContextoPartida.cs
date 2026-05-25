using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    public class ContextoPartida
    {
        public ManoTruco Mano { get; }

        public ContextoPartida(ManoTruco mano) => Mano = mano;

        public bool HabilidadesActivas => Mano.Configuracion.HabilidadesActivas;

        public ConfiguracionPartida Configuracion => Mano.Configuracion;

        public EstadoHabilidadesPartida EstadoHabilidades => Mano.EstadoHabilidades;

        public Jugador Jugador(string idJugador) => idJugador switch
        {
            IdJugador.Humano  => Mano.Humano,
            IdJugador.Maquina => Mano.Maquina,
            _ => throw new ArgumentException($"Jugador desconocido: {idJugador}", nameof(idJugador))
        };

        public IEnumerable<string> RivalesDe(string idJugador)
        {
            if (idJugador == IdJugador.Humano)
                yield return IdJugador.Maquina;
            else if (idJugador == IdJugador.Maquina)
                yield return IdJugador.Humano;
        }

        public EstadoHabilidadesJugador EstadoDe(string idJugador, ClaseHeroe? claseHeroe = null) =>
            EstadoHabilidades.ObtenerOCrear(idJugador, claseHeroe);
    }
}
