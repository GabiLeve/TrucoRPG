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
            if (mano.RasgunoBloqueando)
                throw new InvalidOperationException("Esperá a que el rival termine Rasguño.");
            if (mano.AullidoBloqueando)
                throw new InvalidOperationException("Esperá a que el Lobizón termine el Aullido.");
            if (mano.DestelloBloqueando)
                throw new InvalidOperationException("Esperá a que la Luz Mala termine el Destello.");
            if (mano.EspejismoBloqueando)
                throw new InvalidOperationException("Esperá a que la Luz Mala termine el Espejismo.");
            if (mano.MandingaEspejoBloqueando)
                throw new InvalidOperationException("Esperá a que el Mandinga termine El Espejo.");
            if (mano.MandingaEnganoBloqueando)
                throw new InvalidOperationException("Esperá a que el Mandinga termine El Engaño.");
            if (mano.MandingaMaldicionBloqueando)
                throw new InvalidOperationException("Esperá a que el Mandinga termine El Pacto.");
        }
    }
}
