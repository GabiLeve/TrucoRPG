using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Reglas de negocio del TRUCO en 2v2 (cantar, responder, escalar, irse al mazo).
    /// Es stateless: opera sobre la <see cref="ManoTruco2v2"/> que recibe y la muta.
    /// Lo usan tanto el modo solo (vs bots, vía el Controller) como el multijugador
    /// (vía el Hub). Lo único que cambia entre modos es CÓMO se elige al responsable de
    /// un canto, así que se pasa por parámetro:
    ///   - solo: el humano (J1) decide por su equipo → <see cref="TurnoServicio2v2.ObtenerResponsableTruco"/>
    ///   - multi: responde el rival que sigue en la ronda → <see cref="TurnoServicio2v2.ObtenerResponsableCanto"/>
    /// </summary>
    public static class TrucoServicio2v2
    {
        // El responsable de un canto se pasa como Func<mano, cantorId, responsableId>.

        public static bool PuedeCantar(ManoTruco2v2 mano, string jugadorId) =>
            !mano.TrucoCantado && !mano.TrucoResuelto && mano.GanadorMano == null
            && !mano.ManoTerminada && !mano.PartidaTerminada && mano.TurnoActual == jugadorId;

        /// <summary>Canta el truco (nivel 1). Devuelve false si no corresponde.</summary>
        public static bool Cantar(ManoTruco2v2 mano, string jugadorId, Func<ManoTruco2v2, string, string> responsable)
        {
            if (!PuedeCantar(mano, jugadorId)) return false;

            mano.TrucoCantado      = true;
            mano.CantorTruco       = jugadorId;
            mano.EquipoCantorTruco = mano.ObtenerEquipoDeJugador(jugadorId);
            mano.NivelTruco        = 1;
            mano.PuntosTrucoMano   = 2;
            mano.EstadoTruco       = $"{jugadorId} cantó Truco.";
            mano.TrucoPendienteRespuestaDe = responsable(mano, jugadorId);
            mano.PuedeEscalarTruco = TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, mano.EquipoCantorTruco);
            return true;
        }

        /// <summary>
        /// Responde un truco pendiente: no quiero, quiero, o quiero + escalar (retruco/vale cuatro).
        /// </summary>
        public static bool Responder(ManoTruco2v2 mano, string jugadorId, bool aceptar, string? escalarA, Func<ManoTruco2v2, string, string> responsable)
        {
            if (!mano.TrucoCantado || mano.TrucoPendienteRespuestaDe != jugadorId) return false;
            mano.TrucoPendienteRespuestaDe = null;

            if (!aceptar)
            {
                int pts = mano.NivelTruco;
                mano.TrucoResuelto   = true;
                mano.GanadorMano     = mano.EquipoCantorTruco;
                mano.ManoTerminada   = true;
                mano.PuntosTrucoMano = pts;
                mano.EstadoTruco     = $"{jugadorId} no quiso truco. {mano.EquipoCantorTruco} gana {pts} pt.";
                JuegoServicio2v2.SumarPuntos(mano, mano.EquipoCantorTruco!, pts);
                return true;
            }

            var escalar = escalarA?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(escalar) && mano.NivelTruco < 3 && TurnoServicio2v2.PuedeEscalarTruco(mano, jugadorId))
            {
                SubirNivel(mano, jugadorId, responsable);
                return true;
            }

            mano.TrucoResuelto = true;
            mano.EstadoTruco   = $"{jugadorId} quiso el truco. Vale {mano.PuntosTrucoMano} pt.";
            return true;
        }

        /// <summary>
        /// Sube la apuesta en tu turno (retruco / vale cuatro) cuando ya aceptaste un truco
        /// y tu equipo tiene "la palabra".
        /// </summary>
        public static bool Escalar(ManoTruco2v2 mano, string jugadorId, Func<ManoTruco2v2, string, string> responsable)
        {
            if (mano.GanadorMano != null || mano.ManoTerminada || mano.PartidaTerminada) return false;
            if (!mano.TrucoCantado || mano.TrucoPendienteRespuestaDe != null) return false;
            if (mano.NivelTruco >= 3) return false;
            if (mano.EquipoCantorTruco == mano.ObtenerEquipoDeJugador(jugadorId)) return false; // tu equipo cantó el último
            if (mano.TurnoActual != jugadorId) return false;

            SubirNivel(mano, jugadorId, responsable);
            return true;
        }

        private static void SubirNivel(ManoTruco2v2 mano, string jugadorId, Func<ManoTruco2v2, string, string> responsable)
        {
            string equipo = mano.ObtenerEquipoDeJugador(jugadorId);
            mano.NivelTruco++;
            mano.PuntosTrucoMano   = mano.NivelTruco == 2 ? 3 : 4;
            mano.EquipoCantorTruco = equipo;
            mano.CantorTruco       = jugadorId;
            mano.TrucoResuelto     = false;
            string nombre          = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
            mano.EstadoTruco       = $"{jugadorId} cantó {nombre}.";
            mano.TrucoPendienteRespuestaDe = responsable(mano, jugadorId);
            mano.PuedeEscalarTruco = TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, mano.ObtenerEquipoContrario(equipo).Id);
        }

        /// <summary>Irse al mazo: el equipo del que se va pierde la mano.</summary>
        public static bool IrseAlMazo(ManoTruco2v2 mano, string jugadorId)
        {
            if (mano.GanadorMano != null || mano.ManoTerminada || mano.PartidaTerminada) return false;
            if (mano.TurnoActual != jugadorId && mano.TrucoPendienteRespuestaDe != jugadorId) return false;

            string equipoQueSeVa = mano.ObtenerEquipoDeJugador(jugadorId);
            string equipoGana    = equipoQueSeVa == "EquipoA" ? "EquipoB" : "EquipoA";
            int pts              = mano.TrucoCantado && !mano.TrucoResuelto ? mano.PuntosTrucoMano : 1;

            mano.GanadorMano   = equipoGana;
            mano.ManoTerminada = true;
            mano.TrucoResuelto = true;
            mano.EstadoTruco   = $"{jugadorId} se fue al mazo. {equipoGana} gana {pts} pt.";
            JuegoServicio2v2.SumarPuntos(mano, equipoGana, pts);
            return true;
        }
    }
}
