using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class MaquinaServicioTests
{
    private static Carta C(int valor) =>
        new() { Numero = valor, Palo = "Espada", ValorTruco = valor };

    [Fact]
    public void ElegirCarta_SinCartaHumano_DevuelveCartaDeLaMano()
    {
        var mano = new List<Carta> { C(3), C(7), C(10) };
        var elegida = MaquinaServicio.ElegirCarta(mano, null);
        Assert.Contains(elegida, mano);
    }

    [Fact]
    public void ElegirCarta_TieneCartasQueGanan_EligeListaMenorGanadora()
    {
        var mano = new List<Carta> { C(4), C(8), C(12) };
        var cartaHumano = C(5);

        var elegida = MaquinaServicio.ElegirCarta(mano, cartaHumano);

        Assert.Equal(8, elegida.ValorTruco);
    }

    [Fact]
    public void ElegirCarta_SoloUnaCartaGana_EligeLaGanadora()
    {
        var mano = new List<Carta> { C(2), C(3), C(9) };
        var cartaHumano = C(8);

        var elegida = MaquinaServicio.ElegirCarta(mano, cartaHumano);

        Assert.Equal(9, elegida.ValorTruco);
    }

    [Fact]
    public void ElegirCarta_NingunaCarta_Gana_EligeElMenorValor()
    {
        var mano = new List<Carta> { C(5), C(3), C(7) };
        var cartaHumano = C(14);

        var elegida = MaquinaServicio.ElegirCarta(mano, cartaHumano);

        Assert.Equal(3, elegida.ValorTruco);
    }

    [Fact]
    public void ElegirCarta_SoloUnaCartaEnMano_DevuelveEsa()
    {
        var mano = new List<Carta> { C(6) };
        var elegida = MaquinaServicio.ElegirCarta(mano, C(10));
        Assert.Equal(6, elegida.ValorTruco);
    }
}
