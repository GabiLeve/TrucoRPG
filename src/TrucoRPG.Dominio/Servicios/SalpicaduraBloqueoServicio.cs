using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class SalpicaduraBloqueoServicio
    {
        public static void ValidarNoBloqueado(ManoTruco mano)
        {
            if (mano.SalpicaduraBloqueando)
                throw new InvalidOperationException("Esperá a que el rival termine Salpicadura.");
            if (mano.TravesuraBloqueando)
                throw new InvalidOperationException("Esperá a que el rival termine Travesura.");
        }
    }
}
