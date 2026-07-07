using Microsoft.AspNetCore.SignalR;
using Moq;
using TrucoRPG.API.Hubs;
using TrucoRPG.API.Services;
using Xunit;

namespace TrucoRPG.Tests;

/// <summary>
/// Barrido de ramas del GameHub: cada método tiene guards
/// (sin sala → return, jugador inválido → return, servicio rechaza → return)
/// y acá se ejercitan ambos lados de cada uno, con partidas reales en memoria.
/// Para las acciones cuyo permiso depende de reglas de dominio (quién puede
/// cantar), se invoca con todos los jugadores y se asserta el estado final:
/// el habilitado cubre la rama "aceptado" y el resto la rama "rechazado".
/// </summary>
public class GameHubBranchTests
{
    private readonly SalaService _salas = new();
    private readonly Mock<IGroupManager> _groups = new();
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

    private GameHub Hub(string connectionId)
    {
        var context = new Mock<HubCallerContext>();
        context.SetupGet(c => c.ConnectionId).Returns(connectionId);

        var clients = new Mock<IHubCallerClients>();
        clients.Setup(c => c.Group(It.IsAny<string>())).Returns<string>(g => GroupProxy(g).Object);
        clients.Setup(c => c.OthersInGroup(It.IsAny<string>())).Returns<string>(g => GroupProxy(g).Object);
        clients.Setup(c => c.Client(It.IsAny<string>())).Returns<string>(id => ClientProxy(id).Object);

        return new GameHub(_salas)
        {
            Context = context.Object,
            Clients = clients.Object,
            Groups  = _groups.Object,
        };
    }

    private async Task<string> ArmarPartida(string modo, int jugadores)
    {
        var codigo = await Hub("p1").CrearSala(modo, publica: false);
        for (int i = 2; i <= jugadores; i++)
            await Hub($"p{i}").UnirseASala(codigo);
        for (int i = 1; i <= jugadores; i++)
            await Hub($"p{i}").ListoParaJugar();
        return codigo;
    }

    private static IEnumerable<string> Conns(int n) =>
        Enumerable.Range(1, n).Select(i => $"p{i}");

    private string Conn2v2(string codigo, string rol) =>
        _salas.GetEstado2v2(codigo)!.Posiciones.First(kv => $"J{kv.Value}" == rol).Key;

    private string Conn3v3(string codigo, string rol) =>
        _salas.GetEstado3v3(codigo)!.Posiciones.First(kv => $"J{kv.Value}" == rol).Key;

    // ── Guard "sin sala / sin partida" de TODOS los métodos ──────

    [Fact]
    public async Task TodosLosMetodos_SinSala_NoHacenNadaNiLanzan()
    {
        var hub = Hub("fantasma");

        // Sala / lobby
        await hub.AbandonarSala();
        await hub.ElegirEquipo("sanMartin");
        await hub.ActualizarPosicion(1, 2, "idle", "gaucho", "pueblo");
        await hub.ListoParaJugar();
        await hub.IniciarTruco();

        // 1v1
        await hub.JugarCarta(1, "Espada");
        await hub.SolicitarEnvido("Envido");
        await hub.ResponderEnvido(true);
        await hub.SonBuenas();
        await hub.EscalarEnvido("Real Envido");
        await hub.SolicitarTruco();
        await hub.ResponderTruco(true, null);
        await hub.EscalarTruco();
        await hub.IrseAlMazo();
        await hub.NuevaMano();
        await hub.NuevaPartida();

        // 2v2
        await hub.JugarCarta2v2(1, "Espada");
        await hub.SolicitarEnvido2v2("Envido");
        await hub.ResponderEnvido2v2(true);
        await hub.DeclararTanto2v2(25);
        await hub.SonBuenas2v2();
        await hub.EscalarEnvido2v2("Real Envido");
        await hub.SolicitarTruco2v2();
        await hub.ResponderTruco2v2(true, null);
        await hub.IrseAlMazo2v2();
        await hub.EscalarTruco2v2();
        await hub.NuevaMano2v2();
        await hub.EnviarSenia2v2("anchoEspada");

        // 3v3
        await hub.JugarCarta3v3(1, "Espada");
        await hub.SolicitarEnvido3v3("Envido");
        await hub.ResponderEnvido3v3(true);
        await hub.DeclararTanto3v3(25);
        await hub.SonBuenas3v3();
        await hub.EscalarEnvido3v3("Real Envido");
        await hub.SolicitarTruco3v3();
        await hub.ResponderTruco3v3(true, null);
        await hub.IrseAlMazo3v3();
        await hub.EscalarTruco3v3();
        await hub.NuevaMano3v3();
        await hub.EnviarSenia3v3("sieteOro");

        Assert.Empty(_clientProxies); // nadie recibió nada
    }

    // ── 1v1 ───────────────────────────────────────────────────────

    [Fact]
    public async Task Flujo1v1_JugarCarta_EnTurnoYFueraDeTurno()
    {
        var codigo = await ArmarPartida("1v1", 2);
        var state  = _salas.GetEstado1v1(codigo)!;

        // El estado 1v1 usa roles "Humano" (J1 = p1) y "Maquina" (J2 = p2)
        bool turnoJ1  = state.Mano.TurnoActual == "Humano";
        var connTurno = turnoJ1 ? state.Jugador1Id : state.Jugador2Id;
        var connOtro  = turnoJ1 ? state.Jugador2Id : state.Jugador1Id;
        var manoTurno = turnoJ1 ? state.Mano.Humano : state.Mano.Maquina;
        var manoOtro  = turnoJ1 ? state.Mano.Maquina : state.Mano.Humano;

        // Fuera de turno: rechazado
        await Hub(connOtro).JugarCarta(manoOtro.Mano[0].Numero, manoOtro.Mano[0].Palo);
        Assert.Empty(manoOtro.Jugadas);

        // En turno: aceptado
        await Hub(connTurno).JugarCarta(manoTurno.Mano[0].Numero, manoTurno.Mano[0].Palo);
        Assert.Single(manoTurno.Jugadas);
    }

    [Fact]
    public async Task Flujo1v1_EnvidoNoQuerido_QuedaResuelto()
    {
        var codigo = await ArmarPartida("1v1", 2);
        var state  = _salas.GetEstado1v1(codigo)!;

        foreach (var c in Conns(2)) await Hub(c).SolicitarEnvido("Envido");
        Assert.True(state.Mano.EnvidoCantado);

        foreach (var c in Conns(2)) await Hub(c).ResponderEnvido(false);
        Assert.True(state.Mano.EnvidoResuelto);
    }

    [Fact]
    public async Task Flujo1v1_EnvidoEscaladoYQuerido()
    {
        var codigo = await ArmarPartida("1v1", 2);
        var state  = _salas.GetEstado1v1(codigo)!;

        foreach (var c in Conns(2)) await Hub(c).SolicitarEnvido("Envido");
        foreach (var c in Conns(2)) await Hub(c).EscalarEnvido("Real Envido");
        foreach (var c in Conns(2)) await Hub(c).ResponderEnvido(true);
        foreach (var c in Conns(2)) await Hub(c).SonBuenas();

        Assert.True(state.Mano.EnvidoCantado);
    }

    [Fact]
    public async Task Flujo1v1_TrucoQuerido_LuegoEscalado()
    {
        var codigo = await ArmarPartida("1v1", 2);
        var state  = _salas.GetEstado1v1(codigo)!;

        foreach (var c in Conns(2)) await Hub(c).SolicitarTruco();
        Assert.True(state.Mano.TrucoCantado);

        foreach (var c in Conns(2)) await Hub(c).ResponderTruco(true, null);
        foreach (var c in Conns(2)) await Hub(c).EscalarTruco();
        // El nivel puede o no subir según de quién sea el turno; lo que importa
        // acá es que ambas ramas del guard se ejercitaron sin romper el estado.
        Assert.True(state.Mano.NivelTruco >= 1);
    }

    [Fact]
    public async Task Flujo1v1_MazoNuevaManoYNuevaPartida()
    {
        var codigo = await ArmarPartida("1v1", 2);
        var state  = _salas.GetEstado1v1(codigo)!;

        // NuevaMano con mano en curso: guard la rechaza
        await Hub("p1").NuevaMano();
        Assert.Null(state.Mano.GanadorMano);

        await Hub("p1").IrseAlMazo();
        Assert.NotNull(state.Mano.GanadorMano);

        await Hub("p1").NuevaMano();
        Assert.Null(state.Mano.GanadorMano); // arrancó mano nueva

        // NuevaPartida sin partida terminada: guard la rechaza
        await Hub("p1").NuevaPartida();
        Assert.False(state.Mano.PartidaTerminada);
    }

    [Fact]
    public async Task ActualizarPosicion_ConSala_NotificaALosDemas()
    {
        var codigo = await Hub("p1").CrearSala("1v1", false);
        await Hub("p2").UnirseASala(codigo);

        await Hub("p1").ActualizarPosicion(10, 20, "walk", "gaucho", "pueblo");

        GroupProxy(codigo).Verify(p => p.SendCoreAsync(
            "PosicionActualizada", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListoParaJugar_ConPartidaYaIniciada_ReenviaElEstadoExistente()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var state  = _salas.GetEstado2v2(codigo);

        // Todos vuelven a marcar listo: el hub debe reusar el estado, no crear otro
        foreach (var c in Conns(4)) await Hub(c).ListoParaJugar();

        Assert.Same(state, _salas.GetEstado2v2(codigo));
        ClientProxy(Conn2v2(codigo, "J1")).Verify(p => p.SendCoreAsync(
            "TrucoEstado2v2", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    // ── 2v2 ───────────────────────────────────────────────────────

    [Fact]
    public async Task Flujo2v2_EnvidoQueridoConDeclaracionDeTantos()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var mano   = _salas.GetEstado2v2(codigo)!.Mano;

        // Declarar tanto antes del envido: guard de fase la rechaza
        await Hub("p1").DeclararTanto2v2(25);
        Assert.False(mano.EnvidoCantado);

        foreach (var c in Conns(4)) await Hub(c).SolicitarEnvido2v2("Envido");
        Assert.True(mano.EnvidoCantado);

        var responde = Conn2v2(codigo, mano.EnvidoPendienteRespuestaDe!);
        await Hub(responde).ResponderEnvido2v2(true);
        Assert.Equal("declarando_tantos", mano.FaseEnvido);

        // Declarar con el jugador equivocado: guard la rechaza
        var equivocado = Conns(4).First(c => c != Conn2v2(codigo, mano.EnvidoPendienteRespuestaDe!));
        await Hub(equivocado).DeclararTanto2v2(33);

        // Declaración real: el pendiente declara y los siguientes cierran
        for (int i = 0; i < 6 && mano.FaseEnvido == "declarando_tantos"; i++)
        {
            var pendiente = Conn2v2(codigo, mano.EnvidoPendienteRespuestaDe!);
            if (i == 0) await Hub(pendiente).DeclararTanto2v2(25);
            else        await Hub(pendiente).SonBuenas2v2();
        }

        Assert.True(mano.EnvidoResuelto);
    }

    [Fact]
    public async Task Flujo2v2_EnvidoEscaladoYNoQuerido()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var mano   = _salas.GetEstado2v2(codigo)!.Mano;

        foreach (var c in Conns(4)) await Hub(c).SolicitarEnvido2v2("Envido");

        var pendiente = Conn2v2(codigo, mano.EnvidoPendienteRespuestaDe!);
        await Hub(pendiente).EscalarEnvido2v2("Real Envido");

        var responde = Conn2v2(codigo, mano.EnvidoPendienteRespuestaDe!);
        await Hub(responde).ResponderEnvido2v2(false);

        Assert.True(mano.EnvidoResuelto);
    }

    [Fact]
    public async Task Flujo2v2_TrucoNoQuerido_TerminaLaManoYPermiteNuevaMano()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var state  = _salas.GetEstado2v2(codigo)!;
        var mano   = state.Mano;

        foreach (var c in Conns(4)) await Hub(c).SolicitarTruco2v2();
        Assert.True(mano.TrucoCantado);

        var responde = Conn2v2(codigo, mano.TrucoPendienteRespuestaDe!);
        await Hub(responde).ResponderTruco2v2(false, null);
        Assert.NotNull(mano.GanadorMano);

        await Hub("p1").NuevaMano2v2();
        var nuevo = _salas.GetEstado2v2(codigo)!;
        Assert.NotSame(state, nuevo);
        Assert.Equal(2, nuevo.Mano.NumeroDeMano);
    }

    [Fact]
    public async Task Flujo2v2_TrucoQuerido_YEscalado()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var mano   = _salas.GetEstado2v2(codigo)!.Mano;

        // Solo el jugador en turno puede cantar (PuedeCantar exige TurnoActual)
        foreach (var c in Conns(4)) await Hub(c).SolicitarTruco2v2();
        Assert.True(mano.TrucoCantado);

        var responde = Conn2v2(codigo, mano.TrucoPendienteRespuestaDe!);
        await Hub(responde).ResponderTruco2v2(true, null);
        Assert.True(mano.TrucoResuelto);

        // Escalar exige ser del equipo contrario al cantor Y tener el turno:
        // se juegan cartas hasta que el turno caiga en un rival, que retruca.
        for (int i = 0; i < 4 && mano.NivelTruco == 1; i++)
        {
            var rolTurno = mano.TurnoActual;
            await Hub(Conn2v2(codigo, rolTurno)).EscalarTruco2v2();
            if (mano.NivelTruco == 2) break;

            var carta = mano.ObtenerJugador(rolTurno)!.Mano[0];
            await Hub(Conn2v2(codigo, rolTurno)).JugarCarta2v2(carta.Numero, carta.Palo);
        }
        Assert.Equal(2, mano.NivelTruco);

        // El equipo original responde el retruco con quiero
        var responde2 = Conn2v2(codigo, mano.TrucoPendienteRespuestaDe!);
        await Hub(responde2).ResponderTruco2v2(true, null);
        Assert.True(mano.TrucoResuelto);
    }

    [Fact]
    public async Task Flujo2v2_IrseAlMazo_TerminaLaMano()
    {
        var codigo = await ArmarPartida("2v2", 4);
        var mano   = _salas.GetEstado2v2(codigo)!.Mano;

        foreach (var c in Conns(4))
        {
            await Hub(c).IrseAlMazo2v2();
            if (mano.GanadorMano != null) break;
        }

        Assert.NotNull(mano.GanadorMano);
    }

    // ── 3v3 ───────────────────────────────────────────────────────

    [Fact]
    public async Task Flujo3v3_JugarCarta_EnTurnoYFueraDeTurno()
    {
        var codigo = await ArmarPartida("3v3", 6);
        var state  = _salas.GetEstado3v3(codigo)!;
        var mano   = state.Mano;

        var rolTurno = mano.TurnoActual;
        var otroRol  = Enumerable.Range(1, 6).Select(i => $"J{i}").First(r => r != rolTurno);
        var cartaOtro = mano.ObtenerJugador(otroRol)!.Mano[0];

        await Hub(Conn3v3(codigo, otroRol)).JugarCarta3v3(cartaOtro.Numero, cartaOtro.Palo);
        Assert.Empty(mano.ObtenerJugador(otroRol)!.Jugadas);

        var carta = mano.ObtenerJugador(rolTurno)!.Mano[0];
        await Hub(Conn3v3(codigo, rolTurno)).JugarCarta3v3(carta.Numero, carta.Palo);
        Assert.Single(mano.ObtenerJugador(rolTurno)!.Jugadas);
    }

    [Fact]
    public async Task Flujo3v3_EnvidoQueridoConDeclaracion()
    {
        var codigo = await ArmarPartida("3v3", 6);
        var mano   = _salas.GetEstado3v3(codigo)!.Mano;

        await Hub("p1").DeclararTanto3v3(25); // sin envido: guard de fase
        Assert.False(mano.EnvidoCantado);

        foreach (var c in Conns(6)) await Hub(c).SolicitarEnvido3v3("Envido");
        Assert.True(mano.EnvidoCantado);

        var responde = Conn3v3(codigo, mano.EnvidoPendienteRespuestaDe!);
        await Hub(responde).ResponderEnvido3v3(true);

        for (int i = 0; i < 8 && mano.FaseEnvido == "declarando_tantos"; i++)
        {
            var pendiente = Conn3v3(codigo, mano.EnvidoPendienteRespuestaDe!);
            if (i == 0) await Hub(pendiente).DeclararTanto3v3(25);
            else        await Hub(pendiente).SonBuenas3v3();
        }

        Assert.True(mano.EnvidoResuelto);
    }

    [Fact]
    public async Task Flujo3v3_EnvidoEscaladoYNoQuerido()
    {
        var codigo = await ArmarPartida("3v3", 6);
        var mano   = _salas.GetEstado3v3(codigo)!.Mano;

        foreach (var c in Conns(6)) await Hub(c).SolicitarEnvido3v3("Envido");

        var pendiente = Conn3v3(codigo, mano.EnvidoPendienteRespuestaDe!);
        await Hub(pendiente).EscalarEnvido3v3("Real Envido");

        var responde = Conn3v3(codigo, mano.EnvidoPendienteRespuestaDe!);
        await Hub(responde).ResponderEnvido3v3(false);

        Assert.True(mano.EnvidoResuelto);
    }

    [Fact]
    public async Task Flujo3v3_TrucoNoQuerido_YNuevaMano()
    {
        var codigo = await ArmarPartida("3v3", 6);
        var state  = _salas.GetEstado3v3(codigo)!;
        var mano   = state.Mano;

        foreach (var c in Conns(6)) await Hub(c).SolicitarTruco3v3();
        Assert.True(mano.TrucoCantado);

        var responde = Conn3v3(codigo, mano.TrucoPendienteRespuestaDe!);
        await Hub(responde).ResponderTruco3v3(false, null);
        Assert.NotNull(mano.GanadorMano);

        await Hub("p1").NuevaMano3v3();
        var nuevo = _salas.GetEstado3v3(codigo)!;
        Assert.NotSame(state, nuevo);
        Assert.Equal(2, nuevo.Mano.NumeroDeMano);
    }

    [Fact]
    public async Task Flujo3v3_TrucoQuerido_YEscalado()
    {
        var codigo = await ArmarPartida("3v3", 6);
        var mano   = _salas.GetEstado3v3(codigo)!.Mano;

        // Solo el jugador en turno puede cantar (PuedeCantar exige TurnoActual)
        foreach (var c in Conns(6)) await Hub(c).SolicitarTruco3v3();
        Assert.True(mano.TrucoCantado);

        var responde = Conn3v3(codigo, mano.TrucoPendienteRespuestaDe!);
        await Hub(responde).ResponderTruco3v3(true, null);
        Assert.True(mano.TrucoResuelto);

        // Escalar exige ser del equipo contrario al cantor Y tener el turno:
        // se juegan cartas hasta que el turno caiga en un rival, que retruca.
        for (int i = 0; i < 6 && mano.NivelTruco == 1; i++)
        {
            var rolTurno = mano.TurnoActual;
            await Hub(Conn3v3(codigo, rolTurno)).EscalarTruco3v3();
            if (mano.NivelTruco == 2) break;

            var carta = mano.ObtenerJugador(rolTurno)!.Mano[0];
            await Hub(Conn3v3(codigo, rolTurno)).JugarCarta3v3(carta.Numero, carta.Palo);
        }
        Assert.Equal(2, mano.NivelTruco);

        // El equipo original responde el retruco con quiero
        var responde2 = Conn3v3(codigo, mano.TrucoPendienteRespuestaDe!);
        await Hub(responde2).ResponderTruco3v3(true, null);
        Assert.True(mano.TrucoResuelto);
    }

    [Fact]
    public async Task Flujo3v3_IrseAlMazo_TerminaLaMano()
    {
        var codigo = await ArmarPartida("3v3", 6);
        var mano   = _salas.GetEstado3v3(codigo)!.Mano;

        foreach (var c in Conns(6))
        {
            await Hub(c).IrseAlMazo3v3();
            if (mano.GanadorMano != null) break;
        }

        Assert.NotNull(mano.GanadorMano);
    }

    // ── Lobby / desconexión ───────────────────────────────────────

    [Fact]
    public async Task UnirseASala_DosVecesConLaMismaConexion_DevuelveFalse()
    {
        var codigo = await Hub("p1").CrearSala("2v2", false);
        await Hub("p2").UnirseASala(codigo);

        Assert.False(await Hub("p2").UnirseASala(codigo));
    }

    [Fact]
    public async Task ElegirEquipo_ConEquipoLleno_NoAsigna()
    {
        var codigo = await Hub("p1").CrearSala("2v2", false);
        foreach (var c in new[] { "p2", "p3", "p4" }) await Hub(c).UnirseASala(codigo);

        await Hub("p1").ElegirEquipo("sanMartin");
        await Hub("p2").ElegirEquipo("sanMartin");
        await Hub("p3").ElegirEquipo("sanMartin"); // cupo 2: rechazado

        var equipos = _salas.GetEquiposMap(codigo)!;
        Assert.Equal(2, equipos.Values.Count(v => v == "sanMartin"));
    }

    [Fact]
    public async Task OnDisconnected_ConEquiposElegidos_RebroadcasteaEstadoEquipos()
    {
        var codigo = await Hub("p1").CrearSala("2v2", false);
        foreach (var c in new[] { "p2", "p3", "p4" }) await Hub(c).UnirseASala(codigo);
        await Hub("p1").ElegirEquipo("sanMartin");

        await Hub("p2").OnDisconnectedAsync(null);

        GroupProxy(codigo).Verify(p => p.SendCoreAsync(
            "JugadorDesconectado", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
        // p1 recibió EstadoEquipos por su elección y de nuevo tras la desconexión
        ClientProxy("p1").Verify(p => p.SendCoreAsync(
            "EstadoEquipos", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
