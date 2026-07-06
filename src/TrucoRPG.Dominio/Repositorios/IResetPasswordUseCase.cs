using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrucoRPG.Dominio.Repositorios
{
    public interface IResetPasswordUseCase
    {
        Task EjecutarAsync(string email, string token, string nuevaPassword);
    }
}
