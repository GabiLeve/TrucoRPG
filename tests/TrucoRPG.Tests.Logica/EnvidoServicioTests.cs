using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class EnvidoServicioTests
{
    private static Carta C(int numero, string palo) =>
        new() { Numero = numero, Palo = palo, ValorTruco = 0 };

    // ─── Par del mismo palo ───────────────────────────────────────────

    [Fact]
    public void CalcularTanto_DosCartasMismoPalo_RetornaSumaMas20()
    {
        var mano = new List<Carta> { C(5, "Espada"), C(6, "Espada"), C(3, "Oro") };
        var calcularTantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(31,calcularTantos );
    }

    [Fact]
    public void CalcularTanto_TresCartasMismoPalo_UsaLosDosValoresMasAltos()
    {
        // 7 + 6 + 20 = 33
        var mano = new List<Carta> { C(7, "Copa"), C(6, "Copa"), C(5, "Copa") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(33, tantos);
    }

    [Fact]
    public void CalcularTanto_MaximoAlcanzable_Es33_ConSieteYSeis()
    {
        var mano = new List<Carta> { C(7, "Basto"), C(6, "Basto"), C(1, "Espada") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(33,tantos );
    }

    [Fact]
    public void CalcularTanto_AsY2_MismoPalo_Vale23()
    {
        // 1 + 2 + 20 = 23
        var mano = new List<Carta> { C(1, "Copa"), C(2, "Copa"), C(7, "Espada") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(23, tantos);
    }

    [Fact]
    public void CalcularTanto_Par7Y1_MismoPalo_Vale28()
    {
        // 7 + 1 + 20 = 28
        var mano = new List<Carta> { C(7, "Basto"), C(1, "Basto"), C(6, "Oro") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(28,tantos );
    }

    [Fact]
    public void CalcularTanto_TieneDosParesDistintos_EligeMejorPar()
    {
        // Espada: 7+3+20=30 | Oro: 4+20... no hay par → elige Espada(30)
        var mano = new List<Carta> { C(7, "Espada"), C(3, "Espada"), C(4, "Oro") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(30,tantos );
    }

    [Fact]
    public void CalcularTanto_EligeMejorPalo_CuandoHayDosOpcionesDePalo()
    {
        // Espada: 7+3+20=30; Oro: solo uno → elige Espada
        var mano = new List<Carta> { C(7, "Espada"), C(3, "Espada"), C(6, "Oro") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(30,tantos );
    }

    // ─── Figuras (10, 11, 12) valen 0 ────────────────────────────────

    [Fact]
    public void CalcularTanto_TresFigurasDelMismoPalo_Retorna20()
    {
        // Figura + figura + 20 = 0 + 0 + 20 = 20
        var mano = new List<Carta> { C(10, "Basto"), C(11, "Basto"), C(12, "Basto") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(20, tantos);
    }

    [Fact]
    public void CalcularTanto_DosFigurasDelMismoPalo_SinNumerosDelPalo_Retorna20()
    {
        var mano = new List<Carta> { C(10, "Copa"), C(11, "Copa"), C(5, "Espada") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(20,tantos );
    }

    [Fact]
    public void CalcularTanto_FiguraSola_SinParDelMismoPalo_Vale0()
    {
        // Sin par del mismo palo → valor máximo individual; figuras valen 0
        var mano = new List<Carta> { C(10, "Espada"), C(11, "Oro"), C(12, "Copa") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(0, tantos);
    }

    // ─── Sin par del mismo palo ───────────────────────────────────────

    [Fact]
    public void CalcularTanto_SinDosMismoPalo_RetornaMaximoValorIndividual()
    {
        var mano = new List<Carta> { C(5, "Espada"), C(6, "Oro"), C(3, "Copa") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(6, tantos);
    }

    [Fact]
    public void CalcularTanto_TresCartasTresPalosDistintos_RetornaElMayor()
    {
        var mano = new List<Carta> { C(1, "Espada"), C(7, "Oro"), C(3, "Copa") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(7, tantos);
    }

    [Fact]
    public void CalcularTanto_CartaNumero1_Vale1ParaEnvido_SinPar()
    {
        // As vale 1 para envido; sin par → máximo individual = 3
        var mano = new List<Carta> { C(1, "Espada"), C(2, "Oro"), C(3, "Copa") };
        var tantos = EnvidoServicio.CalcularTanto(mano);
        Assert.Equal(3, tantos);
    }
}
