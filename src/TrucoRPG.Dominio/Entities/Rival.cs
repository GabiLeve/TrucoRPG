namespace TrucoRPG.Dominio.Entities
{
    public class Rival
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int Nivel { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public string NombreHabilidad { get; set; } = string.Empty;

        public string DescripcionHabilidad { get; set; } = string.Empty;

        public ClaseRival TipoRival { get; set; }

        public TipoHabilidadRival TipoHabilidad { get; set; }
    }
}
