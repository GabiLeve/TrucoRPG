using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class IrseAlMazoUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            SalpicaduraBloqueoServicio.ValidarNoBloqueado(mano);

            if (mano.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó.");
            if (mano.GanadorMano != null)
                throw new InvalidOperationException("La mano ya terminó.");
            if (mano.EnvidoPendienteRespuestaHumano || mano.TrucoPendienteRespuestaHumano)
                throw new InvalidOperationException("Respondé el canto pendiente antes de irte al mazo.");

            int puntosParaMaquina   = mano.TrucoCantado && mano.PuntosTrucoMano > 0 ? mano.PuntosTrucoMano : 1;
            mano.GanadorMano        = "Maquina";
            mano.TrucoResuelto      = true;
            mano.CartaMaquinaEnMesa = null;
            mano.EstadoTruco        = $"Te fuiste al mazo. La máquina gana {puntosParaMaquina} punto(s).";
            JuegoServicio.SumarPuntos(
                mano, IdJugador.Maquina, puntosParaMaquina, OrigenPuntos.TrucoMano, mano.CantorTruco);
            MandingaServicio.RegistrarFinMano(mano);
            HabilidadesRivalOrquestador.ActualizarVista(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
