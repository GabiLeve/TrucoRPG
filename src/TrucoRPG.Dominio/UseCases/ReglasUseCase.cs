using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Logica.UseCases
{
    public class ReglasUseCase
    {
        public async Task<IEnumerable<Carta>> GetCartas()
        {
            var mazo = MazoServicio.CrearMazo();

            return await Task.FromResult(mazo.OrderByDescending(c => c.ValorTruco));
        }

    }
}
