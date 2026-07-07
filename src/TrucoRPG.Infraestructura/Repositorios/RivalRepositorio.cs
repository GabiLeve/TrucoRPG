using Microsoft.EntityFrameworkCore;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Infraestructura.Data;

namespace TrucoRPG.Infraestructura.Repositorios
{
    public class RivalRepositorio : IRivalRepositorio
    {
        private readonly AppDbContext _db;

        public RivalRepositorio(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<Rival>> ObtenerTodosAsync() =>
            await _db.Rivales.AsNoTracking().OrderBy(r => r.Nivel).ToListAsync();

        public async Task<Rival?> ObtenerPorNivelAsync(int nivel) =>
            await _db.Rivales.AsNoTracking().FirstOrDefaultAsync(r => r.Nivel == nivel);

        public async Task<Rival?> ObtenerPorTipoAsync(ClaseRival tipo) =>
            await _db.Rivales.AsNoTracking().FirstOrDefaultAsync(r => r.TipoRival == tipo);
    }
}
