using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    /// <summary>
    /// Estrategia de habilidades por héroe. Una implementación por ClaseHeroe.
    /// </summary>
    public interface IHabilidadHeroe
    {
        ClaseHeroe Tipo { get; }

        void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null);

        ResultadoActivarHabilidad? IntentarActivar(
            ContextoPartida contexto,
            SolicitudActivarHabilidad solicitud);
    }
}
