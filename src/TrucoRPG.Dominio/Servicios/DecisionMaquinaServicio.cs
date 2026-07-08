using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class DecisionMaquinaServicio
    {
        private static readonly Random _random = new Random();

        // Seam para tests (mismo patrón que MaquinaServicio2v2/3v3):
        // devuelve un valor en [0, max). En producción usa Random.
        public static Func<int, int> RandomNext = (max) => _random.Next(max);

        public static bool AceptarEnvido(List<Carta> manoMaquina, int nivelMentira)
        {
            int tanto = EnvidoServicio.CalcularTanto(manoMaquina);
            nivelMentira = Math.Clamp(nivelMentira, 0, 100);

            if (tanto >= 30) return true;
            if (tanto <= 20 && nivelMentira == 0) return false;

            int probabilidadAceptar = tanto switch
            {
                >= 29 => 95,
                28 => 85,
                27 => 75,
                26 => 65,
                25 => 55,
                24 => 45,
                23 => 35,
                22 => 25,
                21 => 18,
                20 => 12,
                19 => 8,
                _ => 5
            };

            int bonusPorMentira = (int)Math.Round(nivelMentira * 0.55);

            probabilidadAceptar += bonusPorMentira;

            if (nivelMentira >= 80)
                probabilidadAceptar += 20;

            if (nivelMentira >= 95)
                probabilidadAceptar += 20;

            probabilidadAceptar = Math.Clamp(probabilidadAceptar, 0, 100);

            int tirada = RandomNext(100) + 1;
            return tirada <= probabilidadAceptar;
        }

        public static bool AceptarTruco(List<Carta> manoMaquina, int nivelMentira)
        {
            nivelMentira = Math.Clamp(nivelMentira, 0, 100);
            int cartaMasFuerte = manoMaquina.Any() ? manoMaquina.Max(c => c.ValorTruco) : 0;

            int probabilidadAceptar = cartaMasFuerte switch
            {
                >= 11 => 90,
                10 => 80,
                9 => 68,
                8 => 58,
                7 => 45,
                6 => 35,
                5 => 28,
                _ => 20
            };

            int bonusPorCaradurez = (int)Math.Round(nivelMentira * 0.35);
            probabilidadAceptar = Math.Clamp(probabilidadAceptar + bonusPorCaradurez, 0, 100);

            int tirada = RandomNext(100) + 1;
            return tirada <= probabilidadAceptar;
        }

        public static bool EscalarTruco(List<Carta> manoMaquina, int nivelMentira, int nivelActual)
        {
            if (nivelActual >= 3) return false;

            nivelMentira = Math.Clamp(nivelMentira, 0, 100);
            int cartaMasFuerte = manoMaquina.Any() ? manoMaquina.Max(c => c.ValorTruco) : 0;

            int probabilidad = cartaMasFuerte switch
            {
                >= 13 => 55,
                12 => 42,
                11 => 30,
                10 => 22,
                9 => 15,
                8 => 10,
                _ => 6
            };

            if (nivelActual == 2)
                probabilidad = (int)(probabilidad * 0.65);

            probabilidad += (int)Math.Round(nivelMentira * 0.28);
            probabilidad = Math.Clamp(probabilidad, 0, 100);

            return RandomNext(100) + 1 <= probabilidad;
        }
    }
}
