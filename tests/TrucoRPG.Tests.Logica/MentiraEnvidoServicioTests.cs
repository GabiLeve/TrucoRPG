using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class MentiraEnvidoServicioTests
{
    // ─── NivelMentira = 0 (sin aleatoriedad) ────────────────────────

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_RetornaTantoNormalizado()
    {
        // nivelMentira = 0 → nunca miente → retorna tantoBase
        int tanto = 25;
        int nivelMentira = 0;

        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);

        Assert.Equal(25, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_Tanto0_Retorna0()
    {
        int tanto = 0;
        int nivelMentira = 0;

        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);

        Assert.Equal(0, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_Tanto33_Retorna33()
    {
        int tanto = 33;
        int nivelMentira = 0;

        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);
        Assert.Equal(33, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_Tanto7_Retorna7()
    {
        int tanto = 7;
        int nivelMentira = 0;

        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);

        Assert.Equal(7, resultado);
    }

    // ─── Normalización: tantos entre 8 y 19 mapean a 7 ──────────────

    [Fact]
    public void ObtenerTantoCantado_TantoEntre8Y19_SeNormalizaA7()
    {
        // NormalizarTantoCantado(15) = 7
        int tanto = 15;
        int nivelMentira = 0;

        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);

        Assert.Equal(7, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_Tanto8_SeNormalizaA7()
    {
        int tanto = 8;
        int nivelMentira = 0;

        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);

        Assert.Equal(7, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_Tanto19_SeNormalizaA7()
    {
        int tanto = 19;
        int nivelMentira = 0;

        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);

        Assert.Equal(7, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_Tanto20_NoSeNormalizaA7()
    {
        // 20 está fuera del rango 8-19, no se mapea a 7
        int tanto = 20;
        int nivelMentira = 0;

        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);

        Assert.Equal(20, resultado);
    }

    // ─── mintio = false cuando nivelMentira = 0 ─────────────────────

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_MintioEsFalse()
    {
        int tanto = 25;
        int nivelMentira = 0;

        MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out bool mintio);

        Assert.False(mintio);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_Tanto0_MintioEsFalse()
    {
        int tanto = 0;
        int nivelMentira = 0;

        MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out bool mintio);

        Assert.False(mintio);
    }

    // ─── Resultado siempre en rango válido ───────────────────────────

    [Fact]
    public void ObtenerTantoCantado_ResultadoEsUnTantoValido_NivelMentiraCero()
    {
        // Con nivelMentira = 0 el resultado es siempre tantoBase normalizado
        // tantoBase: 0-7 o 20-33 (los 8-19 mapean a 7)
        var tantosValidos = Enumerable.Range(0, 8).Concat(Enumerable.Range(20, 14)).ToHashSet();
        int nivelMentira = 0;
        var tantosAProbar = new[] { 0, 5, 7, 8, 15, 19, 20, 25, 30, 33 };
        var resultadosObtenidos = new List<int>();
    
        foreach (int tanto in tantosAProbar)
        {
            int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, 0, out _);
            resultadosObtenidos.Add(resultado);
        }

        foreach (int resultado in resultadosObtenidos)
        {
            Assert.Contains(resultado, tantosValidos);
        }
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraAlto_ResultadoSiempreEnRangoValido()
    {
        // Con nivelMentira = 100 puede mentir, pero el resultado debe seguir siendo un tanto válido
        int tanto = 5;
        int nivelMentira = 100;
        var tantosValidos = Enumerable.Range(0, 8).Concat(Enumerable.Range(20, 14)).ToHashSet();
        var resultadosObtenidos = new List<int>();

        for (int i = 0; i < 50; i++)
        {
            int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _);
            resultadosObtenidos.Add(resultado);
        }

        foreach (int resultado in resultadosObtenidos)
        {
            Assert.Contains(resultado, tantosValidos);
        }
    }

    // ─── Bounds de nivelMentira ──────────────────────────────────────

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraNegativo_NoLanzaExcepcion()
    {
        int tanto = 25;
        int nivelMentira = -100;

        var ex = Record.Exception(() =>
            MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _));

        Assert.Null(ex);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraSuperior100_NoLanzaExcepcion()
    {
        int tanto = 25;
        int nivelMentira = 999;

        var ex = Record.Exception(() =>
            MentiraEnvidoServicio.ObtenerTantoCantado(tanto, nivelMentira, out _));

        Assert.Null(ex);
    }
}
