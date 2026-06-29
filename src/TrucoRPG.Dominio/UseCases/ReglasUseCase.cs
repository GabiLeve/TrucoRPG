using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Logica.UseCases
{
    public class ReglasUseCase
    {
        public async Task<IEnumerable<Carta>> ObtenerCartas()
        {
            var mazo = MazoServicio.CrearMazo();

            if (mazo == null)
            {
                throw new InvalidOperationException("No se puede mostrar las cartas");
            }

            return await Task.FromResult(mazo.OrderByDescending(c => c.ValorTruco));
        }

        public async Task<IEnumerable<CategoriaRegla>> ObtenerReglasGenerales()
        {
            var reglas = ReglasServicio.ObtenerReglasGenerales();

            if (reglas == null)
            {
                throw new InvalidOperationException("No se puede mostrar las reglas generales");
            }

            return await Task.FromResult(reglas);
        }

    }
}
