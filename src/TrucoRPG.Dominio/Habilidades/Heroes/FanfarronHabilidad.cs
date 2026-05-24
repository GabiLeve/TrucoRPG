using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Heroes
{
    public sealed class FanfarronHabilidad : HabilidadHeroeBase
    {
        public override ClaseHeroe Tipo => ClaseHeroe.Fanfarron;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            if (evento != EventoPartida.EmpateEnvido || datos is not ResolucionEmpateEnvido resolucion)
                return;

            resolucion.GanadorFinal = IdJugador.Humano;
            contexto.Mano.UltimoMensajeHabilidad =
                "Fanfarrón gana el empate de envido automáticamente.";
        }
    }
}
