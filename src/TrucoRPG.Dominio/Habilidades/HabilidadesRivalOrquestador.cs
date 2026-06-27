using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    public static class HabilidadesRivalOrquestador
    {
        public static void Disparar(ManoTruco mano, EventoPartida evento, object? datos = null)
        {
            if (!mano.Configuracion.HabilidadesRivalActivas)
            {
                ActualizarVista(mano);
                return;
            }

            var contexto = new ContextoPartida(mano);
            var rival = mano.Configuracion.RivalDeLaMaquina!.Value;
            var habilidad = RivalHabilidadFactory.CrearDesdeRival(rival);
            habilidad.OnEvento(contexto, evento, datos);

            ActualizarVista(mano);
        }

        public static void ActualizarVista(ManoTruco mano) =>
            mano.VistaHabilidadesRival = HabilidadesConsultaRivalServicio.ObtenerVista(mano);
    }
}
