using Microsoft.EntityFrameworkCore;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Infraestructura.Data;

namespace TrucoRPG.Infraestructura.Repositorios
{
    public class ProgresoPartidaRepositorio : IProgresoPartidaRepositorio
    {
        private readonly AppDbContext _db;

        public ProgresoPartidaRepositorio(AppDbContext db) => _db = db;

        public async Task<ProgresoPartida?> ObtenerPorUsuarioIdAsync(string usuarioId) =>
            await _db.ProgresoPartida.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);

        public async Task<ProgresoPartida> ObtenerOCrearAsync(string usuarioId)
        {
            var progreso = await _db.ProgresoPartida.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
            if (progreso is not null)
                return progreso;

            progreso = new ProgresoPartida
            {
                UsuarioId = usuarioId,
                UltimoRivalDerrotadoNivel = 0,
                PuntosAcumulados = 0
            };

            _db.ProgresoPartida.Add(progreso);
            await _db.SaveChangesAsync();
            return progreso;
        }

        public async Task RegistrarVictoriaAsync(
            string usuarioId,
            int rivalNivelDerrotado,
            int diferenciaPuntos)
        {
            var progreso = await ObtenerOCrearAsync(usuarioId);

            if (rivalNivelDerrotado > progreso.UltimoRivalDerrotadoNivel)
                progreso.UltimoRivalDerrotadoNivel = rivalNivelDerrotado;

            if (diferenciaPuntos > 0)
                progreso.PuntosAcumulados += diferenciaPuntos;

            await _db.SaveChangesAsync();
        }
    }
}
