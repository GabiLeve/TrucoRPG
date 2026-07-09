using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class SalpicaduraServicio
    {
        private const int CartasAReemplazar = 2;
        private static readonly string[] Palos = ["Espada", "Basto", "Oro", "Copa"];

        public static void CambiarPaloCarta(Carta carta, ManoTruco mano)
        {
            var ocupadas = ObtenerCartasEnJuego(mano, carta);
            var opciones = Palos
                .Where(p => !p.Equals(carta.Palo, StringComparison.OrdinalIgnoreCase))
                .Where(p => !ocupadas.Contains(Clave(carta.Numero, p)))
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

        private static (int Numero, string Palo) Clave(int numero, string palo) =>
            (numero, palo.ToLowerInvariant());

        private static (int Numero, string Palo) Clave(Carta carta) =>
            Clave(carta.Numero, carta.Palo);

        private static HashSet<(int Numero, string Palo)> ObtenerCartasEnJuego(ManoTruco mano, Carta? excluir)
        {
            var ocupadas = new HashSet<(int, string)>();

            void Agregar(Carta? carta)
            {
                if (carta is null || ReferenceEquals(carta, excluir))
                    return;
                ocupadas.Add(Clave(carta));
            }

            foreach (var carta in mano.Humano.Mano)
                Agregar(carta);
            foreach (var carta in mano.Maquina.Mano)
                Agregar(carta);
            foreach (var baza in mano.Bazas)
            {
                Agregar(baza.CartaJugador);
                Agregar(baza.CartaMaquina);
            }
            Agregar(mano.CartaHumanoEnMesa);
            Agregar(mano.CartaMaquinaEnMesa);

            return ocupadas;
        }
    }
}
