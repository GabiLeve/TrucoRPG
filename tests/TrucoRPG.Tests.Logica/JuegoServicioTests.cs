using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

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
        var cartaHumano = Carta(10);
        var cartaMaquina = Carta(5);
        string ganadorEsperado = "Humano";

        var resultado = JuegoServicio.ResolverBaza(cartaHumano, cartaMaquina);

        Assert.Equal(ganadorEsperado, resultado);
    }

    [Fact]
    public void ResolverBaza_MaquinaTieneMayorValor_GanaMaquina()
    {
        var cartaHumano = Carta(3);
        var cartaMaquina = Carta(14);
        string ganadorEsperado = "Maquina";

        var resultado = JuegoServicio.ResolverBaza(cartaHumano, cartaMaquina);

        Assert.Equal(ganadorEsperado, resultado);
    }

    [Fact]
    public void ResolverBaza_MismoValor_EsParda()
    {
        var cartaHumano = Carta(9);
        var cartaMaquina = Carta(9);
        string resultadoEsperado = "Parda";

        var resultado = JuegoServicio.ResolverBaza(cartaHumano, cartaMaquina);

        Assert.Equal(resultadoEsperado, resultado);
    }

    [Fact]
    public void ResolverBaza_AsDeEspada_GanaAAsDeBastoMasAlto()
    {
        // As de Espada (14) > As de Basto (13)
        var asEspada = Carta(14);
        var asBasto = Carta(13);
        string ganadorEsperado = "Humano";

        var resultado = JuegoServicio.ResolverBaza(asEspada, asBasto);

        Assert.Equal(ganadorEsperado, resultado);
    }

    // ─── ResolverGanadorMano: casos base ─────────────────────────────

    [Fact]
    public void ResolverGanadorMano_SinBasas_RetornaNull()
    {
        var bazas = new List<Baza>();
        string jugadorMano = "Humano";

        var ganador = JuegoServicio.ResolverGanadorMano(bazas, jugadorMano);

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
