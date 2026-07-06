using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrucoRPG.Dominio.Repositorios
{
    public interface ILoginUseCase
    {
        Task<string> EjecutarAsync(string email, string password);
    }
}
