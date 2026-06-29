using System.Collections.Generic;
using System.Threading.Tasks;
using TrucoRPG.Dominio.DTOs;
using TrucoRPG.Dominio.Mapeos;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerTiendaUseCase
    {
        private readonly IItemTiendaRepositorio _repositorio;

        public ObtenerTiendaUseCase(IItemTiendaRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<CategoriaTiendaDto>> EjecutarAsync()
        {
            var items = await _repositorio.ObtenerTodosLosItemsAsync();
            return items.ToCategoriasDto();
        }
    }
}
