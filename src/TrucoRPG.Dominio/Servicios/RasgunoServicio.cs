using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class RasgunoServicio
    {
        private static readonly Lazy<List<Carta>> MazoCompleto = new(MazoServicio.CrearMazo);

        public static void DebilitarCartaAleatoria(ManoTruco mano)
        {
            var manoHumano = mano.Humano.Mano;
            var indicesDebilitables = Enumerable.Range(0, manoHumano.Count)
                .Where(i => manoHumano[i].ValorTruco > 1)
                .ToList();

            if (indicesDebilitables.Count == 0)
                return;

            var idx = indicesDebilitables[Random.Shared.Next(indicesDebilitables.Count)];
            var carta = manoHumano[idx];
            var valorObjetivo = carta.ValorTruco - 1;

            var opciones = MazoCompleto.Value
                .Where(c => c.ValorTruco == valorObjetivo)
                .ToList();

            if (opciones.Count == 0)
                return;

            var reemplazo = opciones[Random.Shared.Next(opciones.Count)];
            carta.Numero = reemplazo.Numero;
            carta.Palo = reemplazo.Palo;
            carta.ValorTruco = reemplazo.ValorTruco;
        }
    }
}
