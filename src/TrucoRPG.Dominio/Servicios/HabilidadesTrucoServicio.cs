using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class HabilidadesTrucoServicio
    {
        public static void NotificarTrucoAceptado(ManoTruco mano, string cantorId) =>
            HabilidadesOrquestador.Disparar(
                mano,
                EventoPartida.TrucoAceptado,
                new CantoTrucoAceptado { CantorId = cantorId });
    }
}
