using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrucoRPG.Dominio.Entities
{
    public class CategoriaRegla
    {
        public string Categoria { get; set; } = string.Empty;
        public List<ReglasDetalle> Detalle { get; set; } = new List<ReglasDetalle>();
    }
}
