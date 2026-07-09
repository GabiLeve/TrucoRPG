using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class EnvidoServicio
    {
        /// <summary>
        /// Reglas 1v1: envido antes de la primera baza. Si ya se cantó truco,
        /// solo se puede cantar envido mientras el truco sigue sin responder
        /// ("el envido va primero"); una vez respondido, no hay más envido.
        /// </summary>
        public static bool PuedeCantarEnvido(ManoTruco mano)
        {
            if (mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.Bazas.Count > 0) return false;
            if (mano.PartidaTerminada || mano.GanadorMano != null) return false;

            if (mano.TrucoCantado && !mano.TrucoPendienteRespuestaHumano)
                return false;

            return true;
        }

        public static int CalcularTanto(List<Carta> cartas)
        {
            if (!cartas.Any()) return 0;
            var gruposPorPalo = cartas.GroupBy(c => c.Palo).ToList();

            int mejorTanto = 0;

            foreach (var grupo in gruposPorPalo)
            {
                var cartasDelPalo = grupo
                    .Select(c => ValorEnvido(c.Numero))
                    .OrderByDescending(v => v)
                    .ToList();

                if (cartasDelPalo.Count >= 2)
                {
                    int tanto = cartasDelPalo[0] + cartasDelPalo[1] + 20;
                    if (tanto > mejorTanto)
                        mejorTanto = tanto;
                }
            }

            if (mejorTanto > 0)
                return mejorTanto;

            return cartas.Max(c => ValorEnvido(c.Numero));
        }

        private static int ValorEnvido(int numero)
        {
            if (numero >= 10)
                return 0;

            return numero;
        }

        /// <summary>
        /// Tanto calculado con las 3 cartas originales del jugador (las que tiene en mano
        /// más las que ya jugó). El envido SIEMPRE se cuenta con las cartas repartidas,
        /// aunque alguna ya se haya tirado en la primera vuelta.
        /// </summary>
        public static int CalcularTantoOriginal(Jugador jugador) =>
            CalcularTanto(jugador.Mano.Concat(jugador.Jugadas).ToList());

        public const int UmbralBuenas = 15;

        /// <summary>
        /// Las "malas": menos de 15 puntos. Las "buenas": 15 o más.
        /// </summary>
        public static bool AmbosEnMalas(int puntosHumano, int puntosMaquina) =>
            puntosHumano < UmbralBuenas && puntosMaquina < UmbralBuenas;

        /// <summary>
        /// Puntos de la Falta Envido cuando al menos uno está en las buenas:
        /// lo que le falta al que va ganando para llegar a 30.
        /// </summary>
        public static int CalcularPuntosFalta(int puntosDelQueVaGanando) =>
            Math.Max(30 - puntosDelQueVaGanando, 1);

        /// <summary>
        /// Resuelve la falta envido y suma puntos o termina la partida según las reglas.
        /// </summary>
        public static void AplicarPuntosFaltaEnvido(ManoTruco mano, string ganadorEnvido)
        {
            if (AmbosEnMalas(mano.PuntosHumano, mano.PuntosMaquina))
            {
                mano.PuntosEnvido = 30 - PuntosDe(ganadorEnvido, mano);
                JuegoServicio.TerminarPartidaPorFaltaEnMalas(mano, ganadorEnvido);
                return;
            }

            int puntosLider = Math.Max(mano.PuntosHumano, mano.PuntosMaquina);
            int puntos      = CalcularPuntosFalta(puntosLider);
            mano.PuntosEnvido = puntos;
            JuegoServicio.SumarPuntosFaltaEnvido(mano, ganadorEnvido, puntos);
        }

        private static int PuntosDe(string jugador, ManoTruco mano) =>
            jugador == IdJugador.Humano ? mano.PuntosHumano : mano.PuntosMaquina;

        /// <summary>
        /// Cuánto SUMA cada canto a la cadena del envido cuando se acepta.
        /// (Envido +2, Envido Envido +2, Real Envido +3; la Falta se calcula aparte.)
        /// </summary>
        public static int IncrementoPuntosTipo(string? tipo) =>
            NormalizarTipo(tipo) switch
            {
                "EnvidoEnvido" => 2,
                "RealEnvido"   => 3,
                "FaltaEnvido"  => 0,
                _              => 2
            };

        // ── Resolución completa de envido (extraída del Controller) ───────────────
        public static void ResolverEnvido(ManoTruco mano, int puntosEnJuego, string prefijoEstado)
        {
            // El tanto se calcula con las cartas ORIGINALES (mano + jugadas):
            // si alguien ya tiró una carta en la primera vuelta, igual cuenta para el envido.
            mano.TantoHumano  = CalcularTantoOriginal(mano.Humano);
            mano.TantoMaquina = CalcularTantoOriginal(mano.Maquina);

            mano.TantoCantadoMaquina = MentiraEnvidoServicio.ObtenerTantoCantado(
                mano.TantoMaquina.Value,
                mano.NivelMentiraEnvidoMaquina,
                out bool mintio);

            mano.MaquinaMintioEnvido     = mintio;
            mano.TipoCantoEnvidoMaquina  = ClasificarActitud(mano.TantoMaquina.Value, mintio);

            if (mano.TantoHumano > mano.TantoMaquina)
                mano.GanadorEnvido = "Humano";
            else if (mano.TantoMaquina > mano.TantoHumano)
                mano.GanadorEnvido = "Maquina";
            else
                mano.GanadorEnvido = HabilidadesOrquestador.ResolverGanadorEmpateEnvido(
                    mano, mano.ManoIniciadaPor);

            mano.EnvidoResuelto = true;

            if (mano.TipoEnvidoCantado == "FaltaEnvido")
            {
                EnvidoServicio.AplicarPuntosFaltaEnvido(mano, mano.GanadorEnvido!);
                mano.EstadoEnvido =
                    $"{prefijoEstado}. Tu tanto: {mano.TantoHumano}. " +
                    $"La máquina cantó: {mano.TantoCantadoMaquina} (real: {mano.TantoMaquina}). " +
                    $"Ganador del envido: {mano.GanadorEnvido} ({mano.PuntosEnvido} pto/s)." +
                    (mano.PartidaTerminada ? " ¡Partida terminada!" : "");
            }
            else
            {
                mano.PuntosEnvido = puntosEnJuego;
                mano.EstadoEnvido =
                    $"{prefijoEstado}. Tu tanto: {mano.TantoHumano}. " +
                    $"La máquina cantó: {mano.TantoCantadoMaquina} (real: {mano.TantoMaquina}). " +
                    $"Ganador del envido: {mano.GanadorEnvido} ({mano.PuntosEnvido} pto/s).";

                JuegoServicio.SumarPuntos(
                    mano, mano.GanadorEnvido, mano.PuntosEnvido, OrigenPuntos.Envido, mano.CantorEnvido);
            }
        }

        public static void LimpiarDatosDeEnvido(ManoTruco mano)
        {
            mano.TantoHumano            = null;
            mano.TantoMaquina           = null;
            mano.TantoCantadoMaquina    = null;
            mano.MaquinaMintioEnvido    = false;
            mano.TipoCantoEnvidoMaquina = null;
        }

        public static int ObtenerPuntosSegunTipo(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo)) return 2;
            return tipo.Trim().ToLowerInvariant() switch
            {
                "envido"                              => 2,
                "envido envido" or "envidoenvido"     => 4,
                "real envido"   or "realenvido"       => 3,
                "falta envido"  or "faltaenvido"      => 0,
                _                                     => 2
            };
        }

        public static int OrdinalTipo(string? tipo) =>
            tipo switch
            {
                "Envido"       => 0,
                "EnvidoEnvido" => 1,
                "RealEnvido"   => 2,
                "FaltaEnvido"  => 3,
                _              => -1
            };

        public static string NormalizarTipo(string? tipo) =>
            tipo?.Trim().ToLowerInvariant() switch
            {
                "envido envido" or "envidoenvido"   => "EnvidoEnvido",
                "real envido"   or "realenvido"     => "RealEnvido",
                "falta envido"  or "faltaenvido"    => "FaltaEnvido",
                _                                   => "Envido"
            };

        public static string ClasificarActitud(int tantoReal, bool mintio)
        {
            if (mintio) return "mintio";
            return tantoReal < 23 ? "se_jugo" : "tenia";
        }
    }
}
