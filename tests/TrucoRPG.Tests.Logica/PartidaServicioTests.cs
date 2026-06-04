using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class PartidaServicioTests
{
    // ─── Estructura básica de la mano creada ─────────────────────────

    [Fact]
    public void CrearManoNueva_RetornaManoNoNula()
    {
        var mano = PartidaServicio.CrearManoNueva();
        Assert.NotNull(mano);
    }

    [Fact]
    public void CrearManoNueva_HumanoNoEsMaquina()
    {
        var mano = PartidaServicio.CrearManoNueva();
        Assert.False(mano.Humano.EsMaquina);
    }

    [Fact]
    public void CrearManoNueva_MaquinaEsMaquina()
    {
        var mano = PartidaServicio.CrearManoNueva();
        Assert.True(mano.Maquina.EsMaquina);
    }

    [Fact]
    public void CrearManoNueva_HumanoTieneTresCartas()
    {
        var mano = PartidaServicio.CrearManoNueva();
        Assert.Equal(3, mano.Humano.Mano.Count);
    }

    [Fact]
    public void CrearManoNueva_MaquinaTieneTresCartas()
    {
        var mano = PartidaServicio.CrearManoNueva();
        Assert.Equal(3, mano.Maquina.Mano.Count);
    }

    [Fact]
    public void CrearManoNueva_CartasHumanoYMaquinaSonDistintas()
    {
        var mano = PartidaServicio.CrearManoNueva();
        var humano = mano.Humano.Mano.Select(c => (c.Numero, c.Palo));
        var maquina = mano.Maquina.Mano.Select(c => (c.Numero, c.Palo));

        Assert.Empty(humano.Intersect(maquina));
    }

    // ─── Numeración y puntos ─────────────────────────────────────────

    [Fact]
    public void CrearManoNueva_NumeroDeManoDefecto_Es1()
    {
        var mano = PartidaServicio.CrearManoNueva();
        Assert.Equal(1, mano.NumeroDeMano);
    }

    [Fact]
    public void CrearManoNueva_PuntosIniciales_SonCero()
    {
        var mano = PartidaServicio.CrearManoNueva();

        Assert.Equal(0, mano.PuntosHumano);
        Assert.Equal(0, mano.PuntosMaquina);
    }

    [Fact]
    public void CrearManoNueva_ConPuntosExistentes_LosConserva()
    {
        int nuemerMano = 3;
        int ptsHumano = 8;
        int ptsMaquina = 5;

        var mano = PartidaServicio.CrearManoNueva(numeroDeMano: nuemerMano, puntosHumano: ptsHumano, puntosMaquina: ptsMaquina);

        Assert.Equal(8, mano.PuntosHumano);
        Assert.Equal(5, mano.PuntosMaquina);
    }

    [Fact]
    public void CrearManoNueva_NumeroDeManoImpar_ManoIniciadaPorHumano()
    {
        int manoImpar = 1;

        var mano = PartidaServicio.CrearManoNueva(numeroDeMano: manoImpar);

        Assert.Equal("Humano", mano.ManoIniciadaPor);
    }

    [Fact]
    public void CrearManoNueva_NumeroDeManoPar_ManoIniciadaPorMaquina()
    {
        int manoPar = 2;

        var mano = PartidaServicio.CrearManoNueva(numeroDeMano: manoPar);

        Assert.Equal("Maquina", mano.ManoIniciadaPor);
    }

    [Fact]
    public void CrearManoNueva_TurnoActual_IgualAManoIniciadaPor()
    {
        int numeroDeMano1 = 1;
        int numeroDeMano2 = 2;

        var mano1 = PartidaServicio.CrearManoNueva(numeroDeMano: numeroDeMano1);
        var mano2 = PartidaServicio.CrearManoNueva(numeroDeMano: numeroDeMano2);

        Assert.Equal(mano1.ManoIniciadaPor, mano1.TurnoActual);
        Assert.Equal(mano2.ManoIniciadaPor, mano2.TurnoActual);
    }

    [Fact]
    public void CrearManoNueva_CadaLlamadaGeneraIdUnico()
    {
        var mano1 = PartidaServicio.CrearManoNueva();
        var mano2 = PartidaServicio.CrearManoNueva();

        Assert.NotEqual(mano1.Id, mano2.Id);
    }
}
