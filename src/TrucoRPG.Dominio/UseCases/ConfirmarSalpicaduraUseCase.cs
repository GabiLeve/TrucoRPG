using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ConfirmarSalpicaduraUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (!mano.SalpicaduraBloqueando)
                return mano;

            SalpicaduraServicio.ReemplazarCartasHumano(mano);
            mano.SalpicaduraBloqueando = false;
            mano.UltimoMensajeHabilidadRival =
                "¡Salpicadura! Nahuelito cambió el palo de 2 de tus cartas.";

            if (mano.ManoIniciadaPor == IdJugador.Maquina && mano.GanadorMano is null
                && !MaquinaServicio.EsModoHistoriaPasoAPaso(mano)
                && !mano.TravesuraBloqueando && !mano.RasgunoBloqueando)
                MaquinaServicio.ProcesarIniciativa(mano);

            DestelloServicio.EvaluarTurnoHumano(mano);

            HabilidadesOrquestador.ActualizarVistas(mano);
            HabilidadesRivalOrquestador.ActualizarVista(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
