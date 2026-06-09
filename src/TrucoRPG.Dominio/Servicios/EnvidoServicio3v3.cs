using System.Linq;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Lógica de envido para el modo 3v3.
    /// El tanto del equipo = máximo entre sus tres jugadores.
    /// Declaración en orden (el equipo mano declara último y gana los empates).
    /// "Son buenas" → el declarante reconoce que no puede superar lo cantado.
    /// Espejo de <see cref="EnvidoServicio2v2"/>.
    /// </summary>
    public static class EnvidoServicio3v3
    {
        /// <summary>Tanto del equipo = máximo entre sus jugadores.</summary>
        public static int CalcularTantoEquipo(Equipo3v3 equipo) =>
            equipo.Jugadores.Select(TantoOriginal).DefaultIfEmpty(0).Max();

        /// <summary>Tanto calculado con las 3 cartas originales (mano + ya jugadas).</summary>
        public static int TantoOriginal(Jugador jugador) =>
            EnvidoServicio.CalcularTanto(jugador.Mano.Concat(jugador.Jugadas).ToList());

        /// <summary>Calcula los tantos de todos los jugadores de la mano.</summary>
        public static Dictionary<string, int> CalcularTodosLosTantos(ManoTruco3v3 mano)
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
        public static bool PuedeCantarEnvido(ManoTruco3v3 mano, string jugadorId)
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
        public static bool Cantar(ManoTruco3v3 mano, string jugadorId, string tipo,
                                  Func<ManoTruco3v3, string, string> responsable)
        {
            if (!PuedeCantarEnvido(mano, jugadorId)) return false;

            mano.EnvidoCantado        = true;
            mano.CantorEnvido         = jugadorId;
            mano.TipoEnvidoCantado    = EnvidoServicio.NormalizarTipo(tipo);
            mano.PuntosEnvido         = ObtenerPuntosEnJuego(mano.TipoEnvidoCantado);
            mano.PuntosEnvidoNoQuiero = 1;
            mano.FaseEnvido           = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = responsable(mano, jugadorId);
            mano.EstadoEnvido = $"{jugadorId} cantó {tipo}.";
            return true;
        }

        /// <summary>Responde un envido pendiente: quiero (inicia la declaración de tantos) o no quiero.</summary>
        public static bool Responder(ManoTruco3v3 mano, string jugadorId, bool aceptar)
        {
            if (!mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.FaseEnvido != "pendiente_respuesta") return false;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return false;

            if (!aceptar)
            {
                ResolverNoQuiero(mano);
            }
            else
            {
                mano.EnvidoPendienteRespuestaDe = null;
                mano.FaseEnvido = "aceptado";
                IniciarDeclaracionTantos(mano);
            }
            return true;
        }

        /// <summary>Escala el envido (Envido → Envido Envido → Real Envido → Falta Envido).</summary>
        public static bool Escalar(ManoTruco3v3 mano, string jugadorId, string tipo,
                                   Func<ManoTruco3v3, string, string> responsable)
        {
            if (!mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.FaseEnvido != "pendiente_respuesta") return false;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return false;

            string tipoNuevo = EnvidoServicio.NormalizarTipo(tipo);
            if (EnvidoServicio.OrdinalTipo(tipoNuevo) <= EnvidoServicio.OrdinalTipo(mano.TipoEnvidoCantado)) return false;

            int ptsAntes = mano.PuntosEnvido;
            mano.TipoEnvidoCantado    = tipoNuevo;
            mano.PuntosEnvido         = ObtenerPuntosEnJuego(tipoNuevo);
            mano.PuntosEnvidoNoQuiero = ptsAntes;
            mano.CantorEnvido         = jugadorId;
            mano.EnvidoPendienteRespuestaDe = responsable(mano, jugadorId);
            mano.EstadoEnvido = $"{jugadorId} cantó {tipo}.";
            return true;
        }

        /// <summary>Inicia la fase de declaración de tantos (después del "quiero").</summary>
        public static void IniciarDeclaracionTantos(ManoTruco3v3 mano)
        {
            mano.TantosReales  = CalcularTodosLosTantos(mano);
            mano.TantosDeclarados = new Dictionary<string, int?>();
            foreach (var jugador in mano.OrdenJugadores)
                mano.TantosDeclarados[jugador.Id] = null;

            mano.FaseEnvido = "declarando_tantos";
            mano.IndiceDeclaracionTanto = 0;

            var orden = TurnoServicio3v3.ObtenerOrdenDeclaracionEnvido(mano);
            mano.EnvidoPendienteRespuestaDe = orden[0];
        }

        /// <summary>Procesa la declaración de tanto de un jugador (o "son buenas").</summary>
        public static bool ProcesarDeclaracion(ManoTruco3v3 mano, string jugadorId, int? tanto, bool sonBuenas)
        {
            if (mano.FaseEnvido != "declarando_tantos") return false;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return false;

            var orden = TurnoServicio3v3.ObtenerOrdenDeclaracionEnvido(mano);

            if (sonBuenas)
            {
                mano.SonBuenasDeclarado = true;
                mano.JugadorQueDijoSonBuenas = jugadorId;
                mano.TantosDeclarados[jugadorId] = null;
            }
            else
            {
                mano.TantosDeclarados[jugadorId] = tanto;
            }

            mano.IndiceDeclaracionTanto++;

            AvanzarHastaProximoDeclarante(mano, orden);

            if (mano.IndiceDeclaracionTanto >= orden.Count)
            {
                ResolverPorDeclarados(mano, orden);
                return true;
            }

            mano.EnvidoPendienteRespuestaDe = orden[mano.IndiceDeclaracionTanto];
            return false;
        }

        /// <summary>Mejor tanto YA DECLARADO entre los jugadores de un equipo (-1 si ninguno).</summary>
        private static int MejorTantoDeclaradoDeEquipo(ManoTruco3v3 mano, string equipoId)
        {
            int mejor = -1;
            foreach (var jugador in mano.ObtenerEquipo(equipoId).Jugadores)
            {
                if (mano.TantosDeclarados.TryGetValue(jugador.Id, out var t) && t.HasValue && t.Value > mejor)
                    mejor = t.Value;
            }
            return mejor;
        }

        /// <summary>Equipo que va ganando según los tantos ya declarados (mano gana empates).</summary>
        private static string? EquipoLiderDeclaracion(ManoTruco3v3 mano, List<string> orden, int hastaIndice)
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
                    lider = equipo;
                }
            }
            return lider;
        }

        /// <summary>Avanza el índice salteando a los jugadores cuyo equipo ya va ganando.</summary>
        private static void AvanzarHastaProximoDeclarante(ManoTruco3v3 mano, List<string> orden)
        {
            while (mano.IndiceDeclaracionTanto < orden.Count)
            {
                string siguiente = orden[mano.IndiceDeclaracionTanto];
                string? lider = EquipoLiderDeclaracion(mano, orden, mano.IndiceDeclaracionTanto);
                if (lider != null && mano.ObtenerEquipoDeJugador(siguiente) == lider)
                {
                    mano.IndiceDeclaracionTanto++;
                    continue;
                }
                break;
            }
        }

        /// <summary>Resuelve el envido según los tantos declarados (gana el mejor; empate → mano).</summary>
        private static void ResolverPorDeclarados(ManoTruco3v3 mano, List<string> orden)
        {
            string ganador = EquipoLiderDeclaracion(mano, orden, orden.Count) ?? mano.EquipoMano;
            int decA = MejorTantoDeclaradoDeEquipo(mano, "EquipoA");
            int decB = MejorTantoDeclaradoDeEquipo(mano, "EquipoB");
            FinalizarEnvido(mano, ganador, $"EquipoA: {decA} vs EquipoB: {decB}. Ganador: {ganador}");
        }

        private static void FinalizarEnvido(ManoTruco3v3 mano, string equipoGanador, string descripcion)
        {
            mano.GanadorEnvido          = equipoGanador;
            mano.EnvidoResuelto         = true;
            mano.FaseEnvido             = "resuelto";
            mano.EnvidoPendienteRespuestaDe = null;

            int puntosEnJuego = mano.PuntosEnvido;

            // Falta Envido: vale exactamente lo que le falta al ganador para llegar a 30.
            // ObtenerPuntosSegunTipo devuelve 0 para FaltaEnvido porque el valor es dinámico.
            if (mano.TipoEnvidoCantado == "FaltaEnvido")
            {
                int puntosActualesGanador = equipoGanador == "EquipoA"
                    ? mano.PuntosEquipoA
                    : mano.PuntosEquipoB;
                puntosEnJuego = Math.Max(JuegoServicio3v3.PuntajeObjetivo - puntosActualesGanador, 1);
                mano.PuntosEnvido = puntosEnJuego;
            }

            mano.EstadoEnvido = descripcion + $". Vale {puntosEnJuego} pt.";

            JuegoServicio3v3.SumarPuntos(mano, equipoGanador, puntosEnJuego);
        }

        /// <summary>Resuelve el envido cuando el rival no quiso. Gana el equipo cantor.</summary>
        public static void ResolverNoQuiero(ManoTruco3v3 mano)
        {
            if (mano.CantorEnvido == null) return;
            string equipoCantor = mano.ObtenerEquipoDeJugador(mano.CantorEnvido);
            int pts = Math.Max(1, mano.PuntosEnvidoNoQuiero);
            mano.GanadorEnvido          = equipoCantor;
            mano.PuntosEnvido           = pts;
            mano.EnvidoResuelto         = true;
            mano.FaseEnvido             = "resuelto";
            mano.EnvidoPendienteRespuestaDe = null;
            mano.EstadoEnvido           = $"No quiso. {equipoCantor} gana {pts} punto(s).";
            JuegoServicio3v3.SumarPuntos(mano, equipoCantor, pts);
        }

        /// <summary>Devuelve los puntos en juego según el tipo de envido cantado.</summary>
        public static int ObtenerPuntosEnJuego(string? tipo) =>
            EnvidoServicio.ObtenerPuntosSegunTipo(tipo);
    }
}
