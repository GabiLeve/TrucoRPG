using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Rivales
{
    public class RasgunoHabilidad : HabilidadRivalBase
    {
        public override TipoHabilidadRival Tipo => TipoHabilidadRival.Rasguno;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            if (evento != EventoPartida.ManoIniciada)
                return;

            if (contexto.Mano.NumeroDeMano % 2 != 1)
                return;

            contexto.Mano.RasgunoActivo = true;
            contexto.Mano.RasgunoBloqueando = true;
            contexto.Mano.UltimoMensajeHabilidadRival =
                "¡Rasguño! El Lobizón va a debilitar 1 de tus cartas...";
        }
    }
}
