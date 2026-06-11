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

            var cartaHumano = mano.Humano.Mano
                .FirstOrDefault(c => c.Numero == numero &&
                                     c.Palo.Equals(palo, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("La carta no está en tu mano.");

            mano.Humano.Mano.Remove(cartaHumano);
            mano.Humano.Jugadas.Add(cartaHumano);

            // Si la máquina ya tenía una carta en la mesa, fue ella quien abrió esta baza.
            bool maquinaAbrioLaBaza = mano.CartaMaquinaEnMesa != null;

            Carta cartaMaquina;
            if (mano.CartaMaquinaEnMesa != null)
            {
                cartaMaquina            = mano.CartaMaquinaEnMesa;
                mano.CartaMaquinaEnMesa = null;
            }
            else
            {
                cartaMaquina = MaquinaServicio.ElegirCarta(mano.Maquina.Mano, cartaHumano);
                mano.Maquina.Mano.Remove(cartaMaquina);
                mano.Maquina.Jugadas.Add(cartaMaquina);
            }

            var ganadorBaza = JuegoServicio.ResolverBaza(cartaHumano, cartaMaquina);
            mano.Bazas.Add(new Baza
            {
                CartaJugador = cartaHumano,
                CartaMaquina = cartaMaquina,
                Ganador      = ganadorBaza
            });

            // Si la baza fue parda, vuelve a salir quien ABRIÓ esa baza (no siempre el mano
            // de la mano: en la 2da/3ra baza puede haber salido el que ganó la anterior).
            mano.TurnoActual = ganadorBaza == "Parda"
                ? (maquinaAbrioLaBaza ? "Maquina" : "Humano")
                : ganadorBaza;
            mano.GanadorMano = JuegoServicio.ResolverGanadorMano(mano.Bazas, mano.ManoIniciadaPor);

            if (mano.GanadorMano is "Humano" or "Maquina")
            {
                if (!mano.TrucoCantado)
                    mano.EstadoTruco = "No se cantó truco. La mano vale 1 punto.";

                int puntosMano = mano.PuntosTrucoMano > 0 ? mano.PuntosTrucoMano : 1;
                JuegoServicio.SumarPuntos(
                    mano, mano.GanadorMano, puntosMano, OrigenPuntos.TrucoMano, mano.CantorTruco);
                mano.TrucoResuelto = true;
            }
            else
            {
                MaquinaServicio.AvanzarTurno(mano);
            }

            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }
    }
}
