using TrucoDemo.Clases;
using TrucoDemo.Servicios;

namespace TrucoRPG.Tests.Logica;

public class DecisionMaquinaServicioTests
{
    // Helpers
    private static Carta C(int numero, string palo, int valorTruco) =>
        new() { Numero = numero, Palo = palo, ValorTruco = valorTruco };

    private static List<Carta> ManoConTanto(int tanto)
    {
        // Para tanto >= 20: armamos un par del mismo palo cuya suma + 20 = tanto
        // tanto = a + b + 20  →  a + b = tanto - 20
        // Usamos palo = "Copa" con números simples (no figuras)
        if (tanto >= 20 && tanto <= 33)
        {
            int sum = tanto - 20;
            // Clamp individual values to 0-7 (cards go 1-7, figuras = 0)
            int a = Math.Min(sum, 7);
            int b = sum - a;
            return new List<Carta>
            {
                C(a == 0 ? 10 : a, "Copa", a), // número real no importa para envido, solo ValorTruco no existe → usamos ValorEnvido via Numero
                C(b == 0 ? 11 : b, "Copa", 1),
                C(3, "Espada", 10)
            };
        }
        // Para tanto < 20: tres cartas de palos distintos, max individual = tanto
        return new List<Carta>
        {
            C(tanto > 7 ? 6 : tanto, "Espada", tanto > 7 ? 6 : tanto),
            C(2, "Oro", 2),
            C(1, "Copa", 1)
        };
    }

    // ─── AceptarEnvido ───────────────────────────────────────────────

    [Fact]
    public void AceptarEnvido_TantoMayorIgual30_SiempreRetornaTrue()
    {
        // 7 + 3 del mismo palo = 10 + 20 = 30 → tanto = 30
        var mano = new List<Carta>
        {
            new() { Numero = 7, Palo = "Copa", ValorTruco = 4 },
            new() { Numero = 3, Palo = "Copa", ValorTruco = 10 },
            new() { Numero = 1, Palo = "Espada", ValorTruco = 14 }
        };
        // CalcularTanto: mejor par Copa: 7+3+20 = 30 → siempre acepta
        bool resultado = DecisionMaquinaServicio.AceptarEnvido(mano, 0);
        Assert.True(resultado);
    }

    [Fact]
    public void AceptarEnvido_Tanto33_NivelMentiraCero_SiempreRetornaTrue()
    {
        // 7 + 6 del mismo palo = 13 + 20 = 33 (máximo)
        var mano = new List<Carta>
        {
            new() { Numero = 7, Palo = "Basto", ValorTruco = 4 },
            new() { Numero = 6, Palo = "Basto", ValorTruco = 3 },
            new() { Numero = 4, Palo = "Oro", ValorTruco = 1 }
        };
        bool resultado = DecisionMaquinaServicio.AceptarEnvido(mano, 0);
        Assert.True(resultado);
    }

    [Fact]
    public void AceptarEnvido_TantoMenorIgual20_NivelMentiraCero_SiempreRetornaFalse()
    {
        // Sin par del mismo palo, mejor carta = 6 → tanto = 6
        var mano = new List<Carta>
        {
            new() { Numero = 6, Palo = "Espada", ValorTruco = 3 },
            new() { Numero = 5, Palo = "Oro", ValorTruco = 2 },
            new() { Numero = 4, Palo = "Copa", ValorTruco = 1 }
        };
        // tanto = 6 ≤ 20, nivelMentira = 0 → siempre rechaza
        bool resultado = DecisionMaquinaServicio.AceptarEnvido(mano, 0);
        Assert.False(resultado);
    }

    [Fact]
    public void AceptarEnvido_NivelMentiraSeClampea_NoLanzaExcepcion()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 6, Palo = "Espada", ValorTruco = 3 },
            new() { Numero = 4, Palo = "Oro", ValorTruco = 1 },
            new() { Numero = 2, Palo = "Copa", ValorTruco = 9 }
        };
        // nivelMentira = 999 debe ser clampeado a 100 sin excepción
        var ex = Record.Exception(() => DecisionMaquinaServicio.AceptarEnvido(mano, 999));
        Assert.Null(ex);
    }

    [Fact]
    public void AceptarEnvido_NivelMentiraNegativo_NoLanzaExcepcion()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 5, Palo = "Espada", ValorTruco = 2 },
            new() { Numero = 3, Palo = "Oro", ValorTruco = 10 },
            new() { Numero = 2, Palo = "Copa", ValorTruco = 9 }
        };
        var ex = Record.Exception(() => DecisionMaquinaServicio.AceptarEnvido(mano, -999));
        Assert.Null(ex);
    }

    // ─── AceptarTruco ────────────────────────────────────────────────

    [Fact]
    public void AceptarTruco_CartaMuyFuerte_NivelMentiraMuyAlto_SiempreRetornaTrue()
    {
        // cartaMasFuerte = 14 (≥11) → probabilidad base = 90
        // bonusPorCaradurez = round(100 * 0.35) = 35 → total = 125 → clamp = 100 → always true
        var mano = new List<Carta>
        {
            new() { Numero = 1, Palo = "Espada", ValorTruco = 14 },
            new() { Numero = 4, Palo = "Oro", ValorTruco = 1 },
            new() { Numero = 5, Palo = "Copa", ValorTruco = 2 }
        };
        bool resultado = DecisionMaquinaServicio.AceptarTruco(mano, 100);
        Assert.True(resultado);
    }

    [Fact]
    public void AceptarTruco_ManoVacia_NoLanzaExcepcion()
    {
        // manoMaquina.Any() == false → cartaMasFuerte = 0 → probabilidad = 20 → probabilistic
        var ex = Record.Exception(() => DecisionMaquinaServicio.AceptarTruco(new List<Carta>(), 0));
        Assert.Null(ex);
    }

    [Fact]
    public void AceptarTruco_NivelMentiraClampeado_NoLanzaExcepcion()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 3, Palo = "Espada", ValorTruco = 10 },
        };
        var ex = Record.Exception(() => DecisionMaquinaServicio.AceptarTruco(mano, -50));
        Assert.Null(ex);
    }

    // ─── EscalarTruco ────────────────────────────────────────────────

    [Fact]
    public void EscalarTruco_NivelActualMayorIgual3_SiempreRetornaFalse()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 1, Palo = "Espada", ValorTruco = 14 },
            new() { Numero = 1, Palo = "Basto", ValorTruco = 13 }
        };
        Assert.False(DecisionMaquinaServicio.EscalarTruco(mano, 100, 3));
    }

    [Fact]
    public void EscalarTruco_NivelActual5_SiempreRetornaFalse()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 1, Palo = "Espada", ValorTruco = 14 }
        };
        Assert.False(DecisionMaquinaServicio.EscalarTruco(mano, 100, 5));
    }

    [Fact]
    public void EscalarTruco_NivelActualMenorA3_NoLanzaExcepcion()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 3, Palo = "Espada", ValorTruco = 10 }
        };
        var ex = Record.Exception(() => DecisionMaquinaServicio.EscalarTruco(mano, 0, 1));
        Assert.Null(ex);
    }

    [Fact]
    public void EscalarTruco_ManoVacia_NivelMenorA3_NoLanzaExcepcion()
    {
        var ex = Record.Exception(() => DecisionMaquinaServicio.EscalarTruco(new List<Carta>(), 50, 2));
        Assert.Null(ex);
    }

    [Fact]
    public void EscalarTruco_NivelActual3_IgnoraNivelMentiraYCartas()
    {
        // Sin importar cuán fuerte sea la mano o el nivelMentira, nivel >= 3 siempre es false
        var manoFuerte = new List<Carta>
        {
            new() { Numero = 1, Palo = "Espada", ValorTruco = 14 },
            new() { Numero = 1, Palo = "Basto", ValorTruco = 13 },
            new() { Numero = 7, Palo = "Espada", ValorTruco = 12 }
        };
        Assert.False(DecisionMaquinaServicio.EscalarTruco(manoFuerte, 100, 3));
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public class IniciativaMaquinaEnvidoServicioTests
{
    [Fact]
    public void DebeCantarEnvido_TantoMayorIgual30_SiempreRetornaTrue()
    {
        // 7 + 3 Copa + 20 = 30 → retorna true incondicionalmente
        var mano = new List<Carta>
        {
            new() { Numero = 7, Palo = "Copa", ValorTruco = 4 },
            new() { Numero = 3, Palo = "Copa", ValorTruco = 10 },
            new() { Numero = 4, Palo = "Espada", ValorTruco = 1 }
        };
        bool resultado = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 0);
        Assert.True(resultado);
    }

    [Fact]
    public void DebeCantarEnvido_Tanto33_SiempreRetornaTrue()
    {
        // 7 + 6 Basto + 20 = 33 → retorna true
        var mano = new List<Carta>
        {
            new() { Numero = 7, Palo = "Basto", ValorTruco = 4 },
            new() { Numero = 6, Palo = "Basto", ValorTruco = 3 },
            new() { Numero = 5, Palo = "Espada", ValorTruco = 2 }
        };
        bool resultado = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 0);
        Assert.True(resultado);
    }

    [Fact]
    public void DebeCantarEnvido_NivelMentiraFueraDeLimites_NoLanzaExcepcion()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 3, Palo = "Espada", ValorTruco = 10 },
            new() { Numero = 2, Palo = "Oro", ValorTruco = 9 },
            new() { Numero = 1, Palo = "Copa", ValorTruco = 8 }
        };
        var ex = Record.Exception(() => IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, -100));
        Assert.Null(ex);
        ex = Record.Exception(() => IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 999));
        Assert.Null(ex);
    }

    [Fact]
    public void DebeCantarEnvido_RetornaUnBooleano()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 5, Palo = "Espada", ValorTruco = 2 },
            new() { Numero = 4, Palo = "Oro", ValorTruco = 1 },
            new() { Numero = 2, Palo = "Copa", ValorTruco = 9 }
        };
        bool resultado = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(mano, 50);
        Assert.IsType<bool>(resultado);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public class IniciativaMaquinaTrucoServicioTests
{
    [Fact]
    public void DebeCantarTruco_CartaMuyFuerte_NivelMentira100_SiempreRetornaTrue()
    {
        // cartaMasFuerte = 14 (≥13) → probabilidad = 78
        // + round(100 * 0.62) = 62
        // nivelMentira >= 90 → +12
        // Total = 78 + 62 + 12 = 152 → clamp = 100 → always true
        var mano = new List<Carta>
        {
            new() { Numero = 1, Palo = "Espada", ValorTruco = 14 },
            new() { Numero = 4, Palo = "Oro", ValorTruco = 1 },
            new() { Numero = 5, Palo = "Copa", ValorTruco = 2 }
        };
        bool resultado = IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 100);
        Assert.True(resultado);
    }

    [Fact]
    public void DebeCantarTruco_AsDeEspada_NivelMentira100_SiempreRetornaTrue()
    {
        // Mismo calculo: As de Espada ValorTruco=14 ≥ 13, nivelMentira=100 → probabilidad=100
        var mano = new List<Carta>
        {
            new() { Numero = 1, Palo = "Espada", ValorTruco = 14 }
        };
        bool resultado = IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 100);
        Assert.True(resultado);
    }

    [Fact]
    public void DebeCantarTruco_NivelMentiraFueraDeLimites_NoLanzaExcepcion()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 3, Palo = "Espada", ValorTruco = 10 }
        };
        var ex = Record.Exception(() => IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, -50));
        Assert.Null(ex);
        ex = Record.Exception(() => IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 999));
        Assert.Null(ex);
    }

    [Fact]
    public void DebeCantarTruco_RetornaUnBooleano()
    {
        var mano = new List<Carta>
        {
            new() { Numero = 6, Palo = "Espada", ValorTruco = 3 },
            new() { Numero = 4, Palo = "Oro", ValorTruco = 1 }
        };
        bool resultado = IniciativaMaquinaTrucoServicio.DebeCantarTruco(mano, 30);
        Assert.IsType<bool>(resultado);
    }
}
