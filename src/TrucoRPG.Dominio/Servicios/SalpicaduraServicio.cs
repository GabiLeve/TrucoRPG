using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class SalpicaduraServicio
    {
        private const int CartasAReemplazar = 2;
        private static readonly string[] Palos = ["Espada", "Basto", "Oro", "Copa"];

        public static void CambiarPaloCarta(Carta carta, ManoTruco mano)
        {
            var ocupadas = CartasEnJuegoServicio.Obtener(mano, carta);
            var opciones = Palos
                .Where(p => !p.Equals(carta.Palo, StringComparison.OrdinalIgnoreCase))
                .Where(p => !ocupadas.Contains(CartasEnJuegoServicio.Clave(carta.Numero, p)))
                .ToArray();
            if (opciones.Length == 0)
                return;

            var nuevoPalo = opciones[Random.Shared.Next(opciones.Length)];
            carta.Palo = nuevoPalo;
            carta.ValorTruco = MazoServicio.ObtenerValorTruco(carta.Numero, nuevoPalo);
        }

        public static void ReemplazarCartasHumano(ManoTruco mano)
        {
            var manoHumano = mano.Humano.Mano;
            if (manoHumano.Count < CartasAReemplazar)
                return;

            var indices = Enumerable.Range(0, manoHumano.Count)
                .OrderBy(_ => Random.Shared.Next())
                .Take(CartasAReemplazar)
                .ToList();

            foreach (var idx in indices)
            {
                var carta = manoHumano[idx];
                CambiarPaloCarta(carta, mano);
            }
        }
    }
}
