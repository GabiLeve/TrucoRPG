using TrucoDemo.Clases;
using TrucoDemo.Servicios;

namespace TrucoRPG.Tests.Logica;

public class MazoServicioTests
{
    private readonly List<Carta> _mazo = MazoServicio.CrearMazo();

    [Fact]
    public void CrearMazo_TieneCuarentaCartas()
    {
        Assert.Equal(40, _mazo.Count);
    }

    [Fact]
    public void CrearMazo_NoHayCartasDuplicadas()
    {
        var distintas = _mazo
            .Select(c => (c.Numero, c.Palo))
            .Distinct()
            .Count();

        Assert.Equal(40, distintas);
    }

    [Fact]
    public void CrearMazo_HayCuatroPalos()
    {
        var palos = _mazo.Select(c => c.Palo).Distinct().ToList();
        Assert.Equal(4, palos.Count);
        Assert.Contains("Espada", palos);
        Assert.Contains("Basto", palos);
        Assert.Contains("Oro", palos);
        Assert.Contains("Copa", palos);
    }

    [Fact]
    public void CrearMazo_CadaPaloTieneDiezCartas()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
        {
            int cantidad = _mazo.Count(c => c.Palo == palo);
            Assert.Equal(10, cantidad);
        }
    }

    [Fact]
    public void CrearMazo_AsDeEspadaEsLaCartaMasFuerte()
    {
        var asEspada = _mazo.First(c => c.Numero == 1 && c.Palo == "Espada");
        Assert.Equal(14, asEspada.ValorTruco);
    }

    [Fact]
    public void CrearMazo_AsDeBasto_TieneValor13()
    {
        var asBasto = _mazo.First(c => c.Numero == 1 && c.Palo == "Basto");
        Assert.Equal(13, asBasto.ValorTruco);
    }

    [Fact]
    public void CrearMazo_SieteDeEspada_TieneValor12()
    {
        var sieteEspada = _mazo.First(c => c.Numero == 7 && c.Palo == "Espada");
        Assert.Equal(12, sieteEspada.ValorTruco);
    }

    [Fact]
    public void CrearMazo_SieteDeOro_TieneValor11()
    {
        var sieteOro = _mazo.First(c => c.Numero == 7 && c.Palo == "Oro");
        Assert.Equal(11, sieteOro.ValorTruco);
    }

    [Fact]
    public void CrearMazo_Tres_TieneValor10()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
        {
            var tres = _mazo.First(c => c.Numero == 3 && c.Palo == palo);
            Assert.Equal(10, tres.ValorTruco);
        }
    }

    [Fact]
    public void CrearMazo_Dos_TieneValor9()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
        {
            var dos = _mazo.First(c => c.Numero == 2 && c.Palo == palo);
            Assert.Equal(9, dos.ValorTruco);
        }
    }

    [Fact]
    public void CrearMazo_AsDeOroYCopa_TienenValor8()
    {
        foreach (var palo in new[] { "Oro", "Copa" })
        {
            var as_ = _mazo.First(c => c.Numero == 1 && c.Palo == palo);
            Assert.Equal(8, as_.ValorTruco);
        }
    }
}
