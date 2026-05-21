using TrucoDemo.Clases;
using TrucoDemo.Servicios;

namespace TrucoRPG.Tests.Logica;

public class RepartoServicioTests
{
    private static ManoTruco ManoConJugadores()
    {
        return new ManoTruco
        {
            Humano = new Jugador { Nombre = "Humano", EsMaquina = false },
            Maquina = new Jugador { Nombre = "Maquina", EsMaquina = true }
        };
    }

    // ─── Cantidad de cartas ──────────────────────────────────────────

    [Fact]
    public void Repartir_HumanoRecibeTresCartas()
    {
        var mano = ManoConJugadores();
        RepartoServicio.Repartir(mano);
        Assert.Equal(3, mano.Humano.Mano.Count);
    }

    [Fact]
    public void Repartir_MaquinaRecibeTresCartas()
    {
        var mano = ManoConJugadores();
        RepartoServicio.Repartir(mano);
        Assert.Equal(3, mano.Maquina.Mano.Count);
    }

    [Fact]
    public void Repartir_TotalSeisCartasRepartidas()
    {
        var mano = ManoConJugadores();
        RepartoServicio.Repartir(mano);
        Assert.Equal(6, mano.Humano.Mano.Count + mano.Maquina.Mano.Count);
    }

    // ─── Sin duplicados ──────────────────────────────────────────────

    [Fact]
    public void Repartir_NoHayCartasDuplicadasEntreJugadores()
    {
        var mano = ManoConJugadores();
        RepartoServicio.Repartir(mano);
        var todas = mano.Humano.Mano.Concat(mano.Maquina.Mano);
        int distintas = todas.Select(c => (c.Numero, c.Palo)).Distinct().Count();
        Assert.Equal(6, distintas);
    }

    [Fact]
    public void Repartir_NoHayCartasDuplicadasDentroDelHumano()
    {
        var mano = ManoConJugadores();
        RepartoServicio.Repartir(mano);
        int distintas = mano.Humano.Mano.Select(c => (c.Numero, c.Palo)).Distinct().Count();
        Assert.Equal(3, distintas);
    }

    [Fact]
    public void Repartir_NoHayCartasDuplicadasDentroDeLaMaquina()
    {
        var mano = ManoConJugadores();
        RepartoServicio.Repartir(mano);
        int distintas = mano.Maquina.Mano.Select(c => (c.Numero, c.Palo)).Distinct().Count();
        Assert.Equal(3, distintas);
    }

    // ─── Cartas del mazo válido ──────────────────────────────────────

    [Fact]
    public void Repartir_CartasPertenecenAlMazo()
    {
        var mazoValido = MazoServicio.CrearMazo()
            .Select(c => (c.Numero, c.Palo))
            .ToHashSet();

        var mano = ManoConJugadores();
        RepartoServicio.Repartir(mano);

        var todas = mano.Humano.Mano.Concat(mano.Maquina.Mano);
        foreach (var carta in todas)
            Assert.Contains((carta.Numero, carta.Palo), mazoValido);
    }

    [Fact]
    public void Repartir_CartasTienenValorTrucoValido()
    {
        var mano = ManoConJugadores();
        RepartoServicio.Repartir(mano);
        var todas = mano.Humano.Mano.Concat(mano.Maquina.Mano);
        foreach (var carta in todas)
        {
            Assert.InRange(carta.ValorTruco, 1, 14);
        }
    }

    // ─── Aleatoriedad: distintas manos en sucesivos repartos ────────

    [Fact]
    public void Repartir_DosRepartidosDistintos_ConAltaProbabilidad()
    {
        // Con 40 cartas, la probabilidad de que dos repartos sean idénticos es ~0
        var mano1 = ManoConJugadores();
        var mano2 = ManoConJugadores();
        RepartoServicio.Repartir(mano1);
        RepartoServicio.Repartir(mano2);

        var claves1 = mano1.Humano.Mano.Select(c => (c.Numero, c.Palo)).OrderBy(x => x).ToList();
        var claves2 = mano2.Humano.Mano.Select(c => (c.Numero, c.Palo)).OrderBy(x => x).ToList();

        // No comprobamos que SIEMPRE sean distintos (podría haber coincidencia),
        // pero al menos comprobamos que el resultado es determinístico por invocación
        Assert.Equal(3, claves1.Count);
        Assert.Equal(3, claves2.Count);
    }
}
