using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades.Rivales;

namespace TrucoRPG.Dominio.Habilidades
{
    public static class RivalHabilidadFactory
    {
        public static IHabilidadRival Crear(TipoHabilidadRival tipo) => tipo switch
        {
            TipoHabilidadRival.Ninguna       => new NingunaHabilidad(),
            TipoHabilidadRival.Salpicadura   => new SalpicaduraHabilidad(),
            TipoHabilidadRival.Travesura     => new TravesuraHabilidad(),
            TipoHabilidadRival.Rasguno       => new RasgunoHabilidad(),
            TipoHabilidadRival.Destello      => new NingunaHabilidad(),
            TipoHabilidadRival.MandingaFases => new NingunaHabilidad(),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Habilidad de rival no implementada.")
        };

        public static IHabilidadRival CrearDesdeRival(ClaseRival claseRival) => claseRival switch
        {
            ClaseRival.Nahuelito => Crear(TipoHabilidadRival.Salpicadura),
            ClaseRival.Pomberito => Crear(TipoHabilidadRival.Travesura),
            ClaseRival.Lobizon  => Crear(TipoHabilidadRival.Rasguno),
            ClaseRival.LuzMala  => Crear(TipoHabilidadRival.Ninguna),
            ClaseRival.Mandinga => Crear(TipoHabilidadRival.Ninguna),
            _ => throw new ArgumentOutOfRangeException(nameof(claseRival), claseRival, "Rival no implementado.")
        };
    }
}
