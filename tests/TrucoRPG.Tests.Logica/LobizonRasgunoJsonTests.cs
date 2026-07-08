using System.Text.Json;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.UseCases;
using Xunit;

public class LobizonRasgunoJsonTests
{
    [Fact]
    public void NuevaPartida_Lobizon_SerializaRasgunoBloqueando()
    {
        var mano = new NuevaManoUseCase().EjecutarNuevaPartida(new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = ClaseHeroe.Mentiroso,
            RivalDeLaMaquina = ClaseRival.Lobizon,
            RivalNivel = 3
        });

        Assert.True(mano.RasgunoBloqueando);
        Assert.NotNull(mano.VistaHabilidadesRival);
        Assert.True(mano.VistaHabilidadesRival!.RasgunoBloqueando);

        var json = JsonSerializer.Serialize(mano, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.Contains("rasgunoBloqueando", json);
        Assert.Contains("true", json);
    }
}
