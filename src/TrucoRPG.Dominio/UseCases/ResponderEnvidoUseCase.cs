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
                    int puntosNuevos = EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado);
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
    }
}
