using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class MazoServicioTests
{
    private readonly List<Carta> _mazo = MazoServicio.CrearMazo();

    // ─── Estructura del mazo ─────────────────────────────────────────

    [Fact]
    public void CrearMazo_TieneCuarentaCartas()
    {
        Assert.Equal(40, _mazo.Count);
    }

    [Fact]
    public void CrearMazo_NoHayCartasDuplicadas()
    {
        var distintas = _mazo.Select(c => (c.Numero, c.Palo)).Distinct().Count();
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
            Assert.Equal(10, _mazo.Count(c => c.Palo == palo));
    }

    [Fact]
    public void CrearMazo_NumerosCorrectos_SinOchoNiNueve()
    {
        var nums = _mazo.Select(c => c.Numero).Distinct().OrderBy(n => n).ToList();
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 }, nums);
    }

    // ─── Jerarquía de truco (orden exacto) ───────────────────────────

    [Fact]
    public void CrearMazo_AsDeEspadaEsLaCartaMasFuerte_Valor14()
    {
        var carta = _mazo.First(c => c.Numero == 1 && c.Palo == "Espada");
        Assert.Equal(14, carta.ValorTruco);
    }

    [Fact]
    public void CrearMazo_AsDeBasto_TieneValor13()
    {
        var carta = _mazo.First(c => c.Numero == 1 && c.Palo == "Basto");
        Assert.Equal(13, carta.ValorTruco);
    }

    [Fact]
    public void CrearMazo_SieteDeEspada_TieneValor12()
    {
        var carta = _mazo.First(c => c.Numero == 7 && c.Palo == "Espada");
        Assert.Equal(12, carta.ValorTruco);
    }

    [Fact]
    public void CrearMazo_SieteDeOro_TieneValor11()
    {
        var carta = _mazo.First(c => c.Numero == 7 && c.Palo == "Oro");
        Assert.Equal(11, carta.ValorTruco);
    }

    [Fact]
    public void CrearMazo_Tres_TieneValor10_EnTodosLosPalos()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
            Assert.Equal(10, _mazo.First(c => c.Numero == 3 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_Dos_TieneValor9_EnTodosLosPalos()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
            Assert.Equal(9, _mazo.First(c => c.Numero == 2 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_AsDeOroYCopa_TienenValor8()
    {
        foreach (var palo in new[] { "Oro", "Copa" })
            Assert.Equal(8, _mazo.First(c => c.Numero == 1 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_Doce_TieneValor7_EnTodosLosPalos()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
            Assert.Equal(7, _mazo.First(c => c.Numero == 12 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_Once_TieneValor6_EnTodosLosPalos()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
            Assert.Equal(6, _mazo.First(c => c.Numero == 11 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_Diez_TieneValor5_EnTodosLosPalos()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
            Assert.Equal(5, _mazo.First(c => c.Numero == 10 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_SieteDeBastoCopa_TieneValor4()
    {
        foreach (var palo in new[] { "Basto", "Copa" })
            Assert.Equal(4, _mazo.First(c => c.Numero == 7 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_Seis_TieneValor3_EnTodosLosPalos()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
            Assert.Equal(3, _mazo.First(c => c.Numero == 6 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_Cinco_TieneValor2_EnTodosLosPalos()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
            Assert.Equal(2, _mazo.First(c => c.Numero == 5 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_Cuatro_TieneValor1_EnTodosLosPalos()
    {
        foreach (var palo in new[] { "Espada", "Basto", "Oro", "Copa" })
            Assert.Equal(1, _mazo.First(c => c.Numero == 4 && c.Palo == palo).ValorTruco);
    }

    [Fact]
    public void CrearMazo_JerarquiaEstricta_AsEspada_MasFuerteQueTodo()
    {
        var asEspada = _mazo.First(c => c.Numero == 1 && c.Palo == "Espada");
        var maxResto = _mazo
            .Where(c => !(c.Numero == 1 && c.Palo == "Espada"))
            .Max(c => c.ValorTruco);
        Assert.True(asEspada.ValorTruco > maxResto);
    }
}
