using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class MentiraEnvidoServicioTests
{
    // ─── NivelMentira = 0 (sin aleatoriedad) ────────────────────────

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_RetornaTantoNormalizado()
    {
        // nivelMentira = 0 → nunca miente → retorna tantoBase
        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(25, 0, out _);
        Assert.Equal(25, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_Tanto0_Retorna0()
    {
        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(0, 0, out _);
        Assert.Equal(0, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_Tanto33_Retorna33()
    {
        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(33, 0, out _);
        Assert.Equal(33, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_Tanto7_Retorna7()
    {
        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(7, 0, out _);
        Assert.Equal(7, resultado);
    }

    // ─── Normalización: tantos entre 8 y 19 mapean a 7 ──────────────

    [Fact]
    public void ObtenerTantoCantado_TantoEntre8Y19_SeNormalizaA7()
    {
        // NormalizarTantoCantado(15) = 7
        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(15, 0, out _);
        Assert.Equal(7, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_Tanto8_SeNormalizaA7()
    {
        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(8, 0, out _);
        Assert.Equal(7, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_Tanto19_SeNormalizaA7()
    {
        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(19, 0, out _);
        Assert.Equal(7, resultado);
    }

    [Fact]
    public void ObtenerTantoCantado_Tanto20_NoSeNormalizaA7()
    {
        // 20 está fuera del rango 8-19, no se mapea a 7
        int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(20, 0, out _);
        Assert.Equal(20, resultado);
    }

    // ─── mintio = false cuando nivelMentira = 0 ─────────────────────

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_MintioEsFalse()
    {
        MentiraEnvidoServicio.ObtenerTantoCantado(25, 0, out bool mintio);
        Assert.False(mintio);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraCero_Tanto0_MintioEsFalse()
    {
        MentiraEnvidoServicio.ObtenerTantoCantado(0, 0, out bool mintio);
        Assert.False(mintio);
    }

    // ─── Resultado siempre en rango válido ───────────────────────────

    [Fact]
    public void ObtenerTantoCantado_ResultadoEsUnTantoValido_NivelMentiraCero()
    {
        // Con nivelMentira = 0 el resultado es siempre tantoBase normalizado
        // tantoBase: 0-7 o 20-33 (los 8-19 mapean a 7)
        var tantosValidos = Enumerable.Range(0, 8).Concat(Enumerable.Range(20, 14)).ToHashSet();

        foreach (int tanto in new[] { 0, 5, 7, 8, 15, 19, 20, 25, 30, 33 })
        {
            int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(tanto, 0, out _);
            Assert.Contains(resultado, tantosValidos);
        }
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraAlto_ResultadoSiempreEnRangoValido()
    {
        // Con nivelMentira = 100 puede mentir, pero el resultado debe seguir siendo un tanto válido
        var tantosValidos = Enumerable.Range(0, 8).Concat(Enumerable.Range(20, 14)).ToHashSet();

        for (int i = 0; i < 50; i++)
        {
            int resultado = MentiraEnvidoServicio.ObtenerTantoCantado(5, 100, out _);
            Assert.Contains(resultado, tantosValidos);
        }
    }

    // ─── Bounds de nivelMentira ──────────────────────────────────────

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraNegativo_NoLanzaExcepcion()
    {
        var ex = Record.Exception(() =>
            MentiraEnvidoServicio.ObtenerTantoCantado(25, -100, out _));
        Assert.Null(ex);
    }

    [Fact]
    public void ObtenerTantoCantado_NivelMentiraSuperior100_NoLanzaExcepcion()
    {
        var ex = Record.Exception(() =>
            MentiraEnvidoServicio.ObtenerTantoCantado(25, 999, out _));
        Assert.Null(ex);
    }
}
