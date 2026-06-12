using System.Text.Json;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    public static class SalpicaduraServicio
    {
        private const int CartasAReemplazar = 2;
        private static readonly string[] Palos = ["Espada", "Basto", "Oro", "Copa"];

        public static void ReemplazarCartasHumano(ManoTruco mano)
        {
            var manoHumano = mano.Humano.Mano;
            if (manoHumano.Count < CartasAReemplazar)
                return;

            var indices = Enumerable.Range(0, manoHumano.Count)
                .OrderBy(_ => Random.Shared.Next())
                .Take(CartasAReemplazar)
                .ToList();

            // #region agent log
            var antes = indices.Select(i => new { i, manoHumano[i].Numero, manoHumano[i].Palo, manoHumano[i].ValorTruco }).ToList();
            var mazoAntes = mano.CartasRestantesMazo.Count;
            AgentLog("SalpicaduraServicio.cs:antes", "cartas antes de salpicadura", new { indices, cartas = antes, mazoRestante = mazoAntes }, "A");
            // #endregion

            foreach (var idx in indices)
            {
                var carta = manoHumano[idx];
                var opciones = Palos
                    .Where(p => !p.Equals(carta.Palo, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                var nuevoPalo = opciones[Random.Shared.Next(opciones.Length)];

                carta.Palo = nuevoPalo;
                carta.ValorTruco = MazoServicio.ObtenerValorTruco(carta.Numero, nuevoPalo);
                carta.PaloVisual = null;
            }

            // #region agent log
            var despues = indices.Select(i => new { i, manoHumano[i].Numero, manoHumano[i].Palo, manoHumano[i].ValorTruco }).ToList();
            var numerosCambiaron = antes.Zip(despues, (a, d) => a.Numero != d.Numero).Any(x => x);
            var palosCambiaron = antes.Zip(despues, (a, d) => a.Palo != d.Palo).Any(x => x);
            AgentLog("SalpicaduraServicio.cs:despues", "cartas despues de salpicadura", new
            {
                cartas = despues,
                numerosCambiaron,
                palosCambiaron,
                mazoRestante = mano.CartasRestantesMazo.Count,
                mazoSinCambios = mano.CartasRestantesMazo.Count == mazoAntes,
                origen = "cambio_palo",
                runId = "post-fix"
            }, palosCambiaron && !numerosCambiaron ? "fix" : "A");
            // #endregion
        }

        // #region agent log
        private static void AgentLog(string location, string message, object data, string hypothesisId)
        {
            try
            {
                var line = JsonSerializer.Serialize(new
                {
                    sessionId = "e6cb96",
                    hypothesisId,
                    location,
                    message,
                    data,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
                var paths = new[]
                {
                    @"C:\Users\guido\Documents\GitHub\debug-e6cb96.log",
                    Path.Combine(AppContext.BaseDirectory, "debug-e6cb96.log")
                };
                foreach (var p in paths.Distinct())
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(p);
                        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                        File.AppendAllText(p, line + Environment.NewLine);
                        break;
                    }
                    catch { /* try next path */ }
                }
            }
            catch { /* ignore */ }
        }
        // #endregion
    }
}
