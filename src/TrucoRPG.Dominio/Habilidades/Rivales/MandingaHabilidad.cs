using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.Habilidades.Rivales
{
    public class MandingaHabilidad : HabilidadRivalBase
    {
        public override TipoHabilidadRival Tipo => TipoHabilidadRival.MandingaFases;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            if (evento != EventoPartida.ManoIniciada)
                return;

            MandingaServicio.OnManoIniciada(contexto.Mano);
        }
    }
}
