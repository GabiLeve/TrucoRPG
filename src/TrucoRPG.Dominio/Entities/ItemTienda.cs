using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrucoRPG.Dominio.Entities
{
    public class ItemTienda
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Precio { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Img { get; set; } = string.Empty;
        public string? SpriteKey { get; set; }
        public bool Acumulable { get; set; }
    }
}
