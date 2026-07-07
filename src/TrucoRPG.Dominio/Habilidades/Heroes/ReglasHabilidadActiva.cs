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

        /// <summary>
        /// Calcula la disponibilidad de la activa al iniciar una mano siguiendo un cooldown
        /// "perezoso": la habilidad está disponible cada mano hasta que el jugador la usa por
        /// primera vez; recién ahí empieza a contar y vuelve a estar disponible después de
        /// <paramref name="cooldownManos"/> manos. Llamar una sola vez por mano (en ManoIniciada).
        /// </summary>
        public static void ActualizarDisponibilidadPorCooldown(EstadoHabilidadesJugador estado, int cooldownManos)
        {
            if (!estado.ActivaUsadaAlgunaVez)
            {
                estado.ActivaDisponible = true;
                return;
            }

            estado.ManosDesdeUltimaActiva++;
            estado.ActivaDisponible = estado.ManosDesdeUltimaActiva >= cooldownManos;
        }

        /// <summary>Registra que la activa se usó: la bloquea esta mano y arranca el cooldown.</summary>
        public static void RegistrarUsoActiva(EstadoHabilidadesJugador estado)
        {
            estado.ActivaUsadaEnEstaMano = true;
            estado.ActivaDisponible = false;
            estado.ActivaUsadaAlgunaVez = true;
            estado.ManosDesdeUltimaActiva = 0;
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
