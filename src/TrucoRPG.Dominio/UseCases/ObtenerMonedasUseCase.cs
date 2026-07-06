using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerMonedasUseCase
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;
        public ObtenerMonedasUseCase(IUsuarioRepositorio usuarioRepositorio)
        {
            _usuarioRepositorio = usuarioRepositorio;
        }
        public async Task<int> Ejecutar(string id)
        {
            var usuario = await _usuarioRepositorio.ObtenerPorIdAsync(id);
            if (usuario is null)
            {
                throw new InvalidOperationException("Usuario no encontrado.");
            }
            return usuario.Monedas;
        }
    }
}
