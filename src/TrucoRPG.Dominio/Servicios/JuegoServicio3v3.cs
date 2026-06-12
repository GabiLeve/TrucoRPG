using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Lógica central del juego 3v3: resolver vueltas, resolver mano (3 vueltas de equipos),
    /// puntaje y fin de partida. Espejo de <see cref="JuegoServicio2v2"/> con 6 jugadores.
    /// </summary>
    public static class JuegoServicio3v3
    {
        /// <summary>
        /// Determina qué equipo gana la vuelta (la mejor carta individual representa al equipo).
        /// Si mejor(A) == mejor(B) → "Parda".
        /// </summary>
        public static string ResolverVuelta(Vuelta3v3 vuelta, Equipo3v3 equipoA, Equipo3v3 equipoB)
        {
            int mejorA = ObtenerMejorValorEquipo(vuelta, equipoA, out var cartaA);
            int mejorB = ObtenerMejorValorEquipo(vuelta, equipoB, out var cartaB);

            vuelta.MejorCartaEquipoA = cartaA;
            vuelta.MejorCartaEquipoB = cartaB;

            if (mejorA > mejorB) return "EquipoA";
            if (mejorB > mejorA) return "EquipoB";
            return "Parda";
        }

        private static int ObtenerMejorValorEquipo(Vuelta3v3 vuelta, Equipo3v3 equipo, out Carta? mejorCarta)
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
        /// Devuelve null si la mano aún no puede resolverse.
        /// </summary>
        public static string? ResolverGanadorMano(List<string> ganadoresVueltas, string equipoMano)
        {
            if (ganadoresVueltas.Count == 0) return null;

            string? g1 = ganadoresVueltas.Count > 0 ? ganadoresVueltas[0] : null;
            string? g2 = ganadoresVueltas.Count > 1 ? ganadoresVueltas[1] : null;
            string? g3 = ganadoresVueltas.Count > 2 ? ganadoresVueltas[2] : null;

            if (g1 is "EquipoA" or "EquipoB")
            {
                if (g2 == g1)      return g1;
                if (g2 == "Parda") return g1;
                if (g2 != null && g2 != g1 && g2 != "Parda")
                {
                    if (g3 == null)    return null;
                    if (g3 == "Parda") return g1;
                    return g3;
                }
            }

            if (g1 == "Parda")
            {
                if (g2 is "EquipoA" or "EquipoB")
                    return g2;

                if (g2 == "Parda")
                {
                    if (g3 == null)    return null;
                    if (g3 == "Parda") return equipoMano;
                    return g3;
                }
            }

            return null;
        }

        /// <summary>
        /// Valida que el jugador indicado puede realizar una acción en este momento.
        /// Lanza <see cref="InvalidOperationException"/> si la validación falla.
        /// </summary>
        public static void ValidarAccionJugador(ManoTruco3v3 mano, string jugadorId)
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

        /// <summary>Suma puntos al equipo ganador. Si algún equipo llega a 30, termina la partida.</summary>
        public static void SumarPuntos(ManoTruco3v3 mano, string equipoGanador, int puntos)
        {
            if (puntos <= 0) return;

            if (equipoGanador == "EquipoA")
                mano.PuntosEquipoA += puntos;
            else if (equipoGanador == "EquipoB")
                mano.PuntosEquipoB += puntos;

            EvaluarFinPartida(mano);
        }

        /// <summary>El 3v3 (con su fase Pica-Pica) se juega a 30 puntos.</summary>
        public const int PuntajeObjetivo = 30;

        private static void EvaluarFinPartida(ManoTruco3v3 mano)
        {
            if (mano.PuntosEquipoA >= PuntajeObjetivo)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida   = "EquipoA";
            }
            else if (mano.PuntosEquipoB >= PuntajeObjetivo)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida   = "EquipoB";
            }
        }

        /// <summary>
        /// Variante de <see cref="JugarCarta(ManoTruco3v3, string, Carta)"/> que busca la
        /// carta por número y palo. Devuelve false si la jugada no es válida ahora
        /// (mano terminada, canto pendiente, no es su turno o no tiene esa carta).
        /// </summary>
        public static bool JugarCartaPorValor(ManoTruco3v3 mano, string jugadorId, int numero, string palo)
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
        public static bool JugarCarta(ManoTruco3v3 mano, string jugadorId, Carta carta)
        {
            if (mano.GanadorMano != null || mano.PartidaTerminada) return true;
            if (mano.TrucoPendienteRespuestaDe != null || mano.EnvidoPendienteRespuestaDe != null) return false;
            if (mano.TurnoActual != jugadorId) return false;

            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null) return false;
            if (!jugador.Mano.Contains(carta)) return false;

            mano.VueltaActual ??= new Vuelta3v3();

            mano.VueltaActual.CartasJugadas[jugadorId] = carta;
            jugador.Mano.Remove(carta);
            jugador.Jugadas.Add(carta);

            var ordenTurno = TurnoServicio3v3.ObtenerOrdenTurno(mano);

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
                    mano.GanadorMano   = ganadorMano;
                    mano.ManoTerminada = true;
                    int pts = mano.PuntosTrucoMano > 0 ? mano.PuntosTrucoMano : 1;
                    if (!mano.TrucoCantado)
                        mano.EstadoTruco = "No se cantó truco. La mano vale 1 punto.";
                    SumarPuntos(mano, ganadorMano, pts);
                    mano.TrucoResuelto = true;
                    return true;
                }

                var vueltaResuelta = mano.Vueltas[mano.Vueltas.Count - 1];
                mano.TurnoActual = TurnoServicio3v3.ObtenerAbreSiguienteVuelta(mano, vueltaResuelta, ganadorVuelta);
            }
            else
            {
                var siguiente = TurnoServicio3v3.SiguienteJugador(mano, jugadorId);
                if (siguiente != null)
                    mano.TurnoActual = siguiente;
            }

            return false;
        }
    }
}
