using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Infraestructura.Data;

namespace TrucoRPG.Infraestructura.Repositorios
{
    public class InventarioRepositorio : IInventarioRepositorio
    {
        private readonly AppDbContext _context;

        public InventarioRepositorio(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ItemExistente(string usuarioId, int itemTiendaId)
        {
            return await _context.Inventarios
                .AnyAsync(i => i.UsuarioId == usuarioId && i.ItemTiendaId == itemTiendaId);
        }

        public async Task<bool> Agregar(string usuarioId, int itemTiendaId, int cantidad)
        {
            var itemExistente = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.UsuarioId == usuarioId && i.ItemTiendaId == itemTiendaId);

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                _context.Inventarios.Add(new Inventario
                {
                    UsuarioId = usuarioId,
                    ItemTiendaId = itemTiendaId,
                    Cantidad = cantidad,
                    Equipado = false
                });
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> Eliminar(string usuarioId, int itemTiendaId)
        {
            var item = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.UsuarioId == usuarioId && i.ItemTiendaId == itemTiendaId);

            if (item == null) return false;

            _context.Inventarios.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Inventario>> ObtenerInventarioDeUsuario(string usuarioId)
        {
            return await _context.Inventarios
                .Include(i => i.ItemTienda)
                .Where(i => i.UsuarioId == usuarioId)
                .ToListAsync();
        }
    }
}
