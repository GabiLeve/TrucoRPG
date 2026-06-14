using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class JugarCartaUseCase
    {
        public ManoTruco Ejecutar(Guid manoId, int numero, string palo)
        {
            var mano = PartidaMemoriaServicio.Obtener(manoId)
                ?? throw new KeyNotFoundException("No se encontró la mano.");

            if (mano.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó.");
            if (mano.GanadorMano != null)
                throw new InvalidOperationException("La mano ya terminó.");
            if (mano.TrucoPendienteRespuestaHumano || mano.EnvidoPendienteRespuestaHumano)
                throw new InvalidOperationException("Respondé el canto pendiente antes de jugar.");

            SalpicaduraBloqueoServicio.ValidarNoBloqueado(mano);

            var cartaHumano = mano.Humano.Mano
                .FirstOrDefault(c => c.Numero == numero &&
                                     c.Palo.Equals(palo, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("La carta no está en tu mano.");

            mano.Humano.Mano.Remove(cartaHumano);

            if (mano.CartaMaquinaEnMesa != null)
            {
                mano.Humano.Jugadas.Add(cartaHumano);
                var cartaMaquina            = mano.CartaMaquinaEnMesa;
                mano.CartaMaquinaEnMesa     = null;
                MaquinaServicio.ResolverBazaJugada(mano, cartaHumano, cartaMaquina);
            }
            else if (MaquinaServicio.EsModoHistoriaPasoAPaso(mano))
            {
                mano.CartaHumanoEnMesa = cartaHumano;
                mano.TurnoActual         = "Maquina";
            }
            else
            {
                mano.Humano.Jugadas.Add(cartaHumano);
                var cartaMaquina = MaquinaServicio.ElegirCarta(mano.Maquina.Mano, cartaHumano);
                mano.Maquina.Mano.Remove(cartaMaquina);
                mano.Maquina.Jugadas.Add(cartaMaquina);
                MaquinaServicio.ResolverBazaJugada(mano, cartaHumano, cartaMaquina);
            }

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
