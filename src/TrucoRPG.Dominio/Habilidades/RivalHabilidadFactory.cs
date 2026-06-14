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
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Habilidad de rival no implementada.")
        };

        public static IHabilidadRival CrearDesdeRival(ClaseRival claseRival) => claseRival switch
        {
            ClaseRival.Nahuelito => Crear(TipoHabilidadRival.Salpicadura),
            ClaseRival.Pomberito => Crear(TipoHabilidadRival.Ninguna),
            _ => throw new ArgumentOutOfRangeException(nameof(claseRival), claseRival, "Rival no implementado.")
        };
    }
}
