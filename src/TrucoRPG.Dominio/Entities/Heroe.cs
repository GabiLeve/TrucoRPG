using System;

namespace TrucoRPG.Dominio.Entities
{
    public class Heroe
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nombre { get; set; } = string.Empty;

        public string DescripcionHabilidadPasiva { get; set; } = string.Empty;

        public string DescripcionHabilidadActiva { get; set; } = string.Empty;

        public ClaseHeroe TipoHeroe { get; set; }
    }
}
