using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class CartasEnJuegoServicio
    {
        public static (int Numero, string Palo) Clave(int numero, string palo) =>
            (numero, palo.ToLowerInvariant());

        public static (int Numero, string Palo) Clave(Carta carta) =>
            Clave(carta.Numero, carta.Palo);

        /// <summary>
        /// Cartas en la mano (ambos jugadores), bazas y mesa
        /// <paramref name="excluir"/> 
        /// </summary>
        public static HashSet<(int Numero, string Palo)> Obtener(ManoTruco mano, Carta? excluir)
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
