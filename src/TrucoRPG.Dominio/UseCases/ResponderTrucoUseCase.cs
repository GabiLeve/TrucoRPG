using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    /// <summary>
    /// El humano responde al truco de la máquina: acepta, rechaza, o escala a Retruco / Vale Cuatro.
    /// </summary>
    public class ResponderTrucoUseCase
    {
        public ManoTruco Ejecutar(Guid manoId, bool aceptar, string? escalarA)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (!mano.TrucoPendienteRespuestaHumano)
                throw new InvalidOperationException("No hay respuesta pendiente para truco.");

            mano.TrucoPendienteRespuestaHumano = false;

            // ── NO QUIERO ─────────────────────────────────────────────
            if (!aceptar)
            {
                int puntosRefusal    = mano.NivelTruco;
                mano.TrucoResuelto   = true;
                mano.GanadorMano     = mano.CantorTruco;
                mano.PuntosTrucoMano = puntosRefusal;
                string ganadorNombre = mano.GanadorMano == "Humano" ? "Vos ganaste" : "La máquina ganó";
                mano.EstadoTruco     = $"No quisiste. {ganadorNombre} {puntosRefusal} punto(s).";
                JuegoServicio.SumarPuntos(mano, mano.GanadorMano, puntosRefusal);
                PartidaMemoriaServicio.Actualizar(mano);
                return mano;
            }

            // ── ESCALAR (Retruco o Vale Cuatro como respuesta) ────────
            var escalacion = escalarA?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(escalacion))
            {
                if (escalacion == "retruco" && mano.NivelTruco == 1)
                {
                    mano.NivelTruco      = 2;
                    mano.PuntosTrucoMano = 3;
                    mano.CantorTruco     = "Humano";

                    bool aceptaMaquina = DecisionMaquinaServicio.AceptarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina);
                    if (!aceptaMaquina)
                    {
                        mano.TrucoResuelto   = true;
                        mano.GanadorMano     = "Humano";
                        mano.PuntosTrucoMano = 2;
                        mano.EstadoTruco     = "La máquina no quiso el retruco. \n¡Ganaste 2 puntos!";
                        JuegoServicio.SumarPuntos(mano, "Humano", 2);
                    }
                    else
                    {
                        bool escalaAValeC = DecisionMaquinaServicio.EscalarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina, 2);
                        if (escalaAValeC)
                        {
                            mano.NivelTruco                    = 3;
                            mano.PuntosTrucoMano               = 4;
                            mano.CantorTruco                   = "Maquina";
                            mano.TrucoPendienteRespuestaHumano = true;
                            mano.EstadoTruco                   = "\nLa máquina aceptó y cantó Vale Cuatro! Esta mano vale 4 puntos. \n¿Querés?";
                        }
                        else
                        {
                            // Máquina aceptó el Retruco (nivel 2). La máquina es el respondedor
                            // y podría cantar Vale Cuatro, pero ya decidió no hacerlo.
                            // Solo cerramos en nivel máximo.
                            mano.TrucoResuelto = (mano.NivelTruco >= 3);
                            mano.EstadoTruco   = "La máquina quiso el retruco. Esta mano vale 3 puntos.";
                            HabilidadesTrucoServicio.NotificarTrucoAceptado(mano, IdJugador.Humano);
                        }
                    }
                }
                else if (escalacion == "valecuatro" && mano.NivelTruco == 2)
                {
                    mano.NivelTruco      = 3;
                    mano.PuntosTrucoMano = 4;
                    mano.CantorTruco     = "Humano";

                    bool aceptaMaquina = DecisionMaquinaServicio.AceptarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina);
                    if (!aceptaMaquina)
                    {
                        mano.TrucoResuelto   = true;
                        mano.GanadorMano     = "Humano";
                        mano.PuntosTrucoMano = 3;
                        mano.EstadoTruco     = "La máquina no quiso el vale cuatro. \n¡Ganaste 3 puntos!";
                        JuegoServicio.SumarPuntos(mano, "Humano", 3);
                    }
                    else
                    {
                        mano.TrucoResuelto = true;
                        mano.EstadoTruco   = "La máquina quiso el vale cuatro. Esta mano vale 4 puntos.";
                        HabilidadesTrucoServicio.NotificarTrucoAceptado(mano, IdJugador.Humano);
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Escalación '{escalarA}' inválida para el nivel actual de truco (nivel {mano.NivelTruco}).");
                }
            }
            else
            {
                // ── QUIERO (sin escalar) ──────────────────────────────
                // Solo cerramos la negociación en el nivel máximo (Vale Cuatro).
                // En niveles menores el respondedor aún puede escalar.
                mano.TrucoResuelto = (mano.NivelTruco >= 3);
                mano.EstadoTruco   = $"Quisiste. Esta mano vale {mano.PuntosTrucoMano} punto(s).";
                if (mano.TrucoResuelto || mano.NivelTruco >= 1)
                    HabilidadesTrucoServicio.NotificarTrucoAceptado(mano, mano.CantorTruco ?? IdJugador.Maquina);
            }

            if (!mano.TrucoPendienteRespuestaHumano)
                MaquinaServicio.AvanzarTurno(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
