using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades.Rivales;

namespace TrucoRPG.Dominio.Habilidades
{
    public static class RivalHabilidadFactory
    {
        public static IHabilidadRival Crear(TipoHabilidadRival tipo) => tipo switch
        {
            TipoHabilidadRival.Ninguna      => new NingunaHabilidad(),
            TipoHabilidadRival.Salpicadura  => new SalpicaduraHabilidad(),
            TipoHabilidadRival.Travesura    => new TravesuraHabilidad(),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Habilidad de rival no implementada.")
        };

        public static IHabilidadRival CrearDesdeRival(ClaseRival claseRival) => claseRival switch
        {
            ClaseRival.Nahuelito => Crear(TipoHabilidadRival.Salpicadura),
            ClaseRival.Pomberito => Crear(TipoHabilidadRival.Travesura),
            _ => throw new ArgumentOutOfRangeException(nameof(claseRival), claseRival, "Rival no implementado.")
        };
    }
}
