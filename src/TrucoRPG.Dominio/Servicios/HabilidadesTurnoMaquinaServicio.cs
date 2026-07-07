using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class HabilidadesTurnoMaquinaServicio
    {
        public static void Notificar(ManoTruco mano)
        {
            if (mano.TurnoActual != IdJugador.Maquina || mano.GanadorMano != null)
                return;

            HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.TurnoMaquina);
        }
    }
}
