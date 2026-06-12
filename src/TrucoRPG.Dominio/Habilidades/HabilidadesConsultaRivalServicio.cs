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
                UltimoMensajeHabilidad = mano.UltimoMensajeHabilidadRival
            };
    }
}
