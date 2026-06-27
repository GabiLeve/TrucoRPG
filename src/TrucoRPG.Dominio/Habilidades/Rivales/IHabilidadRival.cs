using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Rivales
{
    public interface IHabilidadRival
    {
        TipoHabilidadRival Tipo { get; }

        void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null);
    }
}
