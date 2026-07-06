using Microsoft.EntityFrameworkCore;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Infraestructura.Data;

namespace TrucoRPG.Infraestructura.Repositorios;

public class ItemTiendaRepositorio : IItemTiendaRepositorio
{
    private readonly AppDbContext _context;

    public ItemTiendaRepositorio(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ItemTienda?> ObtenerItemPorIdAsync(int idItemTienda)
    {
        return await _context.Items.FindAsync(idItemTienda);
    }

    public async Task<List<ItemTienda>> ObtenerTodosLosItemsAsync()
    {
        return await _context.Items.ToListAsync();
    }
}
