using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    public interface IHabilidadHeroe
    {
        ClaseHeroe Tipo { get; }

        void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null);

        ResultadoActivarHabilidad? IntentarActivar(
            ContextoPartida contexto,
            SolicitudActivarHabilidad solicitud);
    }
}
