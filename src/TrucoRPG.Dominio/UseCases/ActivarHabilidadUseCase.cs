using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ActivarHabilidadUseCase
    {
        public ManoTruco Ejecutar(Guid manoId, int? numeroCarta = null, string? paloCarta = null)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó.");
            if (mano.GanadorMano != null)
                throw new InvalidOperationException("La mano ya terminó.");

            var resultado = HabilidadesOrquestador.Activar(mano, new SolicitudActivarHabilidad
            {
                IdJugador = IdJugador.Humano,
                NumeroCarta = numeroCarta,
                PaloCarta = paloCarta
            });

            if (!resultado.Exito)
                throw new InvalidOperationException(resultado.Mensaje);

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
