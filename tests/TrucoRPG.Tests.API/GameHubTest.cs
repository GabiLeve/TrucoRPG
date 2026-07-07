using Microsoft.AspNetCore.SignalR;
using Moq;
using TrucoRPG.API.Hubs;
using TrucoRPG.API.Services;
using Xunit;

namespace TrucoRPG.Tests;

/// <summary>
/// Tests del GameHub: SalaService real (estado en memoria) + mocks de SignalR
/// (Context / Clients / Groups). SendAsync es un extension method sobre
/// SendCoreAsync, así que se verifica contra SendCoreAsync.
/// </summary>
public class GameHubTests
{
    private readonly SalaService _salas = new();
    private readonly Mock<IGroupManager> _groups = new();

    // Un proxy por destino (grupo o cliente) para poder verificar quién recibió qué.
    private readonly Dictionary<string, Mock<IClientProxy>> _groupProxies = new();
    private readonly Dictionary<string, Mock<ISingleClientProxy>> _clientProxies = new();

    private Mock<IClientProxy> GroupProxy(string grupo)
    {
        if (!_groupProxies.TryGetValue(grupo, out var p))
            _groupProxies[grupo] = p = new Mock<IClientProxy>();
        return p;
    }

    private Mock<ISingleClientProxy> ClientProxy(string connId)
    {
        if (!_clientProxies.TryGetValue(connId, out var p))
            _clientProxies[connId] = p = new Mock<ISingleClientProxy>();
        return p;
    }

    private GameHub CrearHub(string connectionId)
    {
        var context = new Mock<HubCallerContext>();
        context.SetupGet(c => c.ConnectionId).Returns(connectionId);

        var clients = new Mock<IHubCallerClients>();
        clients.Setup(c => c.Group(It.IsAny<string>()))
               .Returns<string>(g => GroupProxy(g).Object);
        clients.Setup(c => c.OthersInGroup(It.IsAny<string>()))
               .Returns<string>(g => GroupProxy(g).Object);
        clients.Setup(c => c.Client(It.IsAny<string>()))
               .Returns<string>(id => ClientProxy(id).Object);

        return new GameHub(_salas)
        {
            Context = context.Object,
            Clients = clients.Object,
            Groups  = _groups.Object,
        };
    }

    private static void VerificarEnviado(Mock<IClientProxy> proxy, string evento, Times times) =>
        proxy.Verify(p => p.SendCoreAsync(evento, It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), times);

    private static void VerificarEnviado(Mock<ISingleClientProxy> proxy, string evento, Times times) =>
        proxy.Verify(p => p.SendCoreAsync(evento, It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), times);

    /// <summary>Crea una sala con N jugadores (conn "p1".."pN") y devuelve el código.</summary>
    private async Task<string> ArmarSala(string modo, int jugadores, bool publica = false)
    {
        var codigo = await CrearHub("p1").CrearSala(modo, publica);
        for (int i = 2; i <= jugadores; i++)
            await CrearHub($"p{i}").UnirseASala(codigo);
        return codigo;
    }

    /// <summary>Todos marcan listo → arranca la partida del modo.</summary>
    private async Task<string> ArmarPartida(string modo, int jugadores)
    {
        var codigo = await ArmarSala(modo, jugadores);
        for (int i = 1; i <= jugadores; i++)
            await CrearHub($"p{i}").ListoParaJugar();
        return codigo;
    }

    // ── Salas ─────────────────────────────────────────────────────

    [Fact]
    public async Task CrearSala_DevuelveCodigoYAgregaAlGrupo()
    {
        var codigo = await CrearHub("p1").CrearSala("1v1", publica: false);

        Assert.Equal(6, codigo.Length);
        _groups.Verify(g => g.AddToGroupAsync("p1", codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnirseASala_CodigoInexistente_DevuelveFalse()
    {
        var ok = await CrearHub("p1").UnirseASala("NOEXIS");
        Assert.False(ok);
    }

    [Fact]
    public async Task UnirseASala_1v1Completa_NotificaSalaLista()
    {
        var codigo = await ArmarSala("1v1", 2);

        VerificarEnviado(GroupProxy(codigo), "SalaLista", Times.Once());
    }

    [Fact]
    public async Task UnirseASala_2v2_NotificaLobbyYSalaCompleta()
    {
        var codigo = await ArmarSala("2v2", 4);

        VerificarEnviado(GroupProxy(codigo), "LobbyActualizado", Times.Exactly(3));
        VerificarEnviado(GroupProxy(codigo), "SalaCompleta", Times.Once());
    }

    [Fact]
    public async Task UnirseASala_SalaLlena_DevuelveFalse()
    {
        var codigo = await ArmarSala("1v1", 2);

        var ok = await CrearHub("intruso").UnirseASala(codigo);

        Assert.False(ok);
    }

    [Fact]
    public async Task ListarSalasPublicas_SoloDevuelveLasPublicasConLugar()
    {
        await CrearHub("pub").CrearSala("1v1", publica: true);
        await CrearHub("priv").CrearSala("1v1", publica: false);

        var salas = await CrearHub("otro").ListarSalasPublicas("1v1");

        Assert.Single(salas);
    }

    [Fact]
    public async Task AbandonarSala_NotificaAlRestoDelGrupo()
    {
        var codigo = await ArmarSala("1v1", 2);

        await CrearHub("p2").AbandonarSala();

        _groups.Verify(g => g.RemoveFromGroupAsync("p2", codigo, It.IsAny<CancellationToken>()), Times.Once);
        VerificarEnviado(GroupProxy(codigo), "JugadorDesconectado", Times.Once());
    }

    // ── Equipos ───────────────────────────────────────────────────

    [Fact]
    public async Task ElegirEquipo_BroadcasteaEstadoEquiposACadaJugador()
    {
        await ArmarSala("2v2", 4);

        await CrearHub("p1").ElegirEquipo("sanMartin");

        VerificarEnviado(ClientProxy("p1"), "EstadoEquipos", Times.Once());
        VerificarEnviado(ClientProxy("p4"), "EstadoEquipos", Times.Once());
    }

    [Fact]
    public async Task ElegirEquipo_EquipoInvalido_NoBroadcastea()
    {
        await ArmarSala("2v2", 4);

        await CrearHub("p1").ElegirEquipo("riverPlate");

        VerificarEnviado(ClientProxy("p1"), "EstadoEquipos", Times.Never());
    }

    // ── Listo para jugar / inicio de partida ─────────────────────

    [Fact]
    public async Task ListoParaJugar_1v1_ConAmbosListos_EnviaTrucoEstadoACadaUno()
    {
        var codigo = await ArmarSala("1v1", 2);

        await CrearHub("p1").ListoParaJugar();
        VerificarEnviado(ClientProxy("p1"), "TrucoEstado", Times.Never()); // falta uno

        await CrearHub("p2").ListoParaJugar();

        VerificarEnviado(GroupProxy(codigo), "LobbyListos", Times.Exactly(2));
        VerificarEnviado(ClientProxy("p1"), "TrucoEstado", Times.Once());
        VerificarEnviado(ClientProxy("p2"), "TrucoEstado", Times.Once());
        Assert.NotNull(_salas.GetEstado1v1(codigo));
    }

    [Fact]
    public async Task ListoParaJugar_2v2_ConTodosListos_EnviaTrucoEstado2v2ALosCuatro()
    {
        var codigo = await ArmarPartida("2v2", 4);

        for (int i = 1; i <= 4; i++)
            VerificarEnviado(ClientProxy($"p{i}"), "TrucoEstado2v2", Times.Once());
        Assert.NotNull(_salas.GetEstado2v2(codigo));
    }

    [Fact]
    public async Task ListoParaJugar_3v3_ConTodosListos_EnviaTrucoEstado3v3ALosSeis()
    {
        var codigo = await ArmarPartida("3v3", 6);

        for (int i = 1; i <= 6; i++)
            VerificarEnviado(ClientProxy($"p{i}"), "TrucoEstado3v3", Times.Once());
        Assert.NotNull(_salas.GetEstado3v3(codigo));
    }

    [Fact]
    public async Task IniciarTruco_2v2_CreaEstadoYBroadcastea()
    {
        var codigo = await ArmarSala("2v2", 4);

        await CrearHub("p1").IniciarTruco();

        Assert.NotNull(_salas.GetEstado2v2(codigo));
        VerificarEnviado(ClientProxy("p1"), "TrucoEstado2v2", Times.Once());
    }

    [Fact]
    public async Task IniciarTruco_3v3_CreaEstadoYBroadcastea()
    {
        var codigo = await ArmarSala("3v3", 6);

        await CrearHub("p1").IniciarTruco();

        Assert.NotNull(_salas.GetEstado3v3(codigo));
        VerificarEnviado(ClientProxy("p6"), "TrucoEstado3v3", Times.Once());
    }

    [Fact]
    public async Task IniciarTruco_SinSala_NoHaceNada()
    {
        await CrearHub("suelto").IniciarTruco();
        // no lanza y no broadcastea nada
        Assert.Empty(_clientProxies);
    }

    // ── Jugadas 2v2 (partida real en memoria) ─────────────────────

    [Fact]
    public async Task JugarCarta2v2_EnSuTurno_ActualizaEstadoYBroadcastea()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var state  = _salas.GetEstado2v2(codigo)!;

        // Juega el que tiene el turno, con una carta real de su mano
        var rolTurno  = state.Mano.TurnoActual;
        var connTurno = state.Posiciones.First(kv => $"J{kv.Value}" == rolTurno).Key;
        var carta     = state.Mano.ObtenerJugador(rolTurno)!.Mano[0];

        await CrearHub(connTurno).JugarCarta2v2(carta.Numero, carta.Palo);

        Assert.Single(state.Mano.ObtenerJugador(rolTurno)!.Jugadas);
        VerificarEnviado(ClientProxy(connTurno), "TrucoEstado2v2", Times.Exactly(2));
    }

    [Fact]
    public async Task JugarCarta2v2_FueraDeTurno_NoHaceNada()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var state  = _salas.GetEstado2v2(codigo)!;

        var rolTurno = state.Mano.TurnoActual;
        var otroConn = state.Posiciones.First(kv => $"J{kv.Value}" != rolTurno).Key;
        var otroRol  = $"J{state.Posiciones[otroConn]}";
        var carta    = state.Mano.ObtenerJugador(otroRol)!.Mano[0];

        await CrearHub(otroConn).JugarCarta2v2(carta.Numero, carta.Palo);

        Assert.Empty(state.Mano.ObtenerJugador(otroRol)!.Jugadas);
    }

    [Fact]
    public async Task NuevaMano2v2_ConManoEnCurso_NoReinicia()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var state  = _salas.GetEstado2v2(codigo)!;

        await CrearHub("p1").NuevaMano2v2();

        Assert.Same(state, _salas.GetEstado2v2(codigo)); // mismo estado, no se creó otro
    }

    // ── Señas ─────────────────────────────────────────────────────

    [Fact]
    public async Task EnviarSenia2v2_LlegaSoloAlCompanero()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var state  = _salas.GetEstado2v2(codigo)!;

        var pos1 = state.Posiciones.First(kv => kv.Value == 1).Key;
        var pos2 = state.Posiciones.First(kv => kv.Value == 2).Key;
        var pos3 = state.Posiciones.First(kv => kv.Value == 3).Key;
        var pos4 = state.Posiciones.First(kv => kv.Value == 4).Key;

        await CrearHub(pos1).EnviarSenia2v2("anchoEspada");

        VerificarEnviado(ClientProxy(pos3), "RecibirSenia2v2", Times.Once());
        VerificarEnviado(ClientProxy(pos2), "RecibirSenia2v2", Times.Never());
        VerificarEnviado(ClientProxy(pos4), "RecibirSenia2v2", Times.Never());
    }

    [Fact]
    public async Task EnviarSenia2v2_SinPartida_NoHaceNada()
    {
        await CrearHub("suelto").EnviarSenia2v2("anchoEspada");
        Assert.Empty(_clientProxies);
    }

    [Fact]
    public async Task EnviarSenia3v3_LlegaSoloALosDosCompaneros()
    {
        var codigo = await ArmarPartida("3v3", 6);
        var state  = _salas.GetEstado3v3(codigo)!;

        string Conn(int pos) => state.Posiciones.First(kv => kv.Value == pos).Key;

        // Posición 1 (EquipoA) → compañeros en 3 y 5; rivales 2, 4 y 6
        await CrearHub(Conn(1)).EnviarSenia3v3("sieteOro");

        VerificarEnviado(ClientProxy(Conn(3)), "RecibirSenia3v3", Times.Once());
        VerificarEnviado(ClientProxy(Conn(5)), "RecibirSenia3v3", Times.Once());
        VerificarEnviado(ClientProxy(Conn(2)), "RecibirSenia3v3", Times.Never());
        VerificarEnviado(ClientProxy(Conn(4)), "RecibirSenia3v3", Times.Never());
        VerificarEnviado(ClientProxy(Conn(6)), "RecibirSenia3v3", Times.Never());
    }

    // ── Desconexión ───────────────────────────────────────────────

    [Fact]
    public async Task OnDisconnected_ConJugadoresRestantes_NotificaAlGrupo()
    {
        var codigo = await ArmarSala("1v1", 2);

        await CrearHub("p2").OnDisconnectedAsync(null);

        VerificarEnviado(GroupProxy(codigo), "JugadorDesconectado", Times.Once());
    }
}
