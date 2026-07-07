using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ConfirmarRasgunoUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (!mano.RasgunoBloqueando)
                return mano;

            RasgunoServicio.DebilitarCartaAleatoria(mano);
            mano.RasgunoBloqueando = false;
            mano.UltimoMensajeHabilidadRival =
                "¡Rasguño! El Lobizón debilitó 1 de tus cartas.";

            if (mano.ManoIniciadaPor == IdJugador.Maquina && mano.GanadorMano is null
                && !MaquinaServicio.EsModoHistoriaPasoAPaso(mano)
                && !mano.TravesuraBloqueando)
                MaquinaServicio.ProcesarIniciativa(mano);

            DestelloServicio.EvaluarTurnoHumano(mano);

            HabilidadesOrquestador.ActualizarVistas(mano);
            HabilidadesRivalOrquestador.ActualizarVista(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
