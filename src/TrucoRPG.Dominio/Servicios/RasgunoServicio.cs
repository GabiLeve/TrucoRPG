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
                .Select(i => (Indice: i, Opciones: ObtenerReemplazosValidos(mano, i)))
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

        private static List<Carta> ObtenerReemplazosValidos(ManoTruco mano, int indiceCarta)
        {
            var carta = mano.Humano.Mano[indiceCarta];
            var valorObjetivo = carta.ValorTruco - 1;
            var ocupadas = CartasEnJuegoServicio.Obtener(mano, carta);

            return MazoCompleto.Value
                .Where(c => c.ValorTruco == valorObjetivo)
                .Where(c => !ocupadas.Contains(CartasEnJuegoServicio.Clave(c)))
                .ToList();
        }
    }
}
