using System.Text.Json;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.UseCases;

var mano = new NuevaManoUseCase().EjecutarNuevaPartida(new ConfiguracionPartida
{
    Modo = ModoJuego.Historia,
    HeroeDelHumano = ClaseHeroe.Mentiroso,
    RivalDeLaMaquina = ClaseRival.Lobizon,
    RivalNivel = 3
});
var json = JsonSerializer.Serialize(mano, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "GitHub", "lobizon-mano.json");
File.WriteAllText(path, json);
Console.WriteLine(path);
Console.WriteLine(json.Substring(0, Math.Min(2500, json.Length)));
