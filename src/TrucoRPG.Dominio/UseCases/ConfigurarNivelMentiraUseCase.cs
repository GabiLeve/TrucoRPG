using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ConfigurarNivelMentiraUseCase
    {
        public ManoTruco EjecutarEnvido(Guid manoId, int nivel)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            mano.NivelMentiraEnvidoMaquina = Math.Clamp(nivel, 0, 100);
            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }

        public ManoTruco EjecutarTruco(Guid manoId, int nivel)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            mano.NivelMentiraTrucoMaquina = Math.Clamp(nivel, 0, 100);
            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
