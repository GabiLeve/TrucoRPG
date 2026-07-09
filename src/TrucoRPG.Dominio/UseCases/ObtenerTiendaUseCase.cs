using System.Collections.Generic;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerTiendaUseCase
    {
        private readonly IItemTiendaRepositorio _repositorio;

        public ObtenerTiendaUseCase(IItemTiendaRepositorio repositorio) =>
            _repositorio = repositorio;

        public Task<List<ItemTienda>> EjecutarAsync() =>
            _repositorio.ObtenerTodosLosItemsAsync();
    }
}
