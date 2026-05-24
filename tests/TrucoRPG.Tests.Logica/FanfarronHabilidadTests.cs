using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class FanfarronHabilidadTests
{
    private static Carta C(int numero, string palo) =>
        new() { Numero = numero, Palo = palo, ValorTruco = 0 };

    private static ManoTruco CrearManoEmpateEnvido(bool conFanfarron, string manoIniciadaPor)
    {
        var config = new ConfiguracionPartida
        {
            Modo = ModoJuego.Historia,
            HeroeDelHumano = conFanfarron ? ClaseHeroe.Fanfarron : ClaseHeroe.Timbero
        };

        return new ManoTruco
        {
            Configuracion = config,
            ManoIniciadaPor = manoIniciadaPor,
            Humano = new Jugador
            {
                Id = IdJugador.Humano,
                Mano = new List<Carta> { C(6, "Espada"), C(4, "Oro"), C(10, "Copa") }
            },
            Maquina = new Jugador
            {
                Id = IdJugador.Maquina,
                Mano = new List<Carta> { C(6, "Basto"), C(4, "Copa"), C(10, "Espada") }
            },
            TipoEnvidoCantado = "Envido"
        };
    }

    [Fact]
    public void ResolverEnvido_EmpateConFanfarron_GanaHumanoAunqueNoSeaMano()
    {
        var mano = CrearManoEmpateEnvido(conFanfarron: true, manoIniciadaPor: IdJugador.Maquina);

        EnvidoServicio.ResolverEnvido(mano, 2, "Test");

        Assert.Equal(6, mano.TantoHumano);
        Assert.Equal(6, mano.TantoMaquina);
        Assert.Equal(IdJugador.Humano, mano.GanadorEnvido);
    }

    [Fact]
    public void ResolverEnvido_EmpateSinFanfarron_GanaMano()
    {
        var mano = CrearManoEmpateEnvido(conFanfarron: false, manoIniciadaPor: IdJugador.Maquina);

        EnvidoServicio.ResolverEnvido(mano, 2, "Test");

        Assert.Equal(IdJugador.Maquina, mano.GanadorEnvido);
    }

    [Fact]
    public void ResolverEnvido_EmpateConFanfarron_NoAplicaEnModoTradicional()
    {
        var mano = CrearManoEmpateEnvido(conFanfarron: true, manoIniciadaPor: IdJugador.Maquina);
        mano.Configuracion = new ConfiguracionPartida { Modo = ModoJuego.Tradicional };

        EnvidoServicio.ResolverEnvido(mano, 2, "Test");

        Assert.Equal(IdJugador.Maquina, mano.GanadorEnvido);
    }
}
