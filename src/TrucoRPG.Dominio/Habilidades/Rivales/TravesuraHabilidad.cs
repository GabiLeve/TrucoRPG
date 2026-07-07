using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Rivales
{
    public class TravesuraHabilidad : HabilidadRivalBase
    {
        public override TipoHabilidadRival Tipo => TipoHabilidadRival.Travesura;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            if (evento != EventoPartida.ManoIniciada)
                return;

            if (contexto.Mano.NumeroDeMano % 2 != 1)
                return;

            contexto.Mano.TravesuraActiva = true;
            contexto.Mano.TravesuraBloqueando = true;
            contexto.Mano.CartasOcultasTravesura = [];
            contexto.Mano.UltimoMensajeHabilidadRival =
                "¡Travesura! El Pomberito va a ocultar 2 de tus cartas...";
        }
    }
}
