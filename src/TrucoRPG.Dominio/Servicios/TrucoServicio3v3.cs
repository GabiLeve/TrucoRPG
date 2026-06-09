using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Reglas de negocio del TRUCO en 3v3 (cantar, responder, escalar, irse al mazo).
    /// Stateless: opera sobre la <see cref="ManoTruco3v3"/> que recibe y la muta.
    /// Lo usan tanto el modo solo (vs bots) como el multijugador. El responsable de un
    /// canto se pasa por parámetro:
    ///   - solo: el humano (J1) decide por su equipo.
    ///   - multi: responde el rival que sigue en la ronda → <see cref="TurnoServicio3v3.ObtenerResponsableCanto"/>
    /// Espejo de <see cref="TrucoServicio2v2"/>.
    /// </summary>
    public static class TrucoServicio3v3
    {
        public static bool PuedeCantar(ManoTruco3v3 mano, string jugadorId) =>
            !mano.TrucoCantado && !mano.TrucoResuelto && mano.GanadorMano == null
            && !mano.ManoTerminada && !mano.PartidaTerminada && mano.TurnoActual == jugadorId;

        /// <summary>Canta el truco (nivel 1). Devuelve false si no corresponde.</summary>
        public static bool Cantar(ManoTruco3v3 mano, string jugadorId, Func<ManoTruco3v3, string, string> responsable)
        {
            if (!PuedeCantar(mano, jugadorId)) return false;

            mano.TrucoCantado      = true;
            mano.CantorTruco       = jugadorId;
            mano.EquipoCantorTruco = mano.ObtenerEquipoDeJugador(jugadorId);
            mano.NivelTruco        = 1;
            mano.PuntosTrucoMano   = 2;
            mano.EstadoTruco       = $"{jugadorId} cantó Truco.";
            mano.TrucoPendienteRespuestaDe = responsable(mano, jugadorId);
            mano.PuedeEscalarTruco = TurnoServicio3v3.ObtenerUltimoDelEquipoEnTurno(mano, mano.EquipoCantorTruco);
            return true;
        }

        /// <summary>Responde un truco pendiente: no quiero, quiero, o quiero + escalar.</summary>
        public static bool Responder(ManoTruco3v3 mano, string jugadorId, bool aceptar, string? escalarA, Func<ManoTruco3v3, string, string> responsable)
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
                JuegoServicio3v3.SumarPuntos(mano, mano.EquipoCantorTruco!, pts);
                return true;
            }

            var escalar = escalarA?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(escalar) && mano.NivelTruco < 3 && TurnoServicio3v3.PuedeEscalarTruco(mano, jugadorId))
            {
                SubirNivel(mano, jugadorId, responsable);
                return true;
            }

            mano.TrucoResuelto = true;
            mano.EstadoTruco   = $"{jugadorId} quiso el truco. Vale {mano.PuntosTrucoMano} pt.";
            return true;
        }

        /// <summary>Sube la apuesta en tu turno (retruco / vale cuatro) cuando ya aceptaste un truco.</summary>
        public static bool Escalar(ManoTruco3v3 mano, string jugadorId, Func<ManoTruco3v3, string, string> responsable)
        {
            if (mano.GanadorMano != null || mano.ManoTerminada || mano.PartidaTerminada) return false;
            if (!mano.TrucoCantado || mano.TrucoPendienteRespuestaDe != null) return false;
            if (mano.NivelTruco >= 3) return false;
            if (mano.EquipoCantorTruco == mano.ObtenerEquipoDeJugador(jugadorId)) return false;
            if (mano.TurnoActual != jugadorId) return false;

            SubirNivel(mano, jugadorId, responsable);
            return true;
        }

        private static void SubirNivel(ManoTruco3v3 mano, string jugadorId, Func<ManoTruco3v3, string, string> responsable)
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
            mano.PuedeEscalarTruco = TurnoServicio3v3.ObtenerUltimoDelEquipoEnTurno(mano, mano.ObtenerEquipoContrario(equipo).Id);
        }

        /// <summary>Irse al mazo: el equipo del que se va pierde la mano.</summary>
        public static bool IrseAlMazo(ManoTruco3v3 mano, string jugadorId)
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
            JuegoServicio3v3.SumarPuntos(mano, equipoGana, pts);
            return true;
        }
    }
}
