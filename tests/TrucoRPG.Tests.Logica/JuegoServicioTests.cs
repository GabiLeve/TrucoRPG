using TrucoDemo.Clases;
using TrucoDemo.Servicios;

namespace TrucoRPG.Tests.Logica;

public class JuegoServicioTests
{
    private static Carta Carta(int valor) =>
        new() { Numero = 1, Palo = "Espada", ValorTruco = valor };

    private static Baza Baza(string ganador) =>
        new() { Ganador = ganador };

    [Fact]
    public void ResolverBaza_HumanoTieneMayorValor_GanaHumano()
    {
        var resultado = JuegoServicio.ResolverBaza(Carta(10), Carta(5));
        Assert.Equal("Humano", resultado);
    }

    [Fact]
    public void ResolverBaza_MaquinaTieneMayorValor_GanaMaquina()
    {
        var resultado = JuegoServicio.ResolverBaza(Carta(3), Carta(14));
        Assert.Equal("Maquina", resultado);
    }

    [Fact]
    public void ResolverBaza_MismoValor_EsParda()
    {
        var resultado = JuegoServicio.ResolverBaza(Carta(9), Carta(9));
        Assert.Equal("Parda", resultado);
    }

    [Fact]
    public void ResolverGanadorMano_HumanoGanaDosBasasConsecutivas_GanaHumano()
    {
        var bazas = new List<Baza> { Baza("Humano"), Baza("Humano") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_MaquinaGanaDosBasasConsecutivas_GanaMaquina()
    {
        var bazas = new List<Baza> { Baza("Maquina"), Baza("Maquina") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Maquina", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaLuegoHumano_SinTercera_RetornaNull()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Humano") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Null(ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaLuegoHumanoConTerceraParda_GanaHumano()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Humano"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaLuegoMaquina_SinTercera_RetornaNull()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Maquina") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Null(ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaLuegoMaquinaConTerceraParda_GanaMaquina()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Maquina"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Maquina", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPardaParda_GanaJugadorDeMano()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Parda"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Maquina");
        Assert.Equal("Maquina", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_UnoUnoHumanoEnTercera_GanaHumano()
    {
        var bazas = new List<Baza> { Baza("Humano"), Baza("Maquina"), Baza("Humano") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_UnoUnoMaquinaEnTercera_GanaMaquina()
    {
        var bazas = new List<Baza> { Baza("Humano"), Baza("Maquina"), Baza("Maquina") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Maquina", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_SinBasas_RetornaNull()
    {
        var ganador = JuegoServicio.ResolverGanadorMano(new List<Baza>(), "Humano");
        Assert.Null(ganador);
    }

    [Fact]
    public void ResolverGanadorMano_SoloUnaBaza_RetornaNull()
    {
        var bazas = new List<Baza> { Baza("Humano") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Null(ganador);
    }
}
