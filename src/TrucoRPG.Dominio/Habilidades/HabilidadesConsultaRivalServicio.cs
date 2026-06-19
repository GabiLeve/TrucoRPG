using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    public static class HabilidadesConsultaRivalServicio
    {
        public static VistaHabilidadesRival ObtenerVista(ManoTruco mano) =>
            new()
            {
                HabilidadesActivasEnPartida = mano.Configuracion.HabilidadesRivalActivas,
                SalpicaduraActiva = mano.SalpicaduraActiva,
                SalpicaduraBloqueando = mano.SalpicaduraBloqueando,
                TravesuraActiva = mano.TravesuraActiva,
                TravesuraBloqueando = mano.TravesuraBloqueando,
                RasgunoActivo = mano.RasgunoActivo,
                RasgunoBloqueando = mano.RasgunoBloqueando,
                CartasOcultasTravesura = mano.CartasOcultasTravesura
                    .Select(c => new CartaReferencia { Numero = c.Numero, Palo = c.Palo })
                    .ToList(),
                UltimoMensajeHabilidad = mano.UltimoMensajeHabilidadRival
            };
    }
}
