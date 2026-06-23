using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class RasgunoServicio
    {
        private static readonly Lazy<List<Carta>> MazoCompleto = new(MazoServicio.CrearMazo);

        public static void DebilitarCartaAleatoria(ManoTruco mano)
        {
            var manoHumano = mano.Humano.Mano;

            var candidatos = Enumerable.Range(0, manoHumano.Count)
                .Where(i => manoHumano[i].ValorTruco > 1)
                .Select(i => (Indice: i, Opciones: ObtenerReemplazosValidos(manoHumano, i)))
                .Where(x => x.Opciones.Count > 0)
                .ToList();

            if (candidatos.Count == 0)
                return;

            var elegido = candidatos[Random.Shared.Next(candidatos.Count)];
            var carta = manoHumano[elegido.Indice];
            var reemplazo = elegido.Opciones[Random.Shared.Next(elegido.Opciones.Count)];

            carta.Numero = reemplazo.Numero;
            carta.Palo = reemplazo.Palo;
            carta.ValorTruco = reemplazo.ValorTruco;
        }

        private static List<Carta> ObtenerReemplazosValidos(List<Carta> manoHumano, int indiceCarta)
        {
            var carta = manoHumano[indiceCarta];
            var valorObjetivo = carta.ValorTruco - 1;

            return MazoCompleto.Value
                .Where(c => c.ValorTruco == valorObjetivo)
                .Where(c => !EstaEnMano(manoHumano, c, excluirIndice: indiceCarta))
                .ToList();
        }

        private static bool EstaEnMano(List<Carta> manoHumano, Carta candidata, int excluirIndice)
        {
            for (var i = 0; i < manoHumano.Count; i++)
            {
                if (i == excluirIndice)
                    continue;

                var enMano = manoHumano[i];
                if (enMano.Numero == candidata.Numero
                    && enMano.Palo.Equals(candidata.Palo, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
