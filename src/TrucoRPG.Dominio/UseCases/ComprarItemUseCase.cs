using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ComprarItemUseCase
    {
        private readonly IInventarioRepositorio _inventarioRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        private readonly IItemTiendaRepositorio _itemTiendaRepositorio;
        public ComprarItemUseCase(IInventarioRepositorio inventarioRepositorio, IUsuarioRepositorio usuarioRepositorio, IItemTiendaRepositorio itemTiendaRepositorio)
        {
            _inventarioRepositorio = inventarioRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _itemTiendaRepositorio = itemTiendaRepositorio;
        }
        public async Task<bool> Ejecutar(string usuarioId, int itemTiendaId)
        {
            var item = await _itemTiendaRepositorio.ObtenerItemPorIdAsync(itemTiendaId);
            if (item == null)
            {
                throw new Exception("El item no existe en la tienda.");
            }

            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new Exception("El usuario no existe.");
            }

            if (!item.Acumulable) {
                bool yaTieneItem = await _inventarioRepositorio.ItemExistente(usuarioId, itemTiendaId);
                if (yaTieneItem) throw new Exception("El usuario ya tiene este item.");
            }

            if(usuario.Monedas < item.Precio)
            {
                throw new Exception("El usuario no tiene suficientes monedas.");
            }

            int restoMonedas = usuario.Monedas - item.Precio;
            bool monedasActualizadas = await _usuarioRepositorio.ActualizarMonedasAsync(usuarioId, restoMonedas);

            if (!monedasActualizadas)
            {
                throw new Exception("No se puedo hacer la compra");
            }

            return await _inventarioRepositorio.Agregar(usuarioId, itemTiendaId, 1);
        }
    }
}
