using TrucoDemo.Clases;

namespace TrucoDemo.Servicios
{
    public static class MaquinaServicio
    {
        public static Carta ElegirCarta(List<Carta> manoMaquina, Carta? cartaHumano)
        {
            if (cartaHumano == null)
            {
                return manoMaquina
                    .OrderBy(c => c.ValorTruco)
                    .ElementAt(manoMaquina.Count / 2);
            }

            var cartasQueGanan = manoMaquina
                .Where(c => c.ValorTruco > cartaHumano.ValorTruco)
                .OrderBy(c => c.ValorTruco)
                .ToList();

            if (cartasQueGanan.Any())
                return cartasQueGanan.First();

            return manoMaquina.OrderBy(c => c.ValorTruco).First();
        }
    }
}
