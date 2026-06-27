using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class AvanzarMaquinaHistoriaUseCase
    {
        public (ManoTruco Mano, Truco1v1EventoMaquina? Evento) Ejecutar(Guid manoId)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            var evento = MaquinaServicio.AvanzarUnPaso(mano);
            PartidaMemoriaServicio.Actualizar(mano);
            return (mano, evento);
        }
    }
}
