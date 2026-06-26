using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Repositorios;

public interface IItemTiendaRepositorio
{
    Task<List<ItemTienda>> ObtenerTodosLosItemsAsync();
}
