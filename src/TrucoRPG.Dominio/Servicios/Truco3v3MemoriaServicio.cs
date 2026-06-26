using System.Collections.Concurrent;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>Almacén en memoria de las manos 3v3 del modo solitario (espejo del 2v2).</summary>
    public static class Truco3v3MemoriaServicio
    {
        private static readonly ConcurrentDictionary<Guid, ManoTruco3v3> _partidas = new();

        public static void Guardar(ManoTruco3v3 mano)    => _partidas[mano.Id] = mano;
        public static void Actualizar(ManoTruco3v3 mano) => _partidas[mano.Id] = mano;

        public static ManoTruco3v3? Obtener(Guid id)
        {
            _partidas.TryGetValue(id, out var m);
            return m;
        }
    }
}
