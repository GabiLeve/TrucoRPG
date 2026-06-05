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
            int t1 = EnvidoServicio.CalcularTanto(equipo.Jugador1.Mano);
            int t2 = EnvidoServicio.CalcularTanto(equipo.Jugador2.Mano);
            return Math.Max(t1, t2);
        }

        /// <summary>Calcula los tantos de todos los jugadores de la mano.</summary>
        public static Dictionary<string, int> CalcularTodosLosTantos(ManoTruco2v2 mano)
        {
            var resultado = new Dictionary<string, int>();
            foreach (var jugador in mano.OrdenJugadores)
                resultado[jugador.Id] = EnvidoServicio.CalcularTanto(jugador.Mano);
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
                // El equipo del que declaró "son buenas" pierde
                mano.SonBuenasDeclarado  = true;
                mano.JugadorQueDijoSonBuenas = jugadorId;
                string equipoPierde = mano.ObtenerEquipoDeJugador(jugadorId);
                string equipoGana   = equipoPierde == "EquipoA" ? "EquipoB" : "EquipoA";

                FinalizarEnvido(mano, equipoGana, "Son buenas declarado por " + jugadorId);
                return true;
            }

            // Registrar tanto declarado
            mano.TantosDeclarados[jugadorId] = tanto;
            mano.IndiceDeclaracionTanto++;

            if (mano.IndiceDeclaracionTanto >= orden.Count)
            {
                // Todos declararon → resolver
                ResolverPorTantos(mano, orden);
                return true;
            }

            mano.EnvidoPendienteRespuestaDe = orden[mano.IndiceDeclaracionTanto];
            return false;
        }

        private static void ResolverPorTantos(ManoTruco2v2 mano, List<string> orden)
        {
            int tantoA = CalcularTantoEquipo(mano.EquipoA);
            int tantoB = CalcularTantoEquipo(mano.EquipoB);

            string ganador;
            if (tantoA > tantoB)      ganador = "EquipoA";
            else if (tantoB > tantoA) ganador = "EquipoB";
            else                      ganador = mano.EquipoMano; // empate → gana el mano

            FinalizarEnvido(mano, ganador,
                $"EquipoA: {tantoA} vs EquipoB: {tantoB}. Ganador: {ganador}");
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
