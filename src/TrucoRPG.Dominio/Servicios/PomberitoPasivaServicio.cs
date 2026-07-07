using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class PomberitoPasivaServicio
    {
        public static void AplicarSiManoSilenciosa(ManoTruco mano)
        {
            if (!EsPomberitoHistoria(mano))
                return;

            if (mano.EnvidoCantado || mano.TrucoCantado)
                return;

            JuegoServicio.SumarPuntos(
                mano, IdJugador.Maquina, 1, OrigenPuntos.PasivaPomberito);

            mano.UltimoMensajeHabilidadRival =
                "Trampa del monte: nadie cantó envido ni truco. El Pomberito suma +1 punto extra.";
        }

        private static bool EsPomberitoHistoria(ManoTruco mano) =>
            mano.Configuracion.HabilidadesRivalActivas
            && mano.Configuracion.RivalDeLaMaquina == ClaseRival.Pomberito;
    }
}
