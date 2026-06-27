using System.Collections.Concurrent;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class Truco2v2MemoriaServicio
    {
        private static readonly ConcurrentDictionary<Guid, ManoTruco2v2> _partidas = new();

        public static void Guardar(ManoTruco2v2 mano)    => _partidas[mano.Id] = mano;
        public static void Actualizar(ManoTruco2v2 mano) => _partidas[mano.Id] = mano;

        public static ManoTruco2v2? Obtener(Guid id)
        {
            _partidas.TryGetValue(id, out var m);
            return m;
        }
    }
}
