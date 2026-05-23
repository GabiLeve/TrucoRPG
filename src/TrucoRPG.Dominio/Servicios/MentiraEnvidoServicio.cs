namespace TrucoRPG.Dominio.Servicios
{
    public static class MentiraEnvidoServicio
    {
        private static readonly Random _random = new Random();

        public static int ObtenerTantoCantado(int tantoReal, int nivelMentira, out bool mintio)
        {
            mintio = false;

            nivelMentira = Math.Clamp(nivelMentira, 0, 100);

            int tantoBase = NormalizarTantoCantado(tantoReal);

            if (nivelMentira == 0)
                return tantoBase;

            int probabilidadMentir = nivelMentira;
            bool vaAMentir = _random.Next(1, 101) <= probabilidadMentir;

            if (!vaAMentir)
                return tantoBase;

            var tantosValidos = ObtenerTantosValidos();

            int maxIncremento = ObtenerMaxIncremento(nivelMentira);
            int maxPermitido = Math.Min(33, tantoBase + maxIncremento);

            var candidatos = tantosValidos
                .Where(t => t != tantoBase && t >= tantoBase && t <= maxPermitido)
                .ToList();

            if (!candidatos.Any())
            {
                candidatos = tantosValidos
                    .Where(t => t != tantoBase)
                    .ToList();
            }

            if (!candidatos.Any())
                return tantoBase;

            int tantoCantado = candidatos[_random.Next(candidatos.Count)];

            mintio = EsMentiraReal(tantoReal, tantoCantado);

            return tantoCantado;
        }

        private static bool EsMentiraReal(int tantoReal, int tantoCantado)
        {
            int real = NormalizarTantoCantado(tantoReal);
            int cantado = NormalizarTantoCantado(tantoCantado);

            if (real < 20 && cantado >= 20)
                return true;

            if (real >= 20 && cantado >= 20)
                return false;

            if (real < 20 && cantado < 20)
                return false;

            return false;
        }

        private static int NormalizarTantoCantado(int tanto)
        {

            if (tanto >= 8 && tanto <= 19)
                return 7;

            return Math.Clamp(tanto, 0, 33);
        }

        private static List<int> ObtenerTantosValidos()
        {
            var lista = new List<int>();

            for (int i = 0; i <= 7; i++)
                lista.Add(i);

            for (int i = 20; i <= 33; i++)
                lista.Add(i);

            return lista;
        }

        private static int ObtenerMaxIncremento(int nivelMentira)
        {
            if (nivelMentira <= 10) return 1;
            if (nivelMentira <= 20) return 2;
            if (nivelMentira <= 30) return 3;
            if (nivelMentira <= 40) return 4;
            if (nivelMentira <= 50) return 5;
            if (nivelMentira <= 60) return 6;
            if (nivelMentira <= 70) return 7;
            if (nivelMentira <= 80) return 8;
            if (nivelMentira <= 90) return 10;
            return 13;
        }
    }
}
