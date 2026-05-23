using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    /// <summary>
    /// El humano canta un tipo de envido. La máquina responde automáticamente.
    /// </summary>
    public class CantarEnvidoUseCase
    {
        public ManoTruco Ejecutar(Guid manoId, string tipo)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó. El primero en llegar a 30 gana.");
            if (mano.EnvidoCantado || mano.EnvidoResuelto)
                throw new InvalidOperationException("El envido ya fue cantado.");
            if (mano.TrucoResuelto)
                throw new InvalidOperationException("No se puede cantar envido después de que el truco fue aceptado.");
            if (mano.Bazas.Count > 0)
                throw new InvalidOperationException("El envido solo puede cantarse antes de jugar la primera baza.");

            int puntosEnJuego              = EnvidoServicio.ObtenerPuntosSegunTipo(tipo);
            mano.TipoEnvidoCantado         = EnvidoServicio.NormalizarTipo(tipo);
            mano.EnvidoCantado             = true;
            mano.CantorEnvido              = "Humano";
            mano.EnvidoPendienteRespuestaMaquina = true;
            mano.EstadoEnvido              = $"Humano cantó {tipo}.";

            bool aceptaMaquina = DecisionMaquinaServicio.AceptarEnvido(mano.Maquina.Mano, mano.NivelMentiraEnvidoMaquina);
            mano.EnvidoPendienteRespuestaMaquina = false;

            if (!aceptaMaquina)
            {
                mano.EnvidoResuelto = true;
                mano.GanadorEnvido  = "Humano";
                mano.PuntosEnvido   = 1;
                mano.EstadoEnvido   = "La máquina no quiso. Ganaste 1 punto de envido.";
                JuegoServicio.SumarPuntos(mano, mano.GanadorEnvido, mano.PuntosEnvido);
                EnvidoServicio.LimpiarDatosDeEnvido(mano);
            }
            else
            {
                EnvidoServicio.ResolverEnvido(mano, puntosEnJuego, "La máquina quiso");
            }

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
