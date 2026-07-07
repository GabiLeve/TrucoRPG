using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>Lógica de envido compartida por los modos por equipos (2v2 y 3v3); las fachadas EnvidoServicio2v2/3v3 le pasan lo específico de cada modo (orden de declaración, marcador y responsable del canto).</summary>
    public static class EnvidoEquiposServicio
    {
        public static int TantoOriginal(Jugador jugador) =>
            EnvidoServicio.CalcularTanto(jugador.Mano.Concat(jugador.Jugadas).ToList());

        public static Dictionary<string, int> CalcularTodosLosTantos(IManoEnvidoEquipos mano)
        {
            var resultado = new Dictionary<string, int>();
            foreach (var jugador in mano.OrdenJugadores)
                resultado[jugador.Id] = TantoOriginal(jugador);
            return resultado;
        }

        public static bool PuedeCantarEnvido(IManoEnvidoEquipos mano, string jugadorId)
        {
            if (mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.PartidaTerminada || mano.GanadorMano != null) return false;
            if (mano.JugadoresActivos.Count > 0 && !mano.JugadoresActivos.Contains(jugadorId)) return false;
            if (mano.VueltasJugadas() > 0) return false;
            if ((mano.ObtenerJugador(jugadorId)?.Jugadas.Count ?? 0) > 0) return false;
            if (mano.TrucoCantado &&
                !(mano.NivelTruco == 1
                  && !mano.TrucoResuelto
                  && mano.EquipoCantorTruco != mano.ObtenerEquipoDeJugador(jugadorId)))
                return false;
            return true;
        }

        public static bool Cantar(
            IManoEnvidoEquipos mano, string jugadorId, string tipo,
            Func<string, string> responsable)
        {
            if (!PuedeCantarEnvido(mano, jugadorId)) return false;

            mano.EnvidoCantado        = true;
            mano.CantorEnvido         = jugadorId;
            mano.TipoEnvidoCantado    = EnvidoServicio.NormalizarTipo(tipo);
            mano.PuntosEnvido         = EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado);
            mano.PuntosEnvidoNoQuiero = 1;
            mano.FaseEnvido           = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = responsable(jugadorId);
            mano.EstadoEnvido = $"{jugadorId} cantó {tipo}.";
            return true;
        }

        public static bool Responder(
            IManoEnvidoEquipos mano, string jugadorId, bool aceptar,
            Func<List<string>> orden,
            Action<string, int> sumarPuntos)
        {
            if (!mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.FaseEnvido != "pendiente_respuesta") return false;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return false;

            if (!aceptar)
            {
                ResolverNoQuiero(mano, sumarPuntos);
            }
            else
            {
                mano.EnvidoPendienteRespuestaDe = null;
                mano.FaseEnvido = "aceptado";
                IniciarDeclaracionTantos(mano, orden);
            }
            return true;
        }

        public static bool Escalar(
            IManoEnvidoEquipos mano, string jugadorId, string tipo,
            Func<string, string> responsable)
        {
            if (!mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.FaseEnvido != "pendiente_respuesta") return false;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return false;

            string tipoNuevo = EnvidoServicio.NormalizarTipo(tipo);
            if (EnvidoServicio.OrdinalTipo(tipoNuevo) <= EnvidoServicio.OrdinalTipo(mano.TipoEnvidoCantado)) return false;

            int ptsAntes = mano.PuntosEnvido;
            mano.TipoEnvidoCantado    = tipoNuevo;
            mano.PuntosEnvido         = tipoNuevo == "FaltaEnvido"
                ? 0
                : ptsAntes + EnvidoServicio.IncrementoPuntosTipo(tipoNuevo);
            mano.PuntosEnvidoNoQuiero = Math.Max(1, ptsAntes);
            mano.CantorEnvido         = jugadorId;
            mano.EnvidoPendienteRespuestaDe = responsable(jugadorId);
            mano.EstadoEnvido = $"{jugadorId} cantó {tipo}.";
            return true;
        }

        public static void IniciarDeclaracionTantos(IManoEnvidoEquipos mano, Func<List<string>> orden)
        {
            mano.TantosReales     = CalcularTodosLosTantos(mano);
            mano.TantosDeclarados = new Dictionary<string, int?>();
            foreach (var jugador in mano.OrdenJugadores)
                mano.TantosDeclarados[jugador.Id] = null;

            mano.FaseEnvido = "declarando_tantos";
            mano.IndiceDeclaracionTanto = 0;

            mano.EnvidoPendienteRespuestaDe = orden()[0];
        }

        public static bool ProcesarDeclaracion(
            IManoEnvidoEquipos mano, string jugadorId, int? tanto, bool sonBuenas,
            Func<List<string>> obtenerOrden,
            Action<string, int> sumarPuntos)
        {
            if (mano.FaseEnvido != "declarando_tantos") return false;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return false;

            var orden = obtenerOrden();

            if (sonBuenas)
            {
                mano.SonBuenasDeclarado = true;
                mano.JugadorQueDijoSonBuenas = jugadorId;
                mano.TantosDeclarados[jugadorId] = null;
            }
            else
            {
                if (tanto.HasValue)
                {
                    int declarado = Math.Max(0, tanto.Value);
                    if (mano.TantosReales.TryGetValue(jugadorId, out var real) && declarado > real)
                        declarado = real;
                    tanto = declarado;
                }
                mano.TantosDeclarados[jugadorId] = tanto;
            }

            mano.IndiceDeclaracionTanto++;

            AvanzarHastaProximoDeclarante(mano, orden);

            if (mano.IndiceDeclaracionTanto >= orden.Count)
            {
                ResolverPorDeclarados(mano, orden, sumarPuntos);
                return true;
            }

            mano.EnvidoPendienteRespuestaDe = orden[mano.IndiceDeclaracionTanto];
            return false;
        }

        public static void ResolverNoQuiero(IManoEnvidoEquipos mano, Action<string, int> sumarPuntos)
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
            sumarPuntos(equipoCantor, pts);
        }

        private static int MejorTantoDeclaradoDeEquipo(IManoEnvidoEquipos mano, string equipoId)
        {
            int mejor = -1;
            foreach (var jugador in mano.JugadoresDelEquipo(equipoId))
            {
                if (mano.TantosDeclarados.TryGetValue(jugador.Id, out var t) && t.HasValue && t.Value > mejor)
                    mejor = t.Value;
            }
            return mejor;
        }

        private static string? EquipoLiderDeclaracion(IManoEnvidoEquipos mano, List<string> orden, int hastaIndice)
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

        private static void AvanzarHastaProximoDeclarante(IManoEnvidoEquipos mano, List<string> orden)
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

        private static void ResolverPorDeclarados(
            IManoEnvidoEquipos mano, List<string> orden, Action<string, int> sumarPuntos)
        {
            string ganador = EquipoLiderDeclaracion(mano, orden, orden.Count) ?? mano.EquipoMano;
            int decA = MejorTantoDeclaradoDeEquipo(mano, "EquipoA");
            int decB = MejorTantoDeclaradoDeEquipo(mano, "EquipoB");
            FinalizarEnvido(mano, ganador, $"EquipoA: {decA} vs EquipoB: {decB}. Ganador: {ganador}", sumarPuntos);
        }

        private static void FinalizarEnvido(
            IManoEnvidoEquipos mano, string equipoGanador, string descripcion,
            Action<string, int> sumarPuntos)
        {
            mano.GanadorEnvido          = equipoGanador;
            mano.EnvidoResuelto         = true;
            mano.FaseEnvido             = "resuelto";
            mano.EnvidoPendienteRespuestaDe = null;

            int puntosEnJuego = mano.PuntosEnvido;

            if (mano.TipoEnvidoCantado == "FaltaEnvido")
            {
                int puntosLider = Math.Max(mano.PuntosEquipoA, mano.PuntosEquipoB);
                puntosEnJuego = EnvidoServicio.CalcularPuntosFalta(puntosLider);
                mano.PuntosEnvido = puntosEnJuego;
            }

            mano.EstadoEnvido = descripcion + $". Vale {puntosEnJuego} pt.";

            sumarPuntos(equipoGanador, puntosEnJuego);
        }
    }
}
