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
            mano.GanadorEnvido          = equipoCantor;
            mano.PuntosEnvido           = 1;
            mano.EnvidoResuelto         = true;
            mano.FaseEnvido             = "resuelto";
            mano.EnvidoPendienteRespuestaDe = null;
            mano.EstadoEnvido           = $"No quiso. {equipoCantor} gana 1 punto.";
            JuegoServicio2v2.SumarPuntos(mano, equipoCantor, 1);
        }

        /// <summary>Devuelve los puntos en juego según el tipo de envido cantado.</summary>
        public static int ObtenerPuntosEnJuego(string? tipo) =>
            EnvidoServicio.ObtenerPuntosSegunTipo(tipo);
    }
}
