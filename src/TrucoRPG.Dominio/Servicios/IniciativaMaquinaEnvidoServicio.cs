using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class IniciativaMaquinaEnvidoServicio
    {
        private static readonly Random _random = new Random();

        public static bool DebeCantarEnvido(List<Carta> manoMaquina, int nivelMentira)
        {
            int tanto = EnvidoServicio.CalcularTanto(manoMaquina);
            nivelMentira = Math.Clamp(nivelMentira, 0, 100);

            if (tanto >= 30)
                return true;

            int probabilidad = tanto switch
            {
                >= 29 => 85,
                28 => 75,
                27 => 65,
                26 => 55,
                25 => 45,
                24 => 35,
                23 => 25,
                22 => 18,
                21 => 12,
                20 => 10,
                19 => 8,
                18 => 6,
                _ => 4
            };

            probabilidad += (int)Math.Round(nivelMentira * 0.70);

            if (nivelMentira >= 95)
                probabilidad += 20;

            probabilidad = Math.Clamp(probabilidad, 0, 100);

            int tirada = _random.Next(1, 101);
            return tirada <= probabilidad;
        }
    }
}
