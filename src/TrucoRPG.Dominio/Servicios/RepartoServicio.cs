

using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class RepartoServicio
    {
        public static void Repartir(ManoTruco mano)
        {
            var contexto = mano.RepartoContext ?? new RepartoContext();
            var random = contexto.Random ?? new Random();

            var resto = MazoServicio.CrearMazo()
                .OrderBy(_ => random.Next())
                .ToList();

            mano.Humano.Mano = RepartirAlJugador(resto, 3, IdJugador.Humano, contexto, random);
            mano.Maquina.Mano = RepartirAlJugador(resto, 3, IdJugador.Maquina, contexto, random);
            mano.CartasRestantesMazo = resto;
            mano.RepartoContext = null;
        }

        private static List<Carta> RepartirAlJugador(
            List<Carta> resto,
            int cantidad,
            string idJugador,
            RepartoContext contexto,
            Random random)
        {
            var mano = new List<Carta>(cantidad);
            var probMejorar = contexto.ObtenerProbMejorar(idJugador);

            for (int i = 0; i < cantidad; i++)
            {
                if (resto.Count == 0)
                    break;

                var carta = resto[0];
                resto.RemoveAt(0);

                if (probMejorar > 0 && random.NextDouble() < probMejorar)
                    carta = IntentarMejorarCarta(carta, resto, random);

                mano.Add(carta);
            }

            return mano;
        }

        private static Carta IntentarMejorarCarta(Carta cartaActual, List<Carta> resto, Random random)
        {
            var mejores = resto
                .Where(c => c.ValorTruco > cartaActual.ValorTruco)
                .ToList();

            if (mejores.Count == 0)
                return cartaActual;

            var elegida = mejores[random.Next(mejores.Count)];
            resto.Remove(elegida);
            resto.Add(cartaActual);
            return elegida;
        }
    }
}
