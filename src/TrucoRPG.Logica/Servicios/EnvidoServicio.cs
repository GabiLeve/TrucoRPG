using TrucoDemo.Clases;

namespace TrucoDemo.Servicios
{
    public static class EnvidoServicio
    {
        public static int CalcularTanto(List<Carta> cartas)
        {
            var gruposPorPalo = cartas.GroupBy(c => c.Palo).ToList();

            int mejorTanto = 0;

            foreach (var grupo in gruposPorPalo)
            {
                var cartasDelPalo = grupo
                    .Select(c => ValorEnvido(c.Numero))
                    .OrderByDescending(v => v)
                    .ToList();

                if (cartasDelPalo.Count >= 2)
                {
                    int tanto = cartasDelPalo[0] + cartasDelPalo[1] + 20;
                    if (tanto > mejorTanto)
                        mejorTanto = tanto;
                }
            }

            if (mejorTanto > 0)
                return mejorTanto;

            return cartas.Max(c => ValorEnvido(c.Numero));
        }

        private static int ValorEnvido(int numero)
        {
            if (numero >= 10)
                return 0;

            return numero;
        }
    }
}
