using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Lógica central del juego 2v2: resolver vueltas, resolver mano (3 vueltas de equipos),
    /// puntaje y fin de partida.
    /// </summary>
    public static class JuegoServicio2v2
    {
        /// <summary>
        /// Dado que todos los jugadores jugaron su carta en la vuelta,
        /// determina qué equipo gana la vuelta.
        /// La mejor carta individual de cada equipo representa al equipo.
        /// Si mejor(A) == mejor(B) → "Parda".
        /// </summary>
        public static string ResolverVuelta(
            Vuelta2v2 vuelta,
            Equipo2v2 equipoA,
            Equipo2v2 equipoB)
        {
            int mejorA = ObtenerMejorValorEquipo(vuelta, equipoA, out var cartaA);
            int mejorB = ObtenerMejorValorEquipo(vuelta, equipoB, out var cartaB);

            vuelta.MejorCartaEquipoA = cartaA;
            vuelta.MejorCartaEquipoB = cartaB;

            if (mejorA > mejorB) return "EquipoA";
            if (mejorB > mejorA) return "EquipoB";
            return "Parda";
        }

        private static int ObtenerMejorValorEquipo(
            Vuelta2v2 vuelta,
            Equipo2v2 equipo,
            out Carta? mejorCarta)
        {
            mejorCarta = null;
            int mejor = -1;
            foreach (var jugador in equipo.Jugadores)
            {
                if (vuelta.CartasJugadas.TryGetValue(jugador.Id, out var carta))
                {
                    if (carta.ValorTruco > mejor)
                    {
                        mejor = carta.ValorTruco;
                        mejorCarta = carta;
                    }
                }
            }
            return mejor;
        }

        /// <summary>
        /// Resuelve el ganador de la mano completa (hasta 3 vueltas de equipos).
        /// Algoritmo idéntico al 1v1 pero con "EquipoA"/"EquipoB" en lugar de "Humano"/"Maquina".
        /// Devuelve null si la mano aún no puede resolverse.
        /// </summary>
        public static string? ResolverGanadorMano(
            List<string> ganadoresVueltas,
            string equipoMano)
        {
            if (ganadoresVueltas.Count == 0) return null;

            string? g1 = ganadoresVueltas.Count > 0 ? ganadoresVueltas[0] : null;
            string? g2 = ganadoresVueltas.Count > 1 ? ganadoresVueltas[1] : null;
            string? g3 = ganadoresVueltas.Count > 2 ? ganadoresVueltas[2] : null;

            // Primera vuelta ganada por un equipo
            if (g1 is "EquipoA" or "EquipoB")
            {
                if (g2 == g1)   return g1;   // Ganó las dos primeras
                if (g2 == "Parda") return g1; // Ganó 1ra, parda en 2da → gana 1ra
                if (g2 != null && g2 != g1 && g2 != "Parda")
                {
                    // Uno y uno → va a tercera
                    if (g3 == null)    return null;
                    if (g3 == "Parda") return g1; // Tercera parda → gana quien ganó primera
                    return g3;
                }
            }

            // Primera vuelta parda
            if (g1 == "Parda")
            {
                if (g2 is "EquipoA" or "EquipoB")
                    return g2; // Parda 1ra → decide la 2da

                if (g2 == "Parda")
                {
                    // Parda + Parda → va a tercera
                    if (g3 == null)    return null;
                    if (g3 == "Parda") return equipoMano; // Todas pardas → gana el mano
                    return g3;
                }
            }

            return null;
        }

        /// <summary>
        /// Suma puntos al equipo ganador. Si algún equipo llega a 30, termina la partida.
        /// </summary>
        public static void SumarPuntos(ManoTruco2v2 mano, string equipoGanador, int puntos)
        {
            if (puntos <= 0) return;

            if (equipoGanador == "EquipoA")
                mano.PuntosEquipoA += puntos;
            else if (equipoGanador == "EquipoB")
                mano.PuntosEquipoB += puntos;

            EvaluarFinPartida(mano);
        }

        private static void EvaluarFinPartida(ManoTruco2v2 mano)
        {
            if (mano.PuntosEquipoA >= 30)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida   = "EquipoA";
            }
            else if (mano.PuntosEquipoB >= 30)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida   = "EquipoB";
            }
        }

        /// <summary>
        /// Valida que el jugador indicado puede realizar una acción en este momento.
        /// Lanza <see cref="InvalidOperationException"/> si no (espejo del 3v3;
        /// antes vivía en Truco2v2Controller).
        /// </summary>
        public static void ValidarAccionJugador(ManoTruco2v2 mano, string jugadorId)
        {
            if (mano.PartidaTerminada || mano.ManoTerminada)
                throw new InvalidOperationException("La mano/partida ya terminó.");
            if (mano.TrucoPendienteRespuestaDe == jugadorId)
                throw new InvalidOperationException("Debés responder el truco antes de jugar.");
            if (mano.EnvidoPendienteRespuestaDe == jugadorId)
                throw new InvalidOperationException("Debés responder el envido antes de jugar.");
            if (mano.TurnoActual != jugadorId)
                throw new InvalidOperationException("No es tu turno.");
        }

        /// <summary>
        /// Variante de <see cref="JugarCarta(ManoTruco2v2, string, Carta)"/> que busca la
        /// carta por número y palo. Devuelve false si la jugada no es válida ahora
        /// (mano terminada, canto pendiente, no es su turno o no tiene esa carta).
        /// </summary>
        public static bool JugarCartaPorValor(ManoTruco2v2 mano, string jugadorId, int numero, string palo)
        {
            if (mano.GanadorMano != null || mano.ManoTerminada || mano.PartidaTerminada) return false;
            if (mano.TrucoPendienteRespuestaDe != null || mano.EnvidoPendienteRespuestaDe != null) return false;
            if (mano.TurnoActual != jugadorId) return false;

            var carta = mano.ObtenerJugador(jugadorId)?.Mano.FirstOrDefault(c =>
                c.Numero == numero && c.Palo.Equals(palo, StringComparison.OrdinalIgnoreCase));
            if (carta == null) return false;
            JugarCarta(mano, jugadorId, carta);
            return true;
        }

        /// <summary>
        /// Procesa que un jugador juegue su carta en la vuelta actual.
        /// Si la vuelta se completa, la resuelve y verifica si la mano terminó.
        /// Devuelve true si la mano terminó.
        /// </summary>
        public static bool JugarCarta(ManoTruco2v2 mano, string jugadorId, Carta carta)
        {
            // Verificaciones básicas
            if (mano.GanadorMano != null || mano.PartidaTerminada) return true;
            if (mano.TrucoPendienteRespuestaDe != null || mano.EnvidoPendienteRespuestaDe != null) return false;
            if (mano.TurnoActual != jugadorId) return false;

            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null) return false;
            if (!jugador.Mano.Contains(carta)) return false;

            // Iniciar nueva vuelta si es necesario
            if (mano.VueltaActual == null)
                mano.VueltaActual = new Vuelta2v2();

            mano.VueltaActual.CartasJugadas[jugadorId] = carta;
            jugador.Mano.Remove(carta);
            jugador.Jugadas.Add(carta);

            var ordenTurno = TurnoServicio2v2.ObtenerOrdenTurno(mano);

            // Verificar si la vuelta está completa
            if (mano.VueltaActual.EstaCompleta(ordenTurno.Select(id => id)))
            {
                var ganadorVuelta = ResolverVuelta(mano.VueltaActual, mano.EquipoA, mano.EquipoB);
                mano.VueltaActual.GanadorVuelta = ganadorVuelta;
                mano.Vueltas.Add(mano.VueltaActual);
                mano.VueltaActual = null;

                var ganadoresVueltas = mano.Vueltas.Select(v => v.GanadorVuelta!).ToList();
                var ganadorMano = ResolverGanadorMano(ganadoresVueltas, mano.EquipoMano);

                if (ganadorMano != null)
                {
                    mano.GanadorMano = ganadorMano;
                    mano.ManoTerminada = true;
                    int pts = mano.PuntosTrucoMano > 0 ? mano.PuntosTrucoMano : 1;
                    if (!mano.TrucoCantado)
                        mano.EstadoTruco = "No se cantó truco. La mano vale 1 punto.";
                    SumarPuntos(mano, ganadorMano, pts);
                    mano.TrucoResuelto = true;
                    return true;
                }

                // La vuelta terminó pero la mano no → abre la siguiente quien GANÓ la vuelta
                var vueltaResuelta = mano.Vueltas[mano.Vueltas.Count - 1];
                mano.TurnoActual = TurnoServicio2v2.ObtenerAbreSiguienteVuelta(mano, vueltaResuelta, ganadorVuelta);
            }
            else
            {
                // Vuelta en progreso → siguiente jugador en orden CIRCULAR de mesa.
                // No usamos SiguienteJugador (que parte de JugadorMano) porque cuando
                // una máquina gana la vuelta y la abre desde una posición distinta al
                // JugadorMano, el último en el orden fijo puede no ser el último en
                // jugar, y SiguienteJugador devuelve null antes de tiempo (bug: doble carta).
                var todos = mano.OrdenJugadores.Select(j => j.Id).ToList();
                int idx   = todos.IndexOf(jugadorId);
                for (int i = 1; i < todos.Count; i++)
                {
                    var cand = todos[(idx + i) % todos.Count];
                    if (!mano.VueltaActual!.CartasJugadas.ContainsKey(cand))
                    {
                        mano.TurnoActual = cand;
                        break;
                    }
                }
            }

            return false;
        }
    }
}
