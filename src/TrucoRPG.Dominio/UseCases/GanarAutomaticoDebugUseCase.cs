using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    /// <summary>
    /// SOLO PRUEBAS — Forzar victoria en modo historia. Eliminar antes de producción.
    /// </summary>
    public class GanarAutomaticoDebugUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.Configuracion.Modo != ModoJuego.Historia)
                throw new InvalidOperationException("Solo disponible en partidas de modo historia.");

            if (mano.PartidaTerminada)
                return mano;

            mano.PuntosHumano = 30;
            mano.PartidaTerminada = true;
            mano.GanadorPartida = IdJugador.Humano;
            mano.SalpicaduraBloqueando = false;
            mano.TravesuraBloqueando = false;
            mano.RasgunoBloqueando = false;
            mano.AullidoBloqueando = false;

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
