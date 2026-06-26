using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Rivales
{
    public class SalpicaduraHabilidad : HabilidadRivalBase
    {
        public override TipoHabilidadRival Tipo => TipoHabilidadRival.Salpicadura;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            if (evento != EventoPartida.ManoIniciada)
                return;

            if (contexto.Mano.NumeroDeMano % 2 != 1)
                return;

            contexto.Mano.SalpicaduraActiva = true;
            contexto.Mano.SalpicaduraBloqueando = true;
            contexto.Mano.UltimoMensajeHabilidadRival =
                "¡Salpicadura! Nahuelito va a cambiar el palo de 2 de tus cartas...";
        }
    }
}
