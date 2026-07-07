using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class SumarPuntosHumanoDebugUseCase
    {
        public const int PuntosASumar = 10;

        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.Configuracion.Modo != ModoJuego.Historia)
                throw new InvalidOperationException("Solo disponible en partidas de modo historia.");

            if (!MandingaServicio.EsMandingaHistoria(mano))
                throw new InvalidOperationException("Solo disponible contra El Mandinga.");

            if (mano.PartidaTerminada)
                return mano;

            mano.PuntosHumano = Math.Min(30, mano.PuntosHumano + PuntosASumar);
            MandingaServicio.SincronizarDesbloqueosFases(mano);
            HabilidadesRivalOrquestador.ActualizarVista(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
