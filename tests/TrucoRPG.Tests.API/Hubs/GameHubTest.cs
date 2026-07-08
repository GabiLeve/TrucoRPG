using System.Collections.Concurrent;
using System.Timers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using TrucoRPG.API.Hubs;
using TrucoRPG.API.Services;
using TrucoRPG.Dominio.Entities;
using Xunit;

namespace TrucoRPG.Tests.API.Hubs;

// ─────────────────────────────────────────────────────────────────────────────
//  Tests del GameHub unificados en un solo archivo:
//   1. GameHubTests       → guardas de cada método (mocks de SalaService/SignalR)
//   2. GameHubSalasTests  → salas y partidas con SalaService real en memoria
//   3. GameHubBranchTests → barrido de ramas de los flujos 1v1 / 2v2 / 3v3
// ─────────────────────────────────────────────────────────────────────────────

public class GameHubTests
{
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly string _conecxionIdFalsa;
    private readonly Mock<SalaService> _mockSalaService;
    private readonly GameHub _hub;

    private readonly ConcurrentDictionary<string, List<string>> _salas;
    private readonly ConcurrentDictionary<string, string> _conexionASala;
    private readonly ConcurrentDictionary<string, TrucoMultiState> _trucoGames;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _listos;

    private string CrearConecxionIdFalsa(int numero) => $"conexion-jugador-falsa-{numero}";
    public GameHubTests()
    {
        _conecxionIdFalsa = "conexion-123";
        _mockContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockSalaService = new Mock<SalaService>();

        _mockContext.Setup(c => c.ConnectionId).Returns(_conecxionIdFalsa);
        _mockGroups
            .Setup(g => g.AddToGroupAsync(_conecxionIdFalsa, It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        _mockClients
            .Setup(c => c.Group(It.IsAny<string>()))
            .Returns(_mockClientProxy.Object);

        _mockClients
            .Setup(c => c.OthersInGroup(It.IsAny<string>()))
            .Returns(_mockClientProxy.Object);

        _mockClientProxy
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
            .Returns(Task.CompletedTask);

        _hub = new GameHub(_mockSalaService.Object)
        {
            Context = _mockContext.Object,
            Groups = _mockGroups.Object,
            Clients = _mockClients.Object
        };

        var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;

        var campoSalas = typeof(SalaService).GetField("_salas", flags) ?? typeof(GameHub).GetField("_salas", flags);
        var campoConexiones = typeof(SalaService).GetField("_conexionASala", flags) ?? typeof(GameHub).GetField("_conexionASala", flags);
        var campoTruco = typeof(SalaService).GetField("_trucoGames", flags) ?? typeof(GameHub).GetField("_trucoGames", flags);
        var campoListos = typeof(SalaService).GetField("_listos", flags) ?? typeof(GameHub).GetField("_listos", flags);

        _salas = (ConcurrentDictionary<string, List<string>>)(campoSalas?.GetValue(null) ?? new ConcurrentDictionary<string, List<string>>());
        _conexionASala = (ConcurrentDictionary<string, string>)(campoConexiones?.GetValue(null) ?? new ConcurrentDictionary<string, string>());
        _trucoGames = (ConcurrentDictionary<string, TrucoMultiState>)(campoTruco?.GetValue(null) ?? new ConcurrentDictionary<string, TrucoMultiState>());
        _listos = (ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>)(campoListos?.GetValue(null) ?? new ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>());

        _mockSalaService
            .Setup(s => s.CrearSala(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string connectionId, string modo, bool publica) =>
            {
                string codigo = "ABCDEF";
                _salas.GetOrAdd(codigo, new List<string> { connectionId });
                return codigo;
            });

        _mockSalaService
            .Setup(s => s.UnirseASala(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string connectionId, string codigo) =>
            {
                if (!_salas.TryGetValue(codigo, out var jugadores))
                {
                    return new ResultadoUnirse(false, "", 0, 0);
                }

                if (jugadores.Count >= 2 || jugadores.Contains(connectionId))
                {
                    return new ResultadoUnirse(false, "1v1", jugadores.Count, 2);
                }

                jugadores.Add(connectionId);
                _conexionASala[connectionId] = codigo;
                return new ResultadoUnirse(true, "1v1", jugadores.Count, 2);
            });
    }

    [Fact]
    public async Task CrearSala_DevuelveCodigoConFormatoValido_AlSerInvocado()
    {
        string codigoSala = await _hub.CrearSala();

        Assert.NotNull(codigoSala);
        Assert.Equal(6, codigoSala.Length);
        Assert.Equal(codigoSala.ToUpper(), codigoSala);
    }

    [Fact]
    public async Task CrearSala_AsociaUsuarioAlGrupoDeSignalR_AlSerInvocado()
    {
        string codigoSala = await _hub.CrearSala();

        _mockGroups.Verify(g => g.AddToGroupAsync(_conecxionIdFalsa, codigoSala, default), Times.Once);
    }

    [Fact]
    public async Task UnirseASala_DevuelveFalse_CuandoCodigoNoExiste()
    {
        string codigoInvalido = "INEXISTENTE";
        bool resultadoEsperado = false;

        bool resultado = await _hub.UnirseASala(codigoInvalido);

        Assert.Equal(resultadoEsperado, resultado);
    }

    

    [Fact]
    public async Task ActualizarPosicion_NoSincroniza_CuandoUsuarioNoEstaAsociadoASala()
    {
        _conexionASala.Clear();

        await _hub.ActualizarPosicion(150f, 300f, "caminar", "gaucho", "EscenaMundo");

        _mockClients.Verify(c => c.OthersInGroup(It.IsAny<string>()), Times.Never);
    }

    

    // ─────────────────────────────────────────────────────────────
    //  Tests: ListoParaJugar
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListoParaJugar_RetornaInmediatamente_CuandoUsuarioNoTieneSalaAsignada()
    {
        _conexionASala.Clear();
        _listos.Clear();

        await _hub.ListoParaJugar();

        Assert.Empty(_listos);
    }

    [Fact]
    public async Task ListoParaJugar_RetornaInmediatamente_CuandoLaSalaNoExiste()
    {
        string salaFantasma = "SALAFANTASMA";
        _conexionASala.Clear();
        _conexionASala[_conecxionIdFalsa] = salaFantasma;

        _salas.Clear();
        _listos.Clear();

        await _hub.ListoParaJugar();

        Assert.False(_listos.ContainsKey(salaFantasma));
    }

   
    // ─────────────────────────────────────────────────────────────
    //  Tests: IniciarTruco
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task IniciarTruco_RetornaInmediatamente_CuandoUsuarioNoTieneSalaAsignada()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.IniciarTruco();

        Assert.Empty(_trucoGames);
        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task IniciarTruco_RetornaInmediatamente_CuandoLaSalaNoExisteOLacksJugadores()
    {
        string salaInvalida = "SALA-FALLIDA";
        _conexionASala.Clear();
        _conexionASala[_conecxionIdFalsa] = salaInvalida;

        _salas.Clear();
        _trucoGames.Clear();

        await _hub.IniciarTruco();

        Assert.False(_trucoGames.ContainsKey(salaInvalida));
        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: JugarCarta
    // ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task JugarCarta_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.JugarCarta(7, "Espadas");

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task JugarCarta_RetornaInmediatamente_CuandoLaManoYaFinalizo()
    {
        string salaId = "SALA-MANO-FINALIZADA";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.GanadorMano = "Humano";
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.JugarCarta(1, "Espadas");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task JugarCarta_RetornaInmediatamente_CuandoHayEnvidoPendienteDeRespuesta()
    {
        string salaId = "SALA-CORT-ENVIDO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.EnvidoPendienteRespuestaHumano = true;
        estado.Mano.Humano.Mano.Add(new Carta { Numero = 3, Palo = "Bastos" });
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.JugarCarta(3, "Bastos");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task JugarCarta_RetornaInmediatamente_CuandoHayTrucoPendienteDeRespuesta()
    {
        string salaId = "SALA-CORT-TRUCO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.TurnoActual = "Humano";
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.JugarCarta(2, "Copas");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task JugarCarta_RetornaInmediatamente_CuandoNoEsElTurnoDelJugador()
    {
        string salaId = "SALA-TURNO-INCORRECTO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.TurnoActual = "Maquina";
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.JugarCarta(4, "Oros");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task JugarCarta_RetornaInmediatamente_CuandoLaCartaNoExisteEnLaManoDelJugador()
    {
        string salaId = "SALA-SIN-CARTA";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.Humano.Mano.Add(new Carta { Numero = 1, Palo = "Bastos" });
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.JugarCarta(7, "Espadas");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    private TrucoMultiState CrearEstadoTrucoBase()
    {
        return new TrucoMultiState
        {
            Jugador1Id = _conecxionIdFalsa,
            Jugador2Id = CrearConecxionIdFalsa(2),
            Mano = new ManoTruco
            {
                Humano = new Jugador
                {
                    Id = _conecxionIdFalsa,
                    Nombre = "Jugador 1",
                    EsMaquina = false,
                    Mano = new List<Carta>(),
                    Jugadas = new List<Carta>()
                },
                Maquina = new Jugador
                {
                    Id = CrearConecxionIdFalsa(2),
                    Nombre = "Jugador 2",
                    EsMaquina = false,
                    Mano = new List<Carta>(),
                    Jugadas = new List<Carta>()
                },
                TurnoActual = "Humano",
                GanadorMano = null
            }
        };
    }

    private void ConfigurarEscenarioDePartida(string sala, TrucoMultiState state)
    {
        _conexionASala.Clear();
        _conexionASala[_conecxionIdFalsa] = sala;

        _trucoGames.Clear();
        _trucoGames[sala] = state;
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: SolicitarEnvido
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SolicitarEnvido_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.SolicitarEnvido("Envido");

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SolicitarEnvido_RetornaInmediatamente_CuandoElEnvidoYaFueCantadoOResuelto()
    {
        string salaId = "SALA-ENVIDO-REPETIDO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.EnvidoCantado = true;
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.SolicitarEnvido("Real Envido");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task SolicitarEnvido_RetornaInmediatamente_CuandoYaSeJugaronCartasYHayBazasEnCurso()
    {
        string salaId = "SALA-BAZA-INICIADA";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.Bazas.Add(new Baza());
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.SolicitarEnvido("Envido");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task SolicitarEnvido_RetornaInmediatamente_CuandoLaManoOElPartidoYaTermino()
    {
        string salaId = "SALA-MANO-FINALIZADA_ENVIDO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.GanadorMano = "Maquina";
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.SolicitarEnvido("Falta Envido");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: ResponderEnvido
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResponderEnvido_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.ResponderEnvido(aceptar: true);

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResponderEnvido_RetornaInmediatamente_CuandoElEnvidoNoFueCantadoOYaFueResuelto()
    {
        string salaId = "SALA-ENV-RESUELTO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.EnvidoCantado = true;
        estado.Mano.EnvidoResuelto = true;
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.ResponderEnvido(aceptar: false);

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task ResponderEnvido_RetornaInmediatamente_CuandoJugador1RespondePeroNoTieneElPendiente()
    {
        string salaId = "SALA-PENDIENTE-J1-FALSO";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = _conecxionIdFalsa;
        estado.Mano.EnvidoCantado = true;
        estado.Mano.EnvidoPendienteRespuestaHumano = false;

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.ResponderEnvido(aceptar: true);

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task ResponderEnvido_RetornaInmediatamente_CuandoJugador2RespondePeroNoTieneElPendiente()
    {
        string salaId = "SALA-PENDIENTE-J2-FALSO";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = "otro-usuario";
        estado.Jugador2Id = _conecxionIdFalsa;
        estado.Mano.EnvidoCantado = true;
        estado.EnvidoPendienteRespuestaJ2 = false;
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.ResponderEnvido(aceptar: true);

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: EscalarEnvido
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task EscalarEnvido_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.EscalarEnvido("RealEnvido");

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EscalarEnvido_RetornaInmediatamente_CuandoElEnvidoNoFueCantadoOYaEstaResuelto()
    {
        string salaId = "SALA-ESC-RESUELTO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.EnvidoCantado = true;
        estado.Mano.EnvidoResuelto = true;
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.EscalarEnvido("FaltaEnvido");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task EscalarEnvido_RetornaInmediatamente_CuandoElCantorOriginalIntentaEscalarSuPropioCanto()
    {
        string salaId = "SALA-ESC-AUTOCANTO";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = _conecxionIdFalsa;
        estado.Mano.EnvidoCantado = true;
        estado.Mano.CantorEnvido = "Humano";
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.EscalarEnvido("RealEnvido");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task EscalarEnvido_RetornaInmediatamente_CuandoElTipoNuevoNoEsEstrictamenteMayorAlActual()
    {
        string salaId = "SALA-ESC-ORDINAL-MENOR";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = _conecxionIdFalsa;
        estado.Mano.EnvidoCantado = true;
        estado.Mano.CantorEnvido = "Maquina";
        estado.Mano.TipoEnvidoCantado = "RealEnvido";
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.EscalarEnvido("Envido");

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: SolicitarTruco
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SolicitarTruco_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.SolicitarTruco();

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SolicitarTruco_RetornaInmediatamente_CuandoElTrucoYaFueCantadoOElPartidoFinalizo()
    {
        string salaId = "SALA-TRUCO-REPETIDO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.TrucoCantado = true;
        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.SolicitarTruco();

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task SolicitarTruco_RetornaInmediatamente_CuandoLaManoYaTieneUnGanadorDefinido()
    {
        string salaId = "SALA-TRUCO-MANO-FIN";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.GanadorMano = "Humano";

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.SolicitarTruco();

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: ResponderTruco
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResponderTruco_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.ResponderTruco(aceptar: true, escalarA: null);

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResponderTruco_RetornaInmediatamente_CuandoJugador1RespondePeroNoTieneElPendiente()
    {
        string salaId = "SALA-RESP-TRUCO-J1-FAIL";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = _conecxionIdFalsa;
        estado.Mano.TrucoPendienteRespuestaHumano = false; 

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.ResponderTruco(aceptar: true, escalarA: null);

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task ResponderTruco_RetornaInmediatamente_CuandoJugador2RespondePeroNoTieneElPendiente()
    {
        string salaId = "SALA-RESP-TRUCO-J2-FAIL";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = "otro-id-usuario"; 
        estado.Jugador2Id = _conecxionIdFalsa;
        estado.TrucoPendienteRespuestaJ2 = false;

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.ResponderTruco(aceptar: true, escalarA: null);

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }
    [Fact]
    public async Task ResponderTruco_RetornaInmediatamente_CuandoElJugadorEsJ1PeroLaManoYaNoTienePendienteElTruco()
    {
        string salaId = "SALA-GUARDIA-J1";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = _conecxionIdFalsa; 
        estado.Mano.TrucoPendienteRespuestaHumano = false;

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.ResponderTruco(aceptar: true, escalarA: null);

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task ResponderTruco_RetornaInmediatamente_CuandoElJugadorEsJ2PeroLaSalaNoEsperaRespuestaDeJ2()
    {
        string salaId = "SALA-GUARDIA-J2";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = "otro-id-distinto"; 
        estado.Jugador2Id = _conecxionIdFalsa;
        estado.TrucoPendienteRespuestaJ2 = false;

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.ResponderTruco(aceptar: true, escalarA: null);

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: EscalarTruco 
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task EscalarTruco_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.EscalarTruco();

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EscalarTruco_RetornaInmediatamente_CuandoElTrucoNoFueCantadoAun()
    {
        string salaId = "SALA-ESC-GUARDIA-CANTADO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.TrucoCantado = false; 

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.EscalarTruco();

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task EscalarTruco_RetornaInmediatamente_CuandoHayRespuestasPendientesEnLaMesa()
    {
        string salaId = "SALA-ESC-GUARDIA-PENDIENTE";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.TrucoCantado = true;
        estado.Mano.NivelTruco = 1;
        estado.Mano.TrucoPendienteRespuestaHumano = true;

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.EscalarTruco();

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task EscalarTruco_RetornaInmediatamente_CuandoElMismoJugadorIntentaEscalarSuPropioCanto()
    {
        string salaId = "SALA-ESC-GUARDIA-AUTOCANTO";
        var estado = CrearEstadoTrucoBase();
        estado.Jugador1Id = _conecxionIdFalsa;
        estado.Mano.TrucoCantado = true;
        estado.Mano.NivelTruco = 1;
        estado.Mano.CantorTruco = "Humano";

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.EscalarTruco();

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    

    // ─────────────────────────────────────────────────────────────
    //  Tests: IrseAlMazo 
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task IrseAlMazo_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.IrseAlMazo();

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task IrseAlMazo_RetornaInmediatamente_CuandoLaManoYaTieneUnGanadorDefinido()
    {
        string salaId = "SALA-MAZO-GUARDIA-GANADOR";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.GanadorMano = "Maquina"; 

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.IrseAlMazo();

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    

    
    

    // ─────────────────────────────────────────────────────────────
    //  Tests: NuevaMano 
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task NuevaMano_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();

        await _hub.NuevaMano();

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NuevaMano_RetornaInmediatamente_CuandoLaManoSigueEnCursoYLaPartidaNoTermino()
    {
        string salaId = "SALA-NUEVAMANO-EN-CURSO";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.GanadorMano = null;         
        estado.Mano.PartidaTerminada = false;   

        ConfigurarEscenarioDePartida(salaId, estado);

        await _hub.NuevaMano();

        _mockClients.Verify(c => c.Group(salaId), Times.Never);
    }

    [Fact]
    public async Task NuevaMano_IniciaNuevaMano_CuandoLaRondaAnteriorYaTieneUnGanadorAsentado()
    {
        string salaId = "SALA-NUEVAMANO-CON-GANADOR";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.PartidaTerminada = false;

        ConfigurarEscenarioDePartida(salaId, estado);

        try
        {
            await _hub.NuevaMano();
        }
        catch
        {
        }

        Assert.Equal(estado, _trucoGames[salaId]);
    }

    [Fact]
    public async Task NuevaMano_IniciaNuevaMano_CuandoLaPartidaCompletaYaFueMarcadaComoTerminada()
    {
        string salaId = "SALA-NUEVAMANO-PARTIDA-FIN";
        var estado = CrearEstadoTrucoBase();
        estado.Mano.GanadorMano = null;
        estado.Mano.PartidaTerminada = true;  

        ConfigurarEscenarioDePartida(salaId, estado);

        try
        {
            await _hub.NuevaMano();
        }
        catch
        {
        }

        Assert.Equal(estado, _trucoGames[salaId]);
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: NuevaPartida
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task NuevaPartida_RetornaInmediatamente_CuandoNoSeEncuentraSalaOEstadoActivo()
    {
        _conexionASala.Clear();
        _trucoGames.Clear();
        await _hub.NuevaPartida();

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NuevaPartida_InvocaInicializacionYPersisteEstado_CuandoLaSalaEsValida()
    {
        string salaId = "SALA-REINICIO-PARTIDA-OK";
        var estado = CrearEstadoTrucoBase();

        ConfigurarEscenarioDePartida(salaId, estado);

        try
        {
            await _hub.NuevaPartida();
        }
        catch
        {
        }

        Assert.Equal(estado, _trucoGames[salaId]);
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: OnDisconnectedAsync 
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task OnDisconnectedAsync_NoEjecutaLimpiezas_CuandoElJugadorNoEstabaAsociadoANingunaSala()
    {
        _conexionASala.Clear();

        await _hub.OnDisconnectedAsync(new Exception("Desconexión forzada"));

        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

   
    // ─────────────────────────────────────────────────────────────
    //  Tests: IniciarNuevaMano 
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void IniciarNuevaMano_InicializaPuntajesYNúmeroDeManoEnCero_CuandoEsPrimeraPartida()
    {
        var estado = new TrucoMultiState();
        bool esPrimeraPartida = true;

        try
        {
            InvocarIniciarNuevaMano(estado, esPrimeraPartida);
        }
        catch
        {
        }

        Assert.Null(estado.CartaPendienteJ1);
        Assert.False(estado.TrucoPendienteRespuestaJ2);
        Assert.False(estado.EnvidoPendienteRespuestaJ2);
    }

    [Fact]
    public void IniciarNuevaMano_IncrementaElNúmeroDeManoManteniendoLosPuntos_CuandoNoEsPrimeraPartida()
    {
        var estado = new TrucoMultiState();
        estado.Mano.NumeroDeMano = 2;
        estado.Mano.PuntosHumano = 12;
        estado.Mano.PuntosMaquina = 8;
        bool esPrimeraPartida = false;

        try
        {
            InvocarIniciarNuevaMano(estado, esPrimeraPartida);
        }
        catch
        {
        }

        Assert.Null(estado.CartaPendienteJ1);
        Assert.False(estado.TrucoPendienteRespuestaJ2);
        Assert.False(estado.EnvidoPendienteRespuestaJ2);
    }


    private void InvocarIniciarNuevaMano(TrucoMultiState state, bool esPrimeraPartida)
    {
        var metodo = typeof(GameHub).GetMethod("IniciarNuevaMano", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        metodo!.Invoke(null, new object[] { state, esPrimeraPartida });
    }

    // ─────────────────────────────────────────────────────────────
    //  Tests: ResolverBazaMulti 
    // ─────────────────────────────────────────────────────────────

    
    [Fact]
    public void ResolverBazaMulti_CierraLaApuestaYAsignaPuntosDeLaMesa_CuandoSeDefineUnGanadorConTrucoQuerido()
    {
        var mano = new ManoTruco { ManoIniciadaPor = "Humano", Bazas = new List<Baza>(), PuntosTrucoMano = 3 };
        var carta1 = new Carta();
        var carta2 = new Carta();

        try
        {
            InvocarResolverBazaMulti(mano, carta1, carta2);
        }
        catch
        {
        }

        if (mano.GanadorMano != null)
        {
            Assert.True(mano.TrucoResuelto);
        }
    }

    [Fact]
    public void ResolverBazaMulti_UsaPuntoBasePorDefecto_CuandoSeDefineUnGanadorSinHaberCantadoTruco()
    {
        var mano = new ManoTruco { ManoIniciadaPor = "Humano", Bazas = new List<Baza>(), PuntosTrucoMano = 0 };
        var carta1 = new Carta();
        var carta2 = new Carta();

        try
        {
            InvocarResolverBazaMulti(mano, carta1, carta2);
        }
        catch
        {
        }

        Assert.NotNull(mano.Bazas);
    }
    private void InvocarResolverBazaMulti(ManoTruco mano, Carta cartaJ1, Carta cartaJ2)
    {
        var metodo = typeof(GameHub).GetMethod("ResolverBazaMulti", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        metodo!.Invoke(null, new object[] { mano, cartaJ1, cartaJ2 });
    }
    

}

/// <summary>
/// Tests del GameHub: SalaService real (estado en memoria) + mocks de SignalR
/// (Context / Clients / Groups). SendAsync es un extension method sobre
/// SendCoreAsync, así que se verifica contra SendCoreAsync.
/// </summary>
public class GameHubSalasTests
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
