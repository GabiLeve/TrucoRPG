using TrucoDemo.Clases;

namespace TrucoDemo.Servicios
{
    public static class IniciativaMaquinaTrucoServicio
    {
        private static readonly Random _random = new Random();

        public static bool DebeCantarTruco(List<Carta> manoMaquina, int nivelMentira)
        {
            nivelMentira = Math.Clamp(nivelMentira, 0, 100);
            int cartaMasFuerte = manoMaquina.Max(c => c.ValorTruco);

            int probabilidad = cartaMasFuerte switch
            {
                >= 13 => 78,
                12 => 66,
                11 => 54,
                10 => 42,
                9 => 34,
                8 => 26,
                7 => 20,
                _ => 14
            };

            probabilidad += (int)Math.Round(nivelMentira * 0.62);

            if (nivelMentira >= 90)
                probabilidad += 12;

            probabilidad = Math.Clamp(probabilidad, 0, 100);

            int tirada = _random.Next(1, 101);
            return tirada <= probabilidad;
        }
    }
}
