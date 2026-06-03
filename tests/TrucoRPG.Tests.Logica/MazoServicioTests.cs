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
        int cantidadEsperada = 40;
        int cantidadActual = _mazo.Count;
        Assert.Equal(cantidadEsperada, cantidadActual);
    }

    [Fact]
    public void CrearMazo_NoHayCartasDuplicadas()
    {
        int cantidadEsperada = 40;
        var distintas = _mazo.Select(c => (c.Numero, c.Palo)).Distinct().Count();
        Assert.Equal(cantidadEsperada, distintas);


    }

    [Fact]
    public void CrearMazo_HayCuatroPalos()
    {
        int cantidadEsperada = 4;
        var palos = _mazo.Select(c => c.Palo).Distinct().ToList();
        Assert.Equal(cantidadEsperada, palos.Count);
        Assert.Contains("Espada", palos);
        Assert.Contains("Basto", palos);
        Assert.Contains("Oro", palos);
        Assert.Contains("Copa", palos);
    }

    [Fact]
    public void CrearMazo_CadaPaloTieneDiezCartas()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        var cantidadesPorPalo = new List<int>();
        int cartasEsperadasPorPalo = 10;

        foreach (var palo in palosAProbar)
        {
            int cantidad = _mazo.Count(c => c.Palo == palo);
            cantidadesPorPalo.Add(cantidad);
        }

        foreach (int cantidad in cantidadesPorPalo)
        {
            Assert.Equal(cartasEsperadasPorPalo, cantidad);
        }
    }

    [Fact]
    public void CrearMazo_NumerosCorrectos_SinOchoNiNueve()
    {
        var numerosEsperados = new[] { 1, 2, 3, 4, 5, 6, 7, 10, 11, 12 };
        var nums = _mazo.Select(c => c.Numero).Distinct().OrderBy(n => n).ToList();
        Assert.Equal(numerosEsperados, nums);
    }

    // ─── Jerarquía de truco (orden exacto) ───────────────────────────

    [Fact]
    public void CrearMazo_AsDeEspadaEsLaCartaMasFuerte_Valor14()
    {
        int valorEsperado = 14;
        var carta = _mazo.First(c => c.Numero == 1 && c.Palo == "Espada");
        Assert.Equal(valorEsperado, carta.ValorTruco);
    }

    [Fact]
    public void CrearMazo_AsDeBasto_TieneValor13()
    {
        int valorEsperado = 13;
        var carta = _mazo.First(c => c.Numero == 1 && c.Palo == "Basto");
        Assert.Equal(valorEsperado, carta.ValorTruco);
    }

    [Fact]
    public void CrearMazo_SieteDeEspada_TieneValor12()
    {
        int valorEsperado = 12; 
        var carta = _mazo.First(c => c.Numero == 7 && c.Palo == "Espada");
        Assert.Equal(12, carta.ValorTruco);
    }

    [Fact]
    public void CrearMazo_SieteDeOro_TieneValor11()
    {
        int valorEsperado = 11;
        var carta = _mazo.First(c => c.Numero == 7 && c.Palo == "Oro");
        Assert.Equal(valorEsperado, carta.ValorTruco);
    }

    [Fact]
    public void CrearMazo_Tres_TieneValor10_EnTodosLosPalos()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        int valorEsperado = 10;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 3 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_Dos_TieneValor9_EnTodosLosPalos()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        int valorEsperado = 9;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 2 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_AsDeOroYCopa_TienenValor8()
    {
        var palosAProbar = new[] { "Oro", "Copa" };
        int valorEsperado = 8;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 1 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_Doce_TieneValor7_EnTodosLosPalos()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        int valorEsperado = 7;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 12 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_Once_TieneValor6_EnTodosLosPalos()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        int valorEsperado = 6;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 11 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_Diez_TieneValor5_EnTodosLosPalos()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        int valorEsperado = 5;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 10 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_SieteDeBastoCopa_TieneValor4()
    {
        var palosAProbar = new[] { "Basto", "Copa" };
        int valorEsperado = 4;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 7 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_Seis_TieneValor3_EnTodosLosPalos()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        int valorEsperado = 3;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 6 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_Cinco_TieneValor2_EnTodosLosPalos()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        int valorEsperado = 2;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 5 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_Cuatro_TieneValor1_EnTodosLosPalos()
    {
        var palosAProbar = new[] { "Espada", "Basto", "Oro", "Copa" };
        int valorEsperado = 1;
        var valoresObtenidos = new List<int>();

        foreach (var palo in palosAProbar)
        {
            int valor = _mazo.First(c => c.Numero == 4 && c.Palo == palo).ValorTruco;
            valoresObtenidos.Add(valor);
        }

        foreach (int valor in valoresObtenidos)
        {
            Assert.Equal(valorEsperado, valor);
        }
    }

    [Fact]
    public void CrearMazo_JerarquiaEstricta_AsEspada_MasFuerteQueTodo()
    {
        var asEspada = _mazo.First(c => c.Numero == 1 && c.Palo == "Espada");
        var maxResto = _mazo
            .Where(c => !(c.Numero == 1 && c.Palo == "Espada"))
            .Max(c => c.ValorTruco);

        bool resultado = asEspada.ValorTruco > maxResto;
        Assert.True(resultado);
    }
}
