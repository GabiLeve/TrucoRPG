using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class CrearPersonajeUseCase
    {
        private readonly IUsuarioRepositorio _repositorio;

        public CrearPersonajeUseCase(IUsuarioRepositorio repositorio) {
            _repositorio = repositorio;
        }

        public async Task Ejecutar(string userId, string spriteKey, Guid habilidad) {
            await _repositorio.CrearPersonaje(userId,spriteKey,habilidad);
        }

    }
}
