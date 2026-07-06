using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrucoRPG.Dominio.Entities
{
    public class Inventario
    {
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        public int ItemTiendaId { get; set; }
        public ItemTienda? ItemTienda { get; set; }

        public int Cantidad { get; set; }

        public bool Equipado { get; set; }
    }
}
