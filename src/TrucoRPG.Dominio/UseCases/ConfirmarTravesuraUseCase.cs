using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ConfirmarTravesuraUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (!mano.TravesuraBloqueando)
                return mano;

            TravesuraServicio.OcultarCartasHumano(mano);
            mano.TravesuraBloqueando = false;
            mano.UltimoMensajeHabilidadRival =
                "¡Travesura! El Pomberito ocultó 2 de tus cartas. ¡Recordalas bien!";

            if (mano.ManoIniciadaPor == IdJugador.Maquina && mano.GanadorMano is null
                && !MaquinaServicio.EsModoHistoriaPasoAPaso(mano)
                && !mano.RasgunoBloqueando)
                MaquinaServicio.ProcesarIniciativa(mano);

            DestelloServicio.EvaluarTurnoHumano(mano);

            HabilidadesOrquestador.ActualizarVistas(mano);
            HabilidadesRivalOrquestador.ActualizarVista(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
