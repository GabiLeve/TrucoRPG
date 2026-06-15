using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ResponderEnvidoUseCase
    {
        public ManoTruco Ejecutar(Guid manoId, bool aceptar, string? escalarA)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó. El primero en llegar a 30 gana.");
            if (!mano.EnvidoCantado)
                throw new InvalidOperationException("No hay un envido pendiente.");
            if (!mano.EnvidoPendienteRespuestaHumano)
                throw new InvalidOperationException("No hay respuesta pendiente del humano.");

            // ── NO QUIERO ─────────────────────────────────────────────
            if (!aceptar)
            {
                mano.EnvidoPendienteRespuestaHumano = false;
                mano.EnvidoResuelto = true;
                mano.GanadorEnvido  = "Maquina";
                mano.PuntosEnvido   = 1;
                mano.EstadoEnvido   = "No quisiste el envido. La máquina ganó 1 punto.";
                JuegoServicio.SumarPuntos(mano, mano.GanadorEnvido, mano.PuntosEnvido);
                EnvidoServicio.LimpiarDatosDeEnvido(mano);
                MaquinaServicio.AvanzarTurno(mano);
                PartidaMemoriaServicio.Actualizar(mano);
                return mano;
            }

            // ── ESCALAR (Humano contra-canta Envido/Real/Falta) ──
            var escalacion = escalarA?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(escalacion))
            {
                var tipoActual = (mano.TipoEnvidoCantado ?? "Envido").ToLowerInvariant();
                bool valida = escalacion switch
                {
                    "envido" or "envido envido" or "envidoenvido"
                                   => tipoActual == "envido",
                    "real envido"  => tipoActual is "envido" or "envidoenvido" or "envido envido",
                    "falta envido" => tipoActual is "envido" or "envidoenvido" or "envido envido" or "realenvido" or "real envido",
                    _              => false
                };
                if (!valida)
                    throw new InvalidOperationException(
                        $"No podés escalar a '{escalarA}' desde '{mano.TipoEnvidoCantado}'.");

                // Validación OK → recién ahora limpiamos el flag
                mano.EnvidoPendienteRespuestaHumano = false;

                int puntosAntes        = EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado);
                mano.TipoEnvidoCantado = EnvidoServicio.NormalizarTipo(escalarA);
                mano.CantorEnvido      = "Humano";
                mano.EstadoEnvido      = $"Cantaste {escalarA}.";

                bool aceptaMaquina = DecisionMaquinaServicio.AceptarEnvido(mano.Maquina.Mano, mano.NivelMentiraEnvidoMaquina);
                if (!aceptaMaquina)
                {
                    mano.EnvidoResuelto = true;
                    mano.GanadorEnvido  = "Humano";
                    mano.PuntosEnvido   = puntosAntes > 0 ? puntosAntes : 2;
                    mano.EstadoEnvido   = $"La máquina no quiso {escalarA}. Ganaste {mano.PuntosEnvido} punto(s).";
                    JuegoServicio.SumarPuntos(mano, mano.GanadorEnvido, mano.PuntosEnvido);
                    EnvidoServicio.LimpiarDatosDeEnvido(mano);
                    MaquinaServicio.AvanzarTurno(mano);
                }
                else
                {
                    // Los cantos del envido se ACUMULAN: "Envido" (2) + "Real Envido" (3) = 5,
                    // no solo el valor del último canto. La Falta se calcula en ResolverEnvido.
                    int puntosNuevos = puntosAntes + EnvidoServicio.IncrementoPuntosTipo(mano.TipoEnvidoCantado);
                    EnvidoServicio.ResolverEnvido(mano, puntosNuevos, $"La máquina quiso tu {escalarA}");
                    MaquinaServicio.AvanzarTurno(mano);
                }

                PartidaMemoriaServicio.Actualizar(mano);
                return mano;
            }

            // ── QUIERO (sin escalar) ──────────────────────────────────
            mano.EnvidoPendienteRespuestaHumano = false;
            int puntosEnJuego = EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado ?? "Envido");
            EnvidoServicio.ResolverEnvido(mano, puntosEnJuego, "Aceptaste el envido de la máquina");
            MaquinaServicio.AvanzarTurno(mano);
            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }

        /// <summary>
        /// "Son Buenas" en 1v1: el humano reconoce que la máquina tiene más tantos.
        /// Solo válido cuando la máquina cantó el envido y el humano lo aceptó (quiero)
        /// ANTES de que se resuelvan los tantos.
        /// </summary>
        public ManoTruco EjecutarSonBuenas(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó.");
            if (!mano.EnvidoCantado)
                throw new InvalidOperationException("No hay un envido pendiente.");
            if (mano.EnvidoResuelto)
                throw new InvalidOperationException("El envido ya fue resuelto.");
            // Son buenas solo aplica cuando la máquina cantó y el humano va a declarar tantos
            if (mano.CantorEnvido != "Maquina")
                throw new InvalidOperationException("'Son buenas' solo aplica cuando la máquina cantó el envido.");
            if (mano.EnvidoPendienteRespuestaHumano)
                throw new InvalidOperationException("Primero debés aceptar el envido antes de decir 'son buenas'.");

            // Resolver: la máquina gana
            mano.SonBuenasDeclarado = true;
            mano.FaseEnvido         = "resuelto";
            mano.EnvidoResuelto     = true;
            mano.GanadorEnvido      = "Maquina";
            int puntosEnJuego       = EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado ?? "Envido");
            mano.PuntosEnvido       = puntosEnJuego;
            mano.EstadoEnvido       = $"Dijiste 'son buenas'. La máquina gana {puntosEnJuego} punto(s) de envido.";

            JuegoServicio.SumarPuntos(mano, "Maquina", puntosEnJuego);
            EnvidoServicio.LimpiarDatosDeEnvido(mano);
            MaquinaServicio.AvanzarTurno(mano);
            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
