namespace TrucoRPG.Dominio.Habilidades
{
    public class EstadoHabilidadesPartida
    {
        private readonly Dictionary<string, EstadoHabilidadesJugador> _porJugador = new();

        public IReadOnlyDictionary<string, EstadoHabilidadesJugador> PorJugador => _porJugador;

        public EstadoHabilidadesJugador ObtenerOCrear(string idJugador, Entities.ClaseHeroe? claseHeroe = null)
        {
            if (!_porJugador.TryGetValue(idJugador, out var estado))
            {
                estado = new EstadoHabilidadesJugador { IdJugador = idJugador, ClaseHeroe = claseHeroe };
                _porJugador[idJugador] = estado;
            }
            else if (claseHeroe.HasValue)
            {
                estado.ClaseHeroe = claseHeroe;
            }

            return estado;
        }

        public EstadoHabilidadesJugador? Obtener(string idJugador) =>
            _porJugador.TryGetValue(idJugador, out var e) ? e : null;
    }
}
