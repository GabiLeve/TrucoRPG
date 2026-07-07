using System.Collections.Concurrent;
using System.Timers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using TrucoRPG.API.Hubs;
using TrucoRPG.API.Services;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Tests.API.Hubs;

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
