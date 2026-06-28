using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class ObtenerPersonajeDelUsuarioUseCase
    {
        private readonly IUsuarioRepositorio _repositorio;

        public ObtenerPersonajeDelUsuarioUseCase(IUsuarioRepositorio repositorio) {
            _repositorio = repositorio;
        }

        public async Task<Personaje> Ejecutar(string userId)
        {
            return await _repositorio.ObtenerPersonajeDelUsuario(userId);
        }
    }
}
