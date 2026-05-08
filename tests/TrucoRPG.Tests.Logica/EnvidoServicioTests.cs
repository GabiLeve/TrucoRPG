using TrucoDemo.Clases;
using TrucoDemo.Servicios;

namespace TrucoRPG.Tests.Logica;

public class EnvidoServicioTests
{
    private static Carta C(int numero, string palo) =>
        new() { Numero = numero, Palo = palo, ValorTruco = 0 };

    [Fact]
    public void CalcularTanto_DosCartasMismoPalo_RetornaSumaMas20()
    {
        var mano = new List<Carta> { C(5, "Espada"), C(6, "Espada"), C(3, "Oro") };

        int tanto = EnvidoServicio.CalcularTanto(mano);

        Assert.Equal(31, tanto);
    }

    [Fact]
    public void CalcularTanto_TresCartasMismoPalo_UsaLosDosValoresMasAltos()
    {
        var mano = new List<Carta> { C(7, "Copa"), C(6, "Copa"), C(5, "Copa") };

        int tanto = EnvidoServicio.CalcularTanto(mano);

        Assert.Equal(33, tanto);
    }

    [Fact]
    public void CalcularTanto_MazoConFigurasDelMismoPalo_FigurasValen0()
    {
        var mano = new List<Carta> { C(10, "Basto"), C(11, "Basto"), C(3, "Basto") };

        int tanto = EnvidoServicio.CalcularTanto(mano);

        Assert.Equal(20, tanto);
    }

    [Fact]
    public void CalcularTanto_SinDosMismoPalo_RetornaMaximoValorIndividual()
    {
        var mano = new List<Carta> { C(5, "Espada"), C(6, "Oro"), C(3, "Copa") };

        int tanto = EnvidoServicio.CalcularTanto(mano);

        Assert.Equal(6, tanto);
    }

    [Fact]
    public void CalcularTanto_EligeMejorPalo_CuandoHayDosOpcionesDePalo()
    {
        var mano = new List<Carta> { C(7, "Espada"), C(3, "Espada"), C(6, "Oro") };

        int tanto = EnvidoServicio.CalcularTanto(mano);

        Assert.Equal(30, tanto);
    }

    [Fact]
    public void CalcularTanto_FiguraSola_Vale0ParaEnvido()
    {
        var mano = new List<Carta> { C(10, "Espada"), C(11, "Oro"), C(12, "Copa") };

        int tanto = EnvidoServicio.CalcularTanto(mano);

        Assert.Equal(0, tanto);
    }
}
