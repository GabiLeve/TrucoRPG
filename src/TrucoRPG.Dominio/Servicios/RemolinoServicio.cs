using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class RemolinoServicio
    {
        public const double ProbabilidadActivacion = 0.50;

        public static bool IntentarEnPrimeraBaza(ManoTruco mano, Carta cartaEnMesa)
        {
            if (!EsNahuelitoHistoria(mano))
                return false;

            if (mano.Bazas.Count != 0 || mano.GanadorMano != null)
                return false;

            if (!AzarServicio.TirarProbabilidad(ProbabilidadActivacion))
                return false;

            SalpicaduraServicio.CambiarPaloCarta(cartaEnMesa);
            mano.UltimoMensajeHabilidadRival =
                $"¡Remolino! Nahuelito cambió el palo de tu carta a {cartaEnMesa.Palo}.";
            return true;
        }

        private static bool EsNahuelitoHistoria(ManoTruco mano) =>
            mano.Configuracion.HabilidadesRivalActivas
            && mano.Configuracion.RivalDeLaMaquina == ClaseRival.Nahuelito;
    }
}
