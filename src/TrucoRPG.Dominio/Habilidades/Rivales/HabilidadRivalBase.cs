using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Rivales
{
    public abstract class HabilidadRivalBase : IHabilidadRival
    {
        public abstract TipoHabilidadRival Tipo { get; }

        public virtual void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null) { }
    }
}
