using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace TrucoRPG.Dominio.DTOs
{
    [ExcludeFromCodeCoverage]
    public class ObjetoTiendaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Precio { get; set; }
        public string Img { get; set; } = string.Empty;
        public string? SpriteKey { get; set; }
        public bool Acumulable { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CategoriaTiendaDto
    {
        public string Categoria { get; set; } = string.Empty;
        public List<ObjetoTiendaDto> Objetos { get; set; } = new();
    }
}
