using TrucoDemo.Clases;
using TrucoDemo.Servicios;

namespace TrucoRPG.Tests.Logica;

public class JuegoServicioTests
{
    private static Carta Carta(int valor) =>
        new() { Numero = 1, Palo = "Espada", ValorTruco = valor };

    private static Baza Baza(string ganador) =>
        new() { Ganador = ganador };

    // ─── ResolverBaza ────────────────────────────────────────────────

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
    public void ResolverBaza_AsDeEspada_GanaAAsDeBastoMasAlto()
    {
        // As de Espada (14) > As de Basto (13)
        var resultado = JuegoServicio.ResolverBaza(Carta(14), Carta(13));
        Assert.Equal("Humano", resultado);
    }

    // ─── ResolverGanadorMano: casos base ─────────────────────────────

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
    public void ResolverGanadorMano_UnoUnoSinTercera_RetornaNull()
    {
        var bazas = new List<Baza> { Baza("Humano"), Baza("Maquina") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Null(ganador);
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

    // ─── Parda en SEGUNDA baza ───────────────────────────────────────
    // Regla: parda en 2da baza → gana quien ganó la 1ra.

    [Fact]
    public void ResolverGanadorMano_HumanoGanaPrimera_PardaSegunda_GanaHumano()
    {
        var bazas = new List<Baza> { Baza("Humano"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_MaquinaGanaPrimera_PardaSegunda_GanaMaquina()
    {
        var bazas = new List<Baza> { Baza("Maquina"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Maquina", ganador);
    }

    // ─── Parda en TERCERA baza ───────────────────────────────────────
    // Regla: 1ra y 2da distintas, 3ra parda → gana quien ganó la 1ra.

    [Fact]
    public void ResolverGanadorMano_HumanoGanaPrimera_MaquinaGanaSegunda_PardaTercera_GanaHumano()
    {
        var bazas = new List<Baza> { Baza("Humano"), Baza("Maquina"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_MaquinaGanaPrimera_HumanoGanaSegunda_PardaTercera_GanaMaquina()
    {
        var bazas = new List<Baza> { Baza("Maquina"), Baza("Humano"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Maquina", ganador);
    }

    // ─── Parda en PRIMERA baza ───────────────────────────────────────
    // Regla: parda en 1ra → la carta más alta de la 2da decide (gana quien gana la 2da).

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_HumanoGanaSegunda_GanaHumano()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Humano") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_MaquinaGanaSegunda_GanaMaquina()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Maquina") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Maquina", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_HumanoGanaSegunda_ConTerceraParda_SigueGanandoHumano()
    {
        // La 3ra baza no cambia el resultado: la 2da ya decidió.
        var bazas = new List<Baza> { Baza("Parda"), Baza("Humano"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_MaquinaGanaSegunda_ConTerceraParda_SigueGanandoMaquina()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Maquina"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Maquina", ganador);
    }

    // ─── Parda en PRIMERA y SEGUNDA ──────────────────────────────────
    // Regla: parda+parda → va a la 3ra. Si 3ra también parda → gana el jugador "mano".

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_PardaSegunda_SinTercera_RetornaNull()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Null(ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_PardaSegunda_HumanoGanaTercera_GanaHumano()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Parda"), Baza("Humano") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPrimera_PardaSegunda_MaquinaGanaTercera_GanaMaquina()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Parda"), Baza("Maquina") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Maquina", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPardaParda_JugadorManoEsMaquina_GanaMaquina()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Parda"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Maquina");
        Assert.Equal("Maquina", ganador);
    }

    [Fact]
    public void ResolverGanadorMano_PardaPardaParda_JugadorManoEsHumano_GanaHumano()
    {
        var bazas = new List<Baza> { Baza("Parda"), Baza("Parda"), Baza("Parda") };
        var ganador = JuegoServicio.ResolverGanadorMano(bazas, "Humano");
        Assert.Equal("Humano", ganador);
    }
}
