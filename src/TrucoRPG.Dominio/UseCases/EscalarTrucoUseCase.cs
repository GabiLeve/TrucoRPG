using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class EscalarTrucoUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó.");
            if (mano.GanadorMano != null)
                throw new InvalidOperationException("La mano ya terminó.");
            if (!mano.TrucoCantado || mano.TrucoResuelto)
                throw new InvalidOperationException("No hay truco activo para escalar.");
            if (mano.TrucoPendienteRespuestaHumano || mano.EnvidoPendienteRespuestaHumano)
                throw new InvalidOperationException("Hay un canto pendiente de respuesta.");
            if (mano.NivelTruco >= 3)
                throw new InvalidOperationException("El truco ya está en su nivel máximo.");
            if (mano.CantorTruco == "Humano")
                throw new InvalidOperationException("No podés escalar tu propio canto. Solo puede escalar quien respondió.");

            mano.NivelTruco++;
            mano.CantorTruco     = "Humano";
            string nombreNivel   = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
            mano.PuntosTrucoMano = mano.NivelTruco == 2 ? 3 : 4;
            mano.EstadoTruco     = $"Cantaste {nombreNivel}.";

            bool aceptaMaquina = DecisionMaquinaServicio.AceptarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina);
            if (!aceptaMaquina)
            {
                mano.TrucoResuelto      = true;
                mano.GanadorMano        = "Humano";
                mano.PuntosTrucoMano    = mano.NivelTruco;
                mano.CartaMaquinaEnMesa = null;
                mano.EstadoTruco        = $"La máquina no quiso el {nombreNivel}. \n¡Ganaste {mano.NivelTruco} punto(s)!";
                JuegoServicio.SumarPuntos(mano, "Humano", mano.NivelTruco);
            }
            else if (mano.NivelTruco < 3 &&
                     DecisionMaquinaServicio.EscalarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina, mano.NivelTruco))
            {
                mano.NivelTruco++;
                mano.CantorTruco     = "Maquina";
                mano.PuntosTrucoMano = mano.NivelTruco == 3 ? 4 : 3;
                string nombreContracanto = mano.NivelTruco == 3 ? "Vale Cuatro" : "Retruco";
                mano.TrucoPendienteRespuestaHumano = true;
                mano.EstadoTruco = $"\nLa máquina aceptó y cantó {nombreContracanto}! Esta mano vale {mano.PuntosTrucoMano} punto(s). \n¿Querés?";
            }
            else
            {
                // Solo cerramos la negociación en el nivel máximo (Vale Cuatro).
                // En niveles menores, el respondedor aún puede escalar.
                mano.TrucoResuelto = (mano.NivelTruco >= 3);
                mano.EstadoTruco   = $"La máquina quiso el {nombreNivel}. Esta mano vale {mano.PuntosTrucoMano} punto(s).";
                HabilidadesTrucoServicio.NotificarTrucoAceptado(mano, IdJugador.Humano);
            }

            if (!mano.TrucoPendienteRespuestaHumano)
                MaquinaServicio.AvanzarTurno(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
