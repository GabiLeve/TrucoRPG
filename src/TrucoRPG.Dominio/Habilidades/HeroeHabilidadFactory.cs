using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades.Heroes;

namespace TrucoRPG.Dominio.Habilidades
{
    public static class HeroeHabilidadFactory
    {
        public static IHabilidadHeroe Crear(ClaseHeroe tipo) => tipo switch
        {
            ClaseHeroe.Manipulador => new ManipuladorHabilidad(),
            ClaseHeroe.Timbero     => new TimberoHabilidad(),
            ClaseHeroe.Fanfarron   => new FanfarronHabilidad(),
            ClaseHeroe.Mentiroso   => new MentirosoHabilidad(),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Héroe no soportado.")
        };
    }
}
