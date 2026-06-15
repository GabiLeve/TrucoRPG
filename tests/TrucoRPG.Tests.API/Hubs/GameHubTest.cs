using System.Collections.Concurrent;
using System.Timers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using TrucoRPG.API.Hubs;

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

   
}
