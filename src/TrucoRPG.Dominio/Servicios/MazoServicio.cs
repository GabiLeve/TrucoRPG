using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class MazoServicio
    {
        public static List<Carta> CrearMazo()
        {
            var palos = new[] { "Espada", "Basto", "Oro", "Copa" };
            var numeros = new[] { 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 };

            var mazo = new List<Carta>();

            foreach (var palo in palos)
            {
                foreach (var numero in numeros)
                {
                    mazo.Add(new Carta
                    {
                        Palo = palo,
                        Numero = numero,
                        ValorTruco = ObtenerValorTruco(numero, palo)
                    });
                }
            }

            return mazo;
        }

        private static int ObtenerValorTruco(int numero, string palo)
        {
            if (numero == 1 && palo == "Espada") return 14;
            if (numero == 1 && palo == "Basto") return 13;
            if (numero == 7 && palo == "Espada") return 12;
            if (numero == 7 && palo == "Oro") return 11;
            if (numero == 3) return 10;
            if (numero == 2) return 9;
            if (numero == 1) return 8;
            if (numero == 12) return 7;
            if (numero == 11) return 6;
            if (numero == 10) return 5;
            if (numero == 7) return 4;
            if (numero == 6) return 3;
            if (numero == 5) return 2;
            if (numero == 4) return 1;

            return 0;
        }
    }
}
