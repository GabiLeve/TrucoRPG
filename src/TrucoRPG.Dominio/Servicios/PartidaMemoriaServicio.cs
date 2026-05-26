using System.Collections.Concurrent;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class PartidaMemoriaServicio
    {
        private static readonly ConcurrentDictionary<Guid, ManoTruco> _partidas = new();

        public static void Guardar(ManoTruco mano) =>
            _partidas[mano.Id] = mano;

        public static ManoTruco? Obtener(Guid id)
        {
            _partidas.TryGetValue(id, out var mano);
            return mano;
        }

        public static void Actualizar(ManoTruco mano) =>
            _partidas[mano.Id] = mano;
    }
}
