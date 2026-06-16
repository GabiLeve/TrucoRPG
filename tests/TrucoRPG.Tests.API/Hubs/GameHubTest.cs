using System.Collections.Concurrent;
using System.Timers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using TrucoRPG.API.Hubs;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Tests.API.Hubs;

public class GameHubTests
{
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly string _conecxionIdFalsa;
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

        _hub = new GameHub
        {
            Context = _mockContext.Object,
            Groups = _mockGroups.Object,
            Clients = _mockClients.Object
        };

        var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;

        _salas = (ConcurrentDictionary<string, List<string>>)typeof(GameHub).GetField("_salas", flags)!.GetValue(null)!;
        _conexionASala = (ConcurrentDictionary<string, string>)typeof(GameHub).GetField("_conexionASala", flags)!.GetValue(null)!;
        _trucoGames = (ConcurrentDictionary<string, TrucoMultiState>)typeof(GameHub).GetField("_trucoGames", flags)!.GetValue(null)!;
        _listos = (ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>)typeof(GameHub).GetField("_listos", flags)!.GetValue(null)!;
    
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
    public async Task UnirseASala_DevuelveFalse_CuandoSalaEstaLlena()
    {
        string codigoSala = "LLENA1";
        bool resultadoEsperado = false;
        var jugadoresExistentes = new List<string> { CrearConecxionIdFalsa(1), CrearConecxionIdFalsa(2) };
        var campoSala = typeof(GameHub).GetField("_salas", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var salas = (System.Collections.Concurrent.ConcurrentDictionary<string, List<string>>)campoSala.GetValue(null);
        salas.Clear();
        salas[codigoSala] = jugadoresExistentes;

        bool resultado = await _hub.UnirseASala(codigoSala);

        Assert.Equal(resultadoEsperado, resultado);
    }

    [Fact]
    public async Task UnirseASala_DevuelveTrue_CuandoSalaTieneCupoDisponible()
    {
        string codigoSala = "DISPO1";
        var jugadoresExistentes = new List<string> { CrearConecxionIdFalsa(1) };

        var campoSala = typeof(GameHub).GetField("_salas", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var salas = (System.Collections.Concurrent.ConcurrentDictionary<string, List<string>>)campoSala.GetValue(null);
        salas.Clear();
        salas[codigoSala] = jugadoresExistentes;

        bool resultadoEsperado = true;

        bool resultado = await _hub.UnirseASala(codigoSala);

        Assert.Equal(resultadoEsperado, resultado);
    }


    [Fact]
    public async Task UnirseASala_AsociaUsuarioAlGrupoDeSignalR_CuandoUnionEsExitosa()
    {
        string codigoSala = "SIGR12";
        var jugadoresExistentes = new List<string> { CrearConecxionIdFalsa(1) };

        var campoSala = typeof(GameHub).GetField("_salas", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var salas = (System.Collections.Concurrent.ConcurrentDictionary<string, List<string>>)campoSala.GetValue(null);
        salas.Clear();
        salas[codigoSala] = jugadoresExistentes;

        await _hub.UnirseASala(codigoSala);

        _mockGroups.Verify(g => g.AddToGroupAsync(_conecxionIdFalsa, codigoSala, default), Times.Once);
    }

    [Fact]
    public async Task UnirseASala_NotificaAClientesDeLaSala_CuandoUnionEsExitosa()
    {
        string codigoSala = "NOTIF1";
        var jugadoresExistentes = new List<string> { CrearConecxionIdFalsa(1) };

        var campoSala = typeof(GameHub).GetField("_salas", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var salas = (System.Collections.Concurrent.ConcurrentDictionary<string, List<string>>)campoSala.GetValue(null);
        salas.Clear();
        salas[codigoSala] = jugadoresExistentes;

        await _hub.UnirseASala(codigoSala);

        _mockClientProxy.Verify(
            p => p.SendCoreAsync("SalaLista", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task ActualizarPosicion_NoSincroniza_CuandoUsuarioNoEstaAsociadoASala()
    {
        _conexionASala.Clear();

        await _hub.ActualizarPosicion(150f, 300f, "caminar", "gaucho", "EscenaMundo");

        _mockClients.Verify(c => c.OthersInGroup(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ActualizarPosicion_SincronizaCoordenadasConGrupo_CuandoUsuarioEstaEnSalaValida()
    {
        string codigoSala = "SALA-MAPA";
        _conexionASala.Clear();
        _conexionASala[_conecxionIdFalsa] = codigoSala;

        float posX = 10f;
        float posY = 20f;
        string anim = "correr";
        string sprite = "china";
        string escena = "Pulperia";

        await _hub.ActualizarPosicion(posX, posY, anim, sprite, escena);

        _mockClientProxy.Verify(
            p => p.SendCoreAsync("PosicionActualizada", new object[] { posX, posY, anim, sprite, escena }, default),
            Times.Once
        );
    }

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

    [Fact]
    public async Task ListoParaJugar_RegistraAlJugadorComoListo_PeroNoInicia_CuandoEsElPrimeroEnAvisar()
    {
        string salaEspera = "SALA-ESPERA";

        _conexionASala.Clear();
        _conexionASala[_conecxionIdFalsa] = salaEspera;

        _salas.Clear();
        _salas[salaEspera] = new List<string> { _conecxionIdFalsa, CrearConecxionIdFalsa(2) };

        _listos.Clear();

        await _hub.ListoParaJugar();

        Assert.True(_listos.TryGetValue(salaEspera, out var readySet));
        Assert.True(readySet[_conecxionIdFalsa]);
        Assert.Single(readySet);
    }

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

}
