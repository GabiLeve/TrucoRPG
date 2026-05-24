using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Heroes
{
    internal static class ReglasHabilidadActiva
    {
        public static bool EsInicioDeMano(ManoTruco mano) =>
            mano.Bazas.Count == 0 && mano.GanadorMano == null && !mano.PartidaTerminada;

        public static void ReiniciarUsoEnMano(EstadoHabilidadesJugador estado)
        {
            estado.ActivaUsadaEnEstaMano = false;
            estado.TimberoApuestaActiva = false;
            estado.CartaReveladaRival = null;
        }

        public static bool ValidarActivaBase(ContextoPartida ctx, EstadoHabilidadesJugador estado, out string? error)
        {
            error = null;
            if (!ctx.HabilidadesActivas)
            {
                error = "Las habilidades no están activas.";
                return false;
            }

            if (!estado.ActivaDisponible)
            {
                error = "La habilidad activa no está disponible en esta mano.";
                return false;
            }

            if (estado.ActivaUsadaEnEstaMano)
            {
                error = "Ya usaste la habilidad activa en esta mano.";
                return false;
            }

            return true;
        }
    }
}
