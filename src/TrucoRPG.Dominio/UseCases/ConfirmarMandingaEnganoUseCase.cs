using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ConfirmarMandingaEnganoUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (!mano.MandingaEnganoBloqueando)
                return mano;

            MandingaServicio.ConfirmarEngano(mano);
            HabilidadesRivalOrquestador.ActualizarVista(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
