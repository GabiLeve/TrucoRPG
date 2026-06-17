using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Maneja el orden de turnos, quién puede escalar el truco y quién responde
    /// los cantos en un juego 3v3.
    /// Equipos: EquipoA = posiciones 1, 3 y 5; EquipoB = posiciones 2, 4 y 6.
    /// Orden de mesa (horario): Pos1 → Pos2 → Pos3 → Pos4 → Pos5 → Pos6.
    /// Mismo criterio que <see cref="TurnoServicio2v2"/>, generalizado a 6 jugadores.
    /// </summary>
    public static class TurnoServicio3v3
    {
        /// <summary>
        /// Orden de turno completo de la vuelta actual empezando por el jugador mano.
        /// </summary>
        public static List<string> ObtenerOrdenTurno(ManoTruco3v3 mano)
        {
            // En Pica-Pica solo juegan los activos (J1 y J4); en 3v3 normal, los 6.
            var todos = (mano.JugadoresActivos != null && mano.JugadoresActivos.Count > 0)
                ? new List<string>(mano.JugadoresActivos)
                : mano.OrdenJugadores.Select(j => j.Id).ToList();
            int indexMano = todos.IndexOf(mano.JugadorMano);
            if (indexMano < 0) return todos;

            var ordenado = new List<string>();
            for (int i = 0; i < todos.Count; i++)
                ordenado.Add(todos[(indexMano + i) % todos.Count]);
            return ordenado;
        }

        /// <summary>Siguiente jugador en el orden de turno después de jugadorActualId.</summary>
        public static string? SiguienteJugador(ManoTruco3v3 mano, string jugadorActualId)
        {
            var orden = ObtenerOrdenTurno(mano);
            int idx = orden.IndexOf(jugadorActualId);
            if (idx < 0) return null;
            return orden[(idx + 1) % orden.Count];
        }

        /// <summary>
        /// Abre la siguiente vuelta el JUGADOR que jugó la carta más alta del equipo ganador.
        /// Si fue parda, abre el jugador mano original.
        /// </summary>
        public static string ObtenerAbreSiguienteVuelta(
            ManoTruco3v3 mano, Vuelta3v3 vuelta, string? ganadorVuelta)
        {
            if (ganadorVuelta is null or "Parda")
                return mano.JugadorMano;

            var equipoGanador = mano.ObtenerEquipo(ganadorVuelta);
            string? mejor = null;
            int max = -1;
            foreach (var j in equipoGanador.Jugadores)
            {
                if (vuelta.CartasJugadas.TryGetValue(j.Id, out var c) && c.ValorTruco > max)
                {
                    max = c.ValorTruco;
                    mejor = j.Id;
                }
            }
            return mejor ?? mano.JugadorMano;
        }

        /// <summary>
        /// El primero del equipo ganador en el orden de turno original abre la siguiente vuelta.
        /// Si fue parda, abre el jugador mano original.
        /// </summary>
        public static string ObtenerPrimeroDeVueltaSiguiente(ManoTruco3v3 mano, string? ganadorVuelta)
        {
            if (ganadorVuelta is null or "Parda")
                return mano.JugadorMano;

            var orden = ObtenerOrdenTurno(mano);
            var equipoGanador = mano.ObtenerEquipo(ganadorVuelta);
            return orden.First(id => equipoGanador.ContieneJugador(id));
        }

        /// <summary>
        /// Último jugador del equipo dado en el orden de turno de la vuelta.
        /// Este jugador puede escalar (retruco / vale cuatro).
        /// </summary>
        public static string ObtenerUltimoDelEquipoEnTurno(ManoTruco3v3 mano, string equipoId)
        {
            var orden = ObtenerOrdenTurno(mano);
            var equipo = mano.ObtenerEquipo(equipoId);
            return orden.Last(id => equipo.ContieneJugador(id));
        }

        /// <summary>
        /// Solo el último jugador del equipo contrario al cantor puede escalar el truco.
        /// </summary>
        public static bool PuedeEscalarTruco(ManoTruco3v3 mano, string jugadorId)
        {
            if (!mano.TrucoCantado || mano.EquipoCantorTruco == null) return false;

            var equipoContrario = mano.ObtenerEquipoContrario(mano.EquipoCantorTruco);
            if (!equipoContrario.ContieneJugador(jugadorId)) return false;

            var ultimoDelEquipo = ObtenerUltimoDelEquipoEnTurno(mano, equipoContrario.Id);
            return ultimoDelEquipo == jugadorId;
        }

        /// <summary>Cualquier jugador puede cantar truco si no se cantó antes.</summary>
        public static bool PuedeCantarTruco(ManoTruco3v3 mano, string jugadorId) =>
            !mano.TrucoCantado && !mano.TrucoResuelto && mano.GanadorMano == null && !mano.PartidaTerminada;

        /// <summary>
        /// Responsable de responder un canto (envido/truco) en MULTIJUGADOR (6 jugadores reales):
        /// el rival que sigue en la ronda DESPUÉS del cantor.
        /// </summary>
        public static string ObtenerResponsableCanto(ManoTruco3v3 mano, string cantorId)
        {
            string equipoCantor = mano.ObtenerEquipoDeJugador(cantorId);
            var equipoContrario = mano.ObtenerEquipoContrario(equipoCantor);
            var orden = ObtenerOrdenTurno(mano);
            int idx = orden.IndexOf(cantorId);
            if (idx >= 0)
            {
                for (int i = 1; i <= orden.Count; i++)
                {
                    var id = orden[(idx + i) % orden.Count];
                    if (equipoContrario.ContieneJugador(id)) return id;
                }
            }
            return orden.First(id => equipoContrario.ContieneJugador(id));
        }

        /// <summary>
        /// Atajo para los callers que tienen un jugadorId (no equipoId): convierte y delega.
        /// Útil tanto en el dominio (MaquinaServicio3v3) como en la API para acciones del humano.
        /// </summary>
        public static string ObtenerResponsableParaJugador(ManoTruco3v3 mano, string jugadorId) =>
            ObtenerResponsableTruco(mano, mano.ObtenerEquipoDeJugador(jugadorId));

        /// <summary>
        /// Responsable de responder un canto en modo SOLO (vs bots): el humano (J1) decide
        /// quiero/no quiero por todo su equipo. Si el equipo contrario al cantor no tiene a J1,
        /// responde el primer rival que sigue en la ronda. Espejo del atajo J1 del 2v2.
        /// </summary>
        public static string ObtenerResponsableTruco(ManoTruco3v3 mano, string equipoCantorId)
        {
            var equipoContrario = mano.ObtenerEquipoContrario(equipoCantorId);
            // J1 responde por su equipo SOLO si está efectivamente en la mano activa.
            if (equipoContrario.ContieneJugador("J1") && mano.JugadoresActivos.Contains("J1"))
                return "J1";

            var orden = ObtenerOrdenTurno(mano);
            int idxJ1 = orden.IndexOf("J1");
            if (idxJ1 >= 0)
            {
                for (int i = 1; i <= orden.Count; i++)
                {
                    var id = orden[(idxJ1 + i) % orden.Count];
                    if (equipoContrario.ContieneJugador(id)) return id;
                }
            }
            return orden.First(id => equipoContrario.ContieneJugador(id));
        }

        /// <summary>
        /// Orden de declaración de tantos de envido: arranca por el jugador MANO
        /// y sigue el orden de la mesa.
        /// </summary>
        public static List<string> ObtenerOrdenDeclaracionEnvido(ManoTruco3v3 mano) =>
            ObtenerOrdenTurno(mano);
    }
}
