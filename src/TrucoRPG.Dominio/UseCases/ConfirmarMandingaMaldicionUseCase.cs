using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ConfirmarMandingaMaldicionUseCase
    {
        public ManoTruco Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (!mano.MandingaMaldicionBloqueando)
                return mano;

            MandingaServicio.ConfirmarMaldicion(mano);
            HabilidadesRivalOrquestador.ActualizarVista(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
