using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Rivales
{
    /// <summary>
    /// La lógica de Destello vive en <see cref="Servicios.DestelloServicio"/>.
    /// Esta clase existe para registrar el tipo de habilidad en el factory.
    /// </summary>
    public class DestelloHabilidad : HabilidadRivalBase
    {
        public override TipoHabilidadRival Tipo => TipoHabilidadRival.Destello;
    }
}
