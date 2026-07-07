using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class AullidoServicio
    {
        public const double ProbabilidadActivacion = 0.20;

        public static bool IntentarTrasPrimeraBaza(ManoTruco mano, string ganadorBaza)
        {
            if (!EsLobizonHistoria(mano))
                return false;

            if (mano.AullidoUsadoEnMano || mano.AullidoBloqueando || mano.GanadorMano != null)
                return false;

            if (mano.Bazas.Count != 1 || ganadorBaza != IdJugador.Humano)
                return false;

            if (!AzarServicio.TirarProbabilidad(ProbabilidadActivacion))
                return false;

            mano.AullidoBloqueando = true;
            mano.AullidoUsadoEnMano = true;
            mano.UltimoMensajeHabilidadRival =
                "¡Aullido! El Lobizón te asustó. Te vas al mazo...";
            return true;
        }

        public static void EjecutarIrAlMazo(ManoTruco mano)
        {
            mano.EnvidoPendienteRespuestaHumano = false;
            mano.TrucoPendienteRespuestaHumano = false;
            mano.CartaHumanoEnMesa = null;
            mano.CartaMaquinaEnMesa = null;

            int puntosParaMaquina = mano.TrucoCantado && mano.PuntosTrucoMano > 0
                ? mano.PuntosTrucoMano
                : 1;

            mano.GanadorMano = IdJugador.Maquina;
            mano.TrucoResuelto = true;
            mano.EstadoTruco =
                $"¡Aullido! Te fuiste al mazo. La máquina gana {puntosParaMaquina} punto(s).";
            mano.AullidoBloqueando = false;

            JuegoServicio.SumarPuntos(
                mano, IdJugador.Maquina, puntosParaMaquina, OrigenPuntos.AullidoLobizon, mano.CantorTruco);
        }

        private static bool EsLobizonHistoria(ManoTruco mano) =>
            mano.Configuracion.HabilidadesRivalActivas
            && mano.Configuracion.RivalDeLaMaquina == ClaseRival.Lobizon;
    }
}
