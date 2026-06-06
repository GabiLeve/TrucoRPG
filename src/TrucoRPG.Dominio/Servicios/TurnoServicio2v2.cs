using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Maneja el orden de turnos, quién puede escalar el truco y quién puede cantar/responder
    /// en un juego 2v2.
    /// Regla de equipos: EquipoA = posiciones 1 y 3; EquipoB = posiciones 2 y 4.
    /// Orden de mesa (horario): Pos1 → Pos2 → Pos3 → Pos4.
    /// </summary>
    public static class TurnoServicio2v2
    {
        /// <summary>
        /// Devuelve el orden de turno completo de la vuelta actual empezando por el jugador mano.
        /// El jugador mano es quien abre la vuelta.
        /// </summary>
        public static List<string> ObtenerOrdenTurno(ManoTruco2v2 mano)
        {
            var todos = mano.OrdenJugadores.Select(j => j.Id).ToList();
            int indexMano = todos.IndexOf(mano.JugadorMano);
            if (indexMano < 0) return todos;

            var ordenado = new List<string>();
            for (int i = 0; i < todos.Count; i++)
                ordenado.Add(todos[(indexMano + i) % todos.Count]);
            return ordenado;
        }

        /// <summary>
        /// Calcula el siguiente jugador en el orden de turno después de jugadorActualId.
        /// </summary>
        public static string? SiguienteJugador(ManoTruco2v2 mano, string jugadorActualId)
        {
            var orden = ObtenerOrdenTurno(mano);
            int idx = orden.IndexOf(jugadorActualId);
            if (idx < 0) return null;
            // Circular: si una vuelta la abre un jugador que no es el "mano", el orden
            // debe envolver hasta completar los 4 jugadores (no cortar en el último).
            return orden[(idx + 1) % orden.Count];
        }

        /// <summary>
        /// Abre la siguiente vuelta el JUGADOR que ganó la vuelta (el que jugó la carta más
        /// alta del equipo ganador). Si fue parda, abre el jugador mano original.
        /// </summary>
        public static string ObtenerAbreSiguienteVuelta(
            ManoTruco2v2 mano, Vuelta2v2 vuelta, string? ganadorVuelta)
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
        /// Tras ganar una vuelta, el ganador de esa vuelta es quien abre la siguiente.
        /// Si fue parda, el jugador mano original abre la siguiente.
        /// </summary>
        public static string ObtenerPrimeroDeVueltaSiguiente(
            ManoTruco2v2 mano,
            string? ganadorVuelta)
        {
            if (ganadorVuelta is null or "Parda")
                return mano.JugadorMano;

            // El primero del equipo ganador en el orden de turno original
            var orden = ObtenerOrdenTurno(mano);
            var equipoGanador = mano.ObtenerEquipo(ganadorVuelta);
            return orden.First(id => equipoGanador.ContieneJugador(id));
        }

        /// <summary>
        /// Devuelve el último jugador del equipo dado en el orden de turno de la vuelta.
        /// El "último" es el que tiene la posición más alta en el orden de mesa.
        /// Este jugador puede escalar (retruco / vale cuatro).
        /// </summary>
        public static string ObtenerUltimoDelEquipoEnTurno(ManoTruco2v2 mano, string equipoId)
        {
            var orden = ObtenerOrdenTurno(mano);
            var equipo = mano.ObtenerEquipo(equipoId);
            // El último del equipo = el que aparece más tarde en el orden
            return orden.Last(id => equipo.ContieneJugador(id));
        }

        /// <summary>
        /// Devuelve si un jugador puede escalar el truco (cantar retruco/vale cuatro).
        /// Solo el último jugador del equipo contrario al cantor puede escalar.
        /// </summary>
        public static bool PuedeEscalarTruco(ManoTruco2v2 mano, string jugadorId)
        {
            if (!mano.TrucoCantado || mano.EquipoCantorTruco == null) return false;

            var equipoContrario = mano.ObtenerEquipoContrario(mano.EquipoCantorTruco);
            if (!equipoContrario.ContieneJugador(jugadorId)) return false;

            var ultimoDelEquipo = ObtenerUltimoDelEquipoEnTurno(mano, equipoContrario.Id);
            return ultimoDelEquipo == jugadorId;
        }

        /// <summary>
        /// Devuelve si un jugador puede cantar truco (nivel 1).
        /// Cualquier jugador puede cantar truco si no se cantó antes.
        /// </summary>
        public static bool PuedeCantarTruco(ManoTruco2v2 mano, string jugadorId) =>
            !mano.TrucoCantado && !mano.TrucoResuelto && mano.GanadorMano == null && !mano.PartidaTerminada;

        /// <summary>
        /// Devuelve quién del equipo contrario debe responder el truco cantado.
        /// Regla: es el primer jugador del equipo contrario en el orden de turno.
        /// </summary>
        public static string ObtenerResponsableTruco(ManoTruco2v2 mano, string equipoCantorId)
        {
            var equipoContrario = mano.ObtenerEquipoContrario(equipoCantorId);
            // El humano (J1) siempre decide quiero/no quiero por su equipo (no su compañero).
            if (equipoContrario.ContieneJugador("J1")) return "J1";
            var orden = ObtenerOrdenTurno(mano);
            return orden.First(id => equipoContrario.ContieneJugador(id));
        }

        /// <summary>
        /// Devuelve el orden de declaración de tantos de envido.
        /// El equipo mano declara ÚLTIMO (sus jugadores van al final).
        /// Dentro de cada equipo se mantiene el orden de mesa.
        /// </summary>
        public static List<string> ObtenerOrdenDeclaracionEnvido(ManoTruco2v2 mano)
        {
            var orden = ObtenerOrdenTurno(mano);
            var equipoMano = mano.ObtenerEquipo(mano.EquipoMano);
            var equipoRival = mano.ObtenerEquipoContrario(mano.EquipoMano);

            var rivalPrimero = orden.Where(id => equipoRival.ContieneJugador(id)).ToList();
            var manoUltimo   = orden.Where(id => equipoMano.ContieneJugador(id)).ToList();

            return rivalPrimero.Concat(manoUltimo).ToList();
        }
    }
}
