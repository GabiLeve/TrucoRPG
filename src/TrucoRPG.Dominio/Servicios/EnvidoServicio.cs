using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class EnvidoServicio
    {
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

        // ── Resolución completa de envido (extraída del Controller) ───────────────
        public static void ResolverEnvido(ManoTruco mano, int puntosEnJuego, string prefijoEstado)
        {
            mano.TantoHumano  = CalcularTanto(mano.Humano.Mano);
            mano.TantoMaquina = CalcularTanto(mano.Maquina.Mano);

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

            if (mano.TipoEnvidoCantado == "FaltaEnvido")
            {
                int puntosActualesGanador = mano.GanadorEnvido == "Humano"
                    ? mano.PuntosHumano : mano.PuntosMaquina;
                puntosEnJuego = Math.Max(30 - puntosActualesGanador, 1);
            }

            mano.PuntosEnvido   = puntosEnJuego;
            mano.EnvidoResuelto = true;
            mano.EstadoEnvido =
                $"{prefijoEstado}. Tu tanto: {mano.TantoHumano}. " +
                $"La máquina cantó: {mano.TantoCantadoMaquina} (real: {mano.TantoMaquina}). " +
                $"Ganador del envido: {mano.GanadorEnvido} ({mano.PuntosEnvido} pto/s).";

            JuegoServicio.SumarPuntos(
                mano, mano.GanadorEnvido, mano.PuntosEnvido, OrigenPuntos.Envido, mano.CantorEnvido);
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
