using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.DTOs;
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

            var tiendaAgrupada = items
                .GroupBy(items => items.Categoria.ToUpper())
                .Select(grupo => new CategoriaTiendaDto
                {
                    Categoria = grupo.Key,
                    Objetos = grupo.Select(o => new ObjetoTiendaDto {
                        Id = o.Id,
                        Nombre = o.Nombre,
                        Descripcion = o.Descripcion,
                        Precio = o.Precio,
                        Img = o.Img,
                        SpriteKey = o.SpriteKey
                    }).ToList()
                })
                .ToList();

            return tiendaAgrupada;
        }
    }
}
