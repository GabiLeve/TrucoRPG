using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Heroes
{
    /// <summary>
    /// Implementación vacía; cada héroe sobreescribe solo los eventos que necesita.
    /// </summary>
    public abstract class HabilidadHeroeBase : IHabilidadHeroe
    {
        public abstract ClaseHeroe Tipo { get; }

        public virtual void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null) { }

        public virtual ResultadoActivarHabilidad? IntentarActivar(
            ContextoPartida contexto,
            SolicitudActivarHabilidad solicitud) =>
            ResultadoActivarHabilidad.Error("La habilidad activa aún no está implementada.");
    }
}
