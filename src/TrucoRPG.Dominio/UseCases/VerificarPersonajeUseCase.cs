using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class VerificarPersonajeUseCase
    {

        private readonly IUsuarioRepositorio _repositorio;

        public VerificarPersonajeUseCase(IUsuarioRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<bool> Ejecutar(string userId)
        {
            return await _repositorio.PersonajeExistente(userId);
        }
    }
}
