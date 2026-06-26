using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class TravesuraServicio
    {
        private const int CartasAOcultar = 2;

        public static void OcultarCartasHumano(ManoTruco mano)
        {
            var manoHumano = mano.Humano.Mano;
            if (manoHumano.Count < CartasAOcultar)
                return;

            var indices = Enumerable.Range(0, manoHumano.Count)
                .OrderBy(_ => Random.Shared.Next())
                .Take(CartasAOcultar)
                .ToList();

            mano.CartasOcultasTravesura = indices
                .Select(i => new Carta
                {
                    Numero = manoHumano[i].Numero,
                    Palo   = manoHumano[i].Palo
                })
                .ToList();
        }
    }
}
