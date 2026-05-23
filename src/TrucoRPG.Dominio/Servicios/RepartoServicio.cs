

using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class RepartoServicio
    {
        public static void Repartir(ManoTruco mano)
        {
            var mazo = MazoServicio.CrearMazo();
            var random = new Random();

            mazo = mazo.OrderBy(x => random.Next()).ToList();

            mano.Humano.Mano = mazo.Take(3).ToList();
            mano.Maquina.Mano = mazo.Skip(3).Take(3).ToList();
        }
    }
}
