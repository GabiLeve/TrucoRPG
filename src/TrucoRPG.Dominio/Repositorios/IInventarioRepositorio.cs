using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Repositorios
{
    public interface IInventarioRepositorio
    {
        Task<bool> ItemExistente(string usuarioId, int itemTiendaId);
        Task<bool> Agregar(string usuarioId, int itemTiendaId, int cantidad);
        Task<bool> Eliminar(string usuarioId, int itemTiendaId);
        Task<List<Inventario>> ObtenerInventarioDeUsuario(string usuarioId);
    }
}
