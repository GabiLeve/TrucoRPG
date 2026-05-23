using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class PartidaMemoriaServicio
    {
        private static readonly Dictionary<Guid, ManoTruco> _partidas = new();

        public static void Guardar(ManoTruco mano)
        {
            _partidas[mano.Id] = mano;
        }

        public static ManoTruco? Obtener(Guid id)
        {
            if (_partidas.TryGetValue(id, out var mano))
                return mano;

            return null;
        }

        public static void Actualizar(ManoTruco mano)
        {
            _partidas[mano.Id] = mano;
        }
    }
}
