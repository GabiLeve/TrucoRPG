using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrucoRPG.Dominio.Repositorios
{
    public interface IRegisterUseCase
    {
        Task<string> EjecutarAsync(string username, string email, string password);
    }
}
