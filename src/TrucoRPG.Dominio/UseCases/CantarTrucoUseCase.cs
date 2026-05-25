using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class CantarTrucoUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó. El primero en llegar a 30 gana.");
            if (mano.GanadorMano != null)
                throw new InvalidOperationException("La mano ya terminó.");
            if (mano.TrucoCantado)
                throw new InvalidOperationException("El truco ya fue cantado en esta mano.");

            mano.TrucoCantado = true;
            mano.NivelTruco   = 1;
            mano.CantorTruco  = "Humano";
            mano.EstadoTruco  = "Cantaste Truco.";

            bool aceptaMaquina = DecisionMaquinaServicio.AceptarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina);
            if (!aceptaMaquina)
            {
                mano.TrucoResuelto   = true;
                mano.GanadorMano     = "Humano";
                mano.PuntosTrucoMano = 1;
                mano.EstadoTruco     = "La máquina no quiso el truco. Ganaste 1 punto.";
                JuegoServicio.SumarPuntos(
                    mano, mano.GanadorMano, mano.PuntosTrucoMano, OrigenPuntos.TrucoRechazo, mano.CantorTruco);
                PartidaMemoriaServicio.Actualizar(mano);
                return mano;
            }

            bool escalaARetruco = DecisionMaquinaServicio.EscalarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina, 1);
            if (escalaARetruco)
            {
                mano.NivelTruco                    = 2;
                mano.PuntosTrucoMano               = 3;
                mano.CantorTruco                   = "Maquina";
                mano.TrucoPendienteRespuestaHumano = true;
                mano.EstadoTruco                   = "\nLa máquina aceptó y cantó Retruco! Esta mano vale 3 puntos. \n¿Querés?";
            }
            else
            {
                mano.PuntosTrucoMano = 2;
                // No marcamos TrucoResuelto: el respondedor (humano) aún puede escalar a Retruco.
                mano.EstadoTruco = "La máquina quiso el truco. Esta mano vale 2 puntos.";
                HabilidadesTrucoServicio.NotificarTrucoAceptado(mano, IdJugador.Humano);
                if (!mano.TrucoPendienteRespuestaHumano)
                    MaquinaServicio.AvanzarTurno(mano);
            }

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
