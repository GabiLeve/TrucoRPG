using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Rivales
{
    public class NingunaHabilidad : HabilidadRivalBase
    {
        public override TipoHabilidadRival Tipo => TipoHabilidadRival.Ninguna;
    }
}
