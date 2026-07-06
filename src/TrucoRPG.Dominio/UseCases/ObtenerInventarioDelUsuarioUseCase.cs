using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerInventarioDelUsuarioUseCase
    {
        private readonly IInventarioRepositorio _inventarioRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public ObtenerInventarioDelUsuarioUseCase(IInventarioRepositorio inventarioRepositorio, IUsuarioRepositorio usuarioRepositorio)
        {
            _inventarioRepositorio = inventarioRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
        }

        public async Task<List<Inventario>> Ejecutar(string usuarioId)
        {
            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new Exception("El usuario no existe.");
            }
            return await _inventarioRepositorio.ObtenerInventarioDeUsuario(usuarioId);
        }

    }
}
