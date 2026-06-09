using System.Linq;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Lógica de envido para el modo 2v2.
    /// El tanto del equipo = max(tanto_j1, tanto_j2) del equipo.
    /// Declaración en orden (el equipo mano declara último).
    /// "Son buenas" → el declarante reconoce que pierde.
    /// Empate → gana el equipo mano.
    /// </summary>
    public static class EnvidoServicio2v2
    {
        /// <summary>Calcula el tanto del equipo como el máximo entre sus dos jugadores.</summary>
        public static int CalcularTantoEquipo(Equipo2v2 equipo)
        {
            int t1 = TantoOriginal(equipo.Jugador1);
            int t2 = TantoOriginal(equipo.Jugador2);
            return Math.Max(t1, t2);
        }

        /// <summary>Tanto calculado con las 3 cartas originales (mano + ya jugadas).</summary>
        public static int TantoOriginal(Jugador jugador) =>
            EnvidoServicio.CalcularTanto(jugador.Mano.Concat(jugador.Jugadas).ToList());

        /// <summary>Calcula los tantos de todos los jugadores de la mano.</summary>
        public static Dictionary<string, int> CalcularTodosLosTantos(ManoTruco2v2 mano)
        {
            var resultado = new Dictionary<string, int>();
            foreach (var jugador in mano.OrdenJugadores)
                resultado[jugador.Id] = TantoOriginal(jugador);
            return resultado;
        }

        /// <summary>
        /// Indica si el jugador puede cantar el envido: en la primera vuelta, antes de jugar
        /// su carta, y solo contra el primer truco del rival sin aceptar ("el envido va primero").
        /// </summary>
        public static bool PuedeCantarEnvido(ManoTruco2v2 mano, string jugadorId)
        {
            if (mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.PartidaTerminada || mano.GanadorMano != null) return false;
            if (mano.Vueltas.Count > 0) return false;
            if ((mano.ObtenerJugador(jugadorId)?.Jugadas.Count ?? 0) > 0) return false;
            if (mano.TrucoCantado &&
                !(mano.NivelTruco == 1 && mano.EquipoCantorTruco != mano.ObtenerEquipoDeJugador(jugadorId)))
                return false;
            return true;
        }

        /// <summary>Canta el envido (Envido / Real Envido / Falta Envido). Devuelve false si no corresponde.</summary>
        public static bool Cantar(ManoTruco2v2 mano, string jugadorId, string tipo,
                                  Func<ManoTruco2v2, string, string> responsable)
        {
            if (!PuedeCantarEnvido(mano, jugadorId)) return false;

            mano.EnvidoCantado        = true;
            mano.CantorEnvido         = jugadorId;
            mano.TipoEnvidoCantado    = EnvidoServicio.NormalizarTipo(tipo);
            mano.PuntosEnvido         = ObtenerPuntosEnJuego(mano.TipoEnvidoCantado);
            mano.PuntosEnvidoNoQuiero = 1; // rechazar el primer envido = 1 punto
            mano.FaseEnvido           = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = responsable(mano, jugadorId);
            mano.EstadoEnvido = $"{jugadorId} cantó {tipo}.";
            return true;
        }

        /// <summary>Escala el envido (Envido → Envido Envido → Real Envido → Falta Envido).</summary>
        public static bool Escalar(ManoTruco2v2 mano, string jugadorId, string tipo,
                                   Func<ManoTruco2v2, string, string> responsable)
        {
            if (!mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.FaseEnvido != "pendiente_respuesta") return false;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return false;

            string tipoNuevo = EnvidoServicio.NormalizarTipo(tipo);
            if (EnvidoServicio.OrdinalTipo(tipoNuevo) <= EnvidoServicio.OrdinalTipo(mano.TipoEnvidoCantado)) return false;

            int ptsAntes = mano.PuntosEnvido;
            mano.TipoEnvidoCantado    = tipoNuevo;
            mano.PuntosEnvido         = ObtenerPuntosEnJuego(tipoNuevo);
            mano.PuntosEnvidoNoQuiero = ptsAntes; // rechazar la escalada paga lo de la apuesta anterior
            mano.CantorEnvido         = jugadorId;
            mano.EnvidoPendienteRespuestaDe = responsable(mano, jugadorId);
            mano.EstadoEnvido = $"{jugadorId} cantó {tipo}.";
            return true;
        }

        /// <summary>
        /// Inicia la fase de declaración de tantos (después del "quiero").
        /// Precalcula los tantos reales y prepara el orden de declaración.
        /// </summary>
        public static void IniciarDeclaracionTantos(ManoTruco2v2 mano)
        {
            mano.TantosReales  = CalcularTodosLosTantos(mano);
            mano.TantosDeclarados = new Dictionary<string, int?>();
            foreach (var jugador in mano.OrdenJugadores)
                mano.TantosDeclarados[jugador.Id] = null;

            mano.FaseEnvido = "declarando_tantos";
            mano.IndiceDeclaracionTanto = 0;

            var orden = TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano);
            mano.EnvidoPendienteRespuestaDe = orden[0];
        }

        /// <summary>
        /// Procesa la declaración de tanto de un jugador (o "son buenas").
        /// sonBuenas = true → el jugador reconoce que su equipo pierde.
        /// </summary>
        public static bool ProcesarDeclaracion(
            ManoTruco2v2 mano,
            string jugadorId,
            int? tanto,
            bool sonBuenas)
        {
            if (mano.FaseEnvido != "declarando_tantos") return false;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return false;

            var orden = TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano);

            if (sonBuenas)
            {
                // "Son buenas" = este jugador no muestra tanto (no puede superar lo cantado).
                // No resuelve por sí solo: su compañero todavía puede ganar el tanto.
                mano.SonBuenasDeclarado = true;
                mano.JugadorQueDijoSonBuenas = jugadorId;
                mano.TantosDeclarados[jugadorId] = null;
            }
            else
            {
                mano.TantosDeclarados[jugadorId] = tanto;
            }

            mano.IndiceDeclaracionTanto++;

            // El conteo arranca por el mano y se saltea a los jugadores cuyo equipo YA va
            // ganando: no necesitan cantar, solo cantan los que podrían dar vuelta el tanto.
            AvanzarHastaProximoDeclarante(mano, orden);

            if (mano.IndiceDeclaracionTanto >= orden.Count)
            {
                ResolverPorDeclarados(mano, orden);
                return true;
            }

            mano.EnvidoPendienteRespuestaDe = orden[mano.IndiceDeclaracionTanto];
            return false;
        }

        /// <summary>Mejor tanto YA DECLARADO entre los jugadores de un equipo (-1 si ninguno declaró aún).</summary>
        private static int MejorTantoDeclaradoDeEquipo(ManoTruco2v2 mano, string equipoId)
        {
            int mejor = -1;
            foreach (var jugador in mano.ObtenerEquipo(equipoId).Jugadores)
            {
                if (mano.TantosDeclarados.TryGetValue(jugador.Id, out var t) && t.HasValue && t.Value > mejor)
                    mejor = t.Value;
            }
            return mejor;
        }

        /// <summary>
        /// Equipo que va ganando según los tantos YA declarados hasta cierto índice.
        /// El equipo mano gana los empates. Devuelve null si nadie declaró todavía.
        /// </summary>
        private static string? EquipoLiderDeclaracion(ManoTruco2v2 mano, List<string> orden, int hastaIndice)
        {
            int mejor = -1;
            string? lider = null;
            for (int i = 0; i < hastaIndice && i < orden.Count; i++)
            {
                if (!mano.TantosDeclarados.TryGetValue(orden[i], out var t) || !t.HasValue) continue;
                string equipo = mano.ObtenerEquipoDeJugador(orden[i]);
                if (t.Value > mejor)
                {
                    mejor = t.Value;
                    lider = equipo;
                }
                else if (t.Value == mejor && equipo == mano.EquipoMano)
                {
                    lider = equipo; // empate → gana el mano
                }
            }
            return lider;
        }

        /// <summary>
        /// Avanza el índice de declaración salteando a los jugadores cuyo equipo ya va
        /// ganando (no necesitan cantar). Se detiene en el próximo que podría dar vuelta
        /// el tanto, o al final si ya no queda nadie que pueda.
        /// </summary>
        private static void AvanzarHastaProximoDeclarante(ManoTruco2v2 mano, List<string> orden)
        {
            while (mano.IndiceDeclaracionTanto < orden.Count)
            {
                string siguiente = orden[mano.IndiceDeclaracionTanto];
                string? lider = EquipoLiderDeclaracion(mano, orden, mano.IndiceDeclaracionTanto);
                if (lider != null && mano.ObtenerEquipoDeJugador(siguiente) == lider)
                {
                    mano.IndiceDeclaracionTanto++; // su equipo ya gana → no necesita cantar
                    continue;
                }
                break;
            }
        }

        /// <summary>Resuelve el envido según los tantos declarados (gana el mejor; empate → mano).</summary>
        private static void ResolverPorDeclarados(ManoTruco2v2 mano, List<string> orden)
        {
            string ganador = EquipoLiderDeclaracion(mano, orden, orden.Count) ?? mano.EquipoMano;
            int decA = MejorTantoDeclaradoDeEquipo(mano, "EquipoA");
            int decB = MejorTantoDeclaradoDeEquipo(mano, "EquipoB");
            FinalizarEnvido(mano, ganador, $"EquipoA: {decA} vs EquipoB: {decB}. Ganador: {ganador}");
        }

        private static void FinalizarEnvido(ManoTruco2v2 mano, string equipoGanador, string descripcion)
        {
            mano.GanadorEnvido          = equipoGanador;
            mano.EnvidoResuelto         = true;
            mano.FaseEnvido             = "resuelto";
            mano.EnvidoPendienteRespuestaDe = null;

            int puntosEnJuego = mano.PuntosEnvido;
            mano.EstadoEnvido = descripcion + $". Vale {puntosEnJuego} pt.";

            JuegoServicio2v2.SumarPuntos(mano, equipoGanador, puntosEnJuego);
        }

        /// <summary>
        /// Resuelve el envido cuando el rival no quiso (no quiero).
        /// Gana el equipo cantor con 1 punto.
        /// </summary>
        public static void ResolverNoQuiero(ManoTruco2v2 mano)
        {
            if (mano.CantorEnvido == null) return;
            string equipoCantor = mano.ObtenerEquipoDeJugador(mano.CantorEnvido);
            int pts = Math.Max(1, mano.PuntosEnvidoNoQuiero); // Envido→1, Envido Envido→2, etc.
            mano.GanadorEnvido          = equipoCantor;
            mano.PuntosEnvido           = pts;
            mano.EnvidoResuelto         = true;
            mano.FaseEnvido             = "resuelto";
            mano.EnvidoPendienteRespuestaDe = null;
            mano.EstadoEnvido           = $"No quiso. {equipoCantor} gana {pts} punto(s).";
            JuegoServicio2v2.SumarPuntos(mano, equipoCantor, pts);
        }

        /// <summary>Devuelve los puntos en juego según el tipo de envido cantado.</summary>
        public static int ObtenerPuntosEnJuego(string? tipo) =>
            EnvidoServicio.ObtenerPuntosSegunTipo(tipo);
    }
}
