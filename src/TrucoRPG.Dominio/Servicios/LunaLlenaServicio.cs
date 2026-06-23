using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class LunaLlenaServicio
    {
        public static void IntentarAlAceptarTrucoMaquina(ManoTruco mano, string cantorId)
        {
            if (!EsLobizonHistoria(mano))
                return;

            if (cantorId != IdJugador.Maquina)
                return;

            if (mano.LunaLlenaUsadaEnMano)
                return;

            RasgunoServicio.DebilitarCartaAleatoria(mano);
            mano.LunaLlenaUsadaEnMano = true;
            mano.UltimoMensajeHabilidadRival =
                "¡Luna llena! El Lobizón debilitó 1 carta de tu mano al aceptar su canto.";
        }

        private static bool EsLobizonHistoria(ManoTruco mano) =>
            mano.Configuracion.HabilidadesRivalActivas
            && mano.Configuracion.RivalDeLaMaquina == ClaseRival.Lobizon;
    }
}
