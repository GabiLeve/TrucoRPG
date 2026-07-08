using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TrucoRPG.API.Services;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.API.Hubs;

[Authorize]
public class GameHub : Hub
{
    private readonly SalaService _salas;

    public GameHub(SalaService salas) => _salas = salas;

    // ─────────────────────────────────────────────────────────────
    //  SALA — crear / unirse / abandonar / listar
    // ─────────────────────────────────────────────────────────────

    public async Task<string> CrearSala(string modo = "1v1", bool publica = false)
    {
        var codigo = _salas.CrearSala(Context.ConnectionId, modo, publica);
        await Groups.AddToGroupAsync(Context.ConnectionId, codigo);
        return codigo;
    }

    // SalaPublicaInfo vive en TrucoRPG.API.Services — se usa directamente aquí gracias al using
    public Task<List<SalaPublicaInfo>> ListarSalasPublicas(string modo = "1v1") =>
        Task.FromResult(_salas.ListarSalasPublicas(modo));

    public virtual async Task<bool> UnirseASala(string codigo)
    {
        // Normalizar una sola vez: el grupo de SignalR debe ser EXACTAMENTE el mismo
        // string que usó CrearSala (mayúsculas), si no el jugador queda en otro grupo
        // y nunca recibe los broadcasts de la sala.
        codigo = codigo.ToUpperInvariant().Trim();
        var r = _salas.UnirseASala(Context.ConnectionId, codigo);
        if (!r.Ok) return false;

        await Groups.AddToGroupAsync(Context.ConnectionId, codigo);

        if (r.Modo == "1v1")
        {
            if (r.Cantidad == 2)
                await Clients.Group(codigo).SendAsync("SalaLista");
        }
        else
        {
            await Clients.Group(codigo).SendAsync("LobbyActualizado", new
            {
                jugadoresEnSala = r.Cantidad,
                maxJugadores    = r.Max,
            });
            if (r.Cantidad == r.Max)
                await Clients.Group(codigo).SendAsync("SalaCompleta");
        }

        return true;
    }

    public async Task AbandonarSala()
    {
        var r = _salas.AbandonarSala(Context.ConnectionId);
        if (r.Sala == null) return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, r.Sala);

        if (!r.SalaVacia)
            await Clients.Group(r.Sala).SendAsync("JugadorDesconectado");
    }

    // ─────────────────────────────────────────────────────────────
    //  EQUIPOS
    // ─────────────────────────────────────────────────────────────

    public async Task ElegirEquipo(string equipo)
    {
        var r = _salas.ElegirEquipo(Context.ConnectionId, equipo);
        if (r == null) return;
        await BroadcastEstadoEquipos(r.Sala, r.Jugadores, r.EquiposMap, r.Modo);
    }

    // ─────────────────────────────────────────────────────────────
    //  MOVIMIENTO
    // ─────────────────────────────────────────────────────────────

    public async Task ActualizarPosicion(float x, float y, string animacion, string sprite, string escena)
    {
        var sala = _salas.GetSala(Context.ConnectionId);
        if (sala != null)
            await Clients.OthersInGroup(sala).SendAsync("PosicionActualizada", x, y, animacion, sprite, escena);
    }

    // ─────────────────────────────────────────────────────────────
    //  TRUCO — listo para jugar
    // ─────────────────────────────────────────────────────────────

    public async Task ListoParaJugar()
    {
        var r = _salas.MarcarListo(Context.ConnectionId);
        if (r == null) return;

        var sala = r.Sala;

        await Clients.Group(sala).SendAsync("LobbyListos", new
        {
            listos     = r.CantidadListos,
            requeridos = r.Requeridos,
        });

        if (!r.TodosListos) return;

        if (r.Modo == "2v2")
        {
            var state = _salas.GetEstado2v2(sala) ?? _salas.IniciarNuevaMano2v2(sala, esPrimeraPartida: true);
            _salas.SetEstado2v2(sala, state);
            await BroadcastTrucoEstado2v2(sala, state);
            return;
        }

        if (r.Modo == "3v3")
        {
            var state = _salas.GetEstado3v3(sala) ?? _salas.IniciarNuevaMano3v3(sala, esPrimeraPartida: true);
            _salas.SetEstado3v3(sala, state);
            await BroadcastTrucoEstado3v3(sala, state);
            return;
        }

        // 1v1
        if (_salas.GetEstado1v1(sala) is { } existing)
        {
            await BroadcastTrucoEstado(sala, existing);
            return;
        }

        var jugadores = _salas.GetJugadores(sala);
        var state1v1 = new TrucoMultiState
        {
            Jugador1Id = jugadores[0],
            Jugador2Id = jugadores[1],
        };
        TrucoMulti1v1Servicio.IniciarNuevaMano(state1v1, esPrimeraPartida: true);
        _salas.SetEstado1v1(sala, state1v1);
        await BroadcastTrucoEstado(sala, state1v1);
    }

    public async Task IniciarTruco()
    {
        var sala = _salas.GetSala(Context.ConnectionId);
        if (sala == null) return;

        var modo      = _salas.GetModo(sala);
        var jugadores = _salas.GetJugadores(sala);

        if (modo == "2v2" && jugadores.Count >= 4)
        {
            var state = _salas.IniciarNuevaMano2v2(sala, esPrimeraPartida: true);
            _salas.SetEstado2v2(sala, state);
            await BroadcastTrucoEstado2v2(sala, state);
            return;
        }

        if (modo == "3v3" && jugadores.Count >= 6)
        {
            var state = _salas.IniciarNuevaMano3v3(sala, esPrimeraPartida: true);
            _salas.SetEstado3v3(sala, state);
            await BroadcastTrucoEstado3v3(sala, state);
            return;
        }

        if (jugadores.Count < 2) return;
        var state1v1 = new TrucoMultiState { Jugador1Id = jugadores[0], Jugador2Id = jugadores[1] };
        TrucoMulti1v1Servicio.IniciarNuevaMano(state1v1, esPrimeraPartida: true);
        _salas.SetEstado1v1(sala, state1v1);
        await BroadcastTrucoEstado(sala, state1v1);
    }

    // ─────────────────────────────────────────────────────────────
    //  1v1 — Jugar carta / Envido / Truco / Mazo / Nueva mano
    // ─────────────────────────────────────────────────────────────

    public async Task JugarCarta(int numero, string palo)
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.JugarCarta(state, esJ1, numero, palo)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task SolicitarEnvido(string tipo)
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.CantarEnvido(state, esJ1, tipo)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task ResponderEnvido(bool aceptar)
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.ResponderEnvido(state, esJ1, aceptar)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task SonBuenas()
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.SonBuenas(state, esJ1)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task EscalarEnvido(string tipo)
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.EscalarEnvido(state, esJ1, tipo)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task SolicitarTruco()
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.CantarTruco(state, esJ1)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task ResponderTruco(bool aceptar, string? escalarA)
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.ResponderTruco(state, esJ1, aceptar, escalarA)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task EscalarTruco()
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.EscalarTruco(state, esJ1)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task IrseAlMazo()
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.IrseAlMazo(state, esJ1)) return;
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task NuevaMano()
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        if (state!.Mano.GanadorMano == null && !state.Mano.PartidaTerminada) return;
        TrucoMulti1v1Servicio.IniciarNuevaMano(state, esPrimeraPartida: false);
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task NuevaPartida()
    {
        if (!_salas.TryGetEstado1v1(Context.ConnectionId, out var sala, out var state)) return;
        // Solo se puede reiniciar cuando la partida terminó (evita que un jugador
        // borre la partida en curso del rival).
        if (!state!.Mano.PartidaTerminada) return;
        TrucoMulti1v1Servicio.IniciarNuevaMano(state, esPrimeraPartida: true);
        _salas.SetEstado1v1(sala!, state);
        await BroadcastTrucoEstado(sala!, state);
    }

    // ─────────────────────────────────────────────────────────────
    //  2v2 — Jugar carta / Envido / Truco / Mazo / Nueva mano
    // ─────────────────────────────────────────────────────────────

    public async Task JugarCarta2v2(int numero, string palo)
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!JuegoServicio2v2.JugarCartaPorValor(state.Mano, jId, numero, palo)) return;
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task SolicitarEnvido2v2(string tipo)
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!EnvidoServicio2v2.Cantar(state.Mano, jId, tipo, TurnoServicio2v2.ObtenerResponsableCanto)) return;
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task ResponderEnvido2v2(bool aceptar)
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!EnvidoServicio2v2.Responder(state.Mano, jId, aceptar)) return;
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task DeclararTanto2v2(int tanto)
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var mano = state!.Mano;
        if (mano.FaseEnvido != "declarando_tantos") return;
        var jId = state.GetJugadorId(Context.ConnectionId);
        if (mano.EnvidoPendienteRespuestaDe != jId) return;
        EnvidoServicio2v2.ProcesarDeclaracion(mano, jId, tanto, sonBuenas: false);
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task SonBuenas2v2()
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var mano = state!.Mano;
        if (mano.FaseEnvido != "declarando_tantos") return;
        var jId = state.GetJugadorId(Context.ConnectionId);
        if (mano.EnvidoPendienteRespuestaDe != jId) return;
        EnvidoServicio2v2.ProcesarDeclaracion(mano, jId, null, sonBuenas: true);
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task EscalarEnvido2v2(string tipo)
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!EnvidoServicio2v2.Escalar(state.Mano, jId, tipo, TurnoServicio2v2.ObtenerResponsableCanto)) return;
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task SolicitarTruco2v2()
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!TrucoServicio2v2.Cantar(state.Mano, jId, TurnoServicio2v2.ObtenerResponsableCanto)) return;
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task ResponderTruco2v2(bool aceptar, string? escalarA)
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!TrucoServicio2v2.Responder(state.Mano, jId, aceptar, escalarA, TurnoServicio2v2.ObtenerResponsableCanto)) return;
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task IrseAlMazo2v2()
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!TrucoServicio2v2.IrseAlMazo(state.Mano, jId)) return;
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task EscalarTruco2v2()
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!TrucoServicio2v2.Escalar(state.Mano, jId, TurnoServicio2v2.ObtenerResponsableCanto)) return;
        _salas.SetEstado2v2(sala!, state);
        await BroadcastTrucoEstado2v2(sala!, state);
    }

    public async Task NuevaMano2v2()
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out var sala, out var state)) return;
        if (state!.Mano.GanadorMano == null && !state.Mano.PartidaTerminada) return;
        var nuevo = _salas.IniciarNuevaMano2v2(sala!, esPrimeraPartida: false, estadoAnterior: state.Mano);
        _salas.SetEstado2v2(sala!, nuevo);
        await BroadcastTrucoEstado2v2(sala!, nuevo);
    }

    // ─────────────────────────────────────────────────────────────
    //  3v3 — Jugar carta / Envido / Truco / Mazo / Nueva mano
    // ─────────────────────────────────────────────────────────────

    public async Task JugarCarta3v3(int numero, string palo)
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!JuegoServicio3v3.JugarCartaPorValor(state.Mano, jId, numero, palo)) return;
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task SolicitarEnvido3v3(string tipo)
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!EnvidoServicio3v3.Cantar(state.Mano, jId, tipo, TurnoServicio3v3.ObtenerResponsableCanto)) return;
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task ResponderEnvido3v3(bool aceptar)
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!EnvidoServicio3v3.Responder(state.Mano, jId, aceptar)) return;
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task DeclararTanto3v3(int tanto)
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var mano = state!.Mano;
        if (mano.FaseEnvido != "declarando_tantos") return;
        var jId = state.GetJugadorId(Context.ConnectionId);
        if (mano.EnvidoPendienteRespuestaDe != jId) return;
        EnvidoServicio3v3.ProcesarDeclaracion(mano, jId, tanto, sonBuenas: false);
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task SonBuenas3v3()
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var mano = state!.Mano;
        if (mano.FaseEnvido != "declarando_tantos") return;
        var jId = state.GetJugadorId(Context.ConnectionId);
        if (mano.EnvidoPendienteRespuestaDe != jId) return;
        EnvidoServicio3v3.ProcesarDeclaracion(mano, jId, null, sonBuenas: true);
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task EscalarEnvido3v3(string tipo)
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!EnvidoServicio3v3.Escalar(state.Mano, jId, tipo, TurnoServicio3v3.ObtenerResponsableCanto)) return;
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task SolicitarTruco3v3()
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!TrucoServicio3v3.Cantar(state.Mano, jId, TurnoServicio3v3.ObtenerResponsableCanto)) return;
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task ResponderTruco3v3(bool aceptar, string? escalarA)
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!TrucoServicio3v3.Responder(state.Mano, jId, aceptar, escalarA, TurnoServicio3v3.ObtenerResponsableCanto)) return;
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task IrseAlMazo3v3()
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!TrucoServicio3v3.IrseAlMazo(state.Mano, jId)) return;
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task EscalarTruco3v3()
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        var jId = state!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jId)) return;
        if (!TrucoServicio3v3.Escalar(state.Mano, jId, TurnoServicio3v3.ObtenerResponsableCanto)) return;
        _salas.SetEstado3v3(sala!, state);
        await BroadcastTrucoEstado3v3(sala!, state);
    }

    public async Task NuevaMano3v3()
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out var sala, out var state)) return;
        if (state!.Mano.GanadorMano == null && !state.Mano.PartidaTerminada) return;
        var nuevo = _salas.IniciarNuevaMano3v3(sala!, esPrimeraPartida: false, estadoAnterior: state.Mano);
        _salas.SetEstado3v3(sala!, nuevo);
        await BroadcastTrucoEstado3v3(sala!, nuevo);
    }

    // ─────────────────────────────────────────────────────────────
    //  Señas — solo llegan a los compañeros de equipo
    // ─────────────────────────────────────────────────────────────

    public async Task EnviarSenia2v2(string tipo)
    {
        if (!_salas.TryGetEstado2v2(Context.ConnectionId, out _, out var state)) return;
        if (!state!.Posiciones.TryGetValue(Context.ConnectionId, out var miPos)) return;

        // Compañero: 1↔3, 2↔4
        int posCompanero = miPos switch { 1 => 3, 3 => 1, 2 => 4, 4 => 2, _ => 0 };
        var companero = state.Posiciones.FirstOrDefault(x => x.Value == posCompanero).Key;
        if (string.IsNullOrEmpty(companero)) return;

        await Clients.Client(companero).SendAsync("RecibirSenia2v2", tipo);
    }

    public async Task EnviarSenia3v3(string tipo)
    {
        if (!_salas.TryGetEstado3v3(Context.ConnectionId, out _, out var state)) return;
        if (!state!.Posiciones.TryGetValue(Context.ConnectionId, out var miPos)) return;

        var miRol    = $"J{miPos}";
        var miEquipo = state.Mano.ObtenerEquipoDeJugador(miRol);

        // Enviar a los dos compañeros (EquipoA = pos 1/3/5, EquipoB = pos 2/4/6)
        foreach (var kv in state.Posiciones)
        {
            if (kv.Key == Context.ConnectionId) continue;

            var rol = $"J{kv.Value}";
            if (state.Mano.ObtenerEquipoDeJugador(rol) != miEquipo) continue;

            await Clients.Client(kv.Key).SendAsync("RecibirSenia3v3", tipo, miRol);
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Desconexión
    // ─────────────────────────────────────────────────────────────

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var r = _salas.OnDesconectado(Context.ConnectionId);

        if (r.Sala != null)
        {
            if (!r.SalaVacia)
            {
                await Clients.Group(r.Sala).SendAsync("JugadorDesconectado");

                if (r.EquiposMap != null)
                    await BroadcastEstadoEquipos(r.Sala, r.JugadoresRestantes, r.EquiposMap, r.Modo);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ─────────────────────────────────────────────────────────────
    //  Broadcasts (responsabilidad exclusiva del Hub)
    // ─────────────────────────────────────────────────────────────

    private async Task BroadcastEstadoEquipos(
        string sala,
        List<string> jugadores,
        ConcurrentDictionary<string, string> equiposMap,
        string modo)
    {
        int cupo           = ModoJuegoConfig.JugadoresPorEquipo(modo);
        int countSanMartin = equiposMap.Values.Count(v => v == "sanMartin");
        int countBelgrano  = equiposMap.Values.Count(v => v == "belgrano");
        bool equiposListos = countSanMartin == cupo && countBelgrano == cupo;

        var dto = jugadores.Select((cId, i) => new
        {
            posicion = i + 1,
            equipo   = equiposMap.TryGetValue(cId, out var eq) ? eq : (string?)null,
        }).ToList();

        for (int i = 0; i < jugadores.Count; i++)
        {
            await Clients.Client(jugadores[i]).SendAsync("EstadoEquipos", new
            {
                miPosicion     = i + 1,
                jugadores      = dto,
                equiposListos,
                countSanMartin,
                countBelgrano,
            });
        }
    }

    //---------- SEÑAS 2VS2 ---------//

    private string? ObtenerConnectionCompanero(TrucoMultiState2v2 state)
    {
        if (!state.Posiciones.TryGetValue(Context.ConnectionId, out var miPos))
            return null;

        int posCompanero = miPos switch
        {
            1 => 3,
            3 => 1,
            2 => 4,
            4 => 2,
            _ => 0
        };

        // Buscar el connectionId del compañero a partir de su posición
        return state.Posiciones
            .FirstOrDefault(x => x.Value == posCompanero)
            .Key;
    }

    public async Task EnviarSenia2v2(string tipo)
    {
        //encontrar sala
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out var sala))
            return;
        //encontrar estado TrucoMultiState2v2
        if (!_trucoGames2v2.TryGetValue(sala, out var state))
            return;

        //encontrar compañero
        var companeroConnectionId = ObtenerConnectionCompanero(state);

        if (string.IsNullOrEmpty(companeroConnectionId))
            return;

        //Ejecutar
        await Clients.Client(companeroConnectionId)
            .SendAsync("RecibirSenia2v2", tipo);
    }

    public async Task EnviarSenia3v3(string tipo)
    {
        //encontrar sala y estado
        if (!ObtenerSalaYEstado3v3(out _, out var state))
            return;

        //encontrar mi posición y equipo (EquipoA = pos 1/3/5, EquipoB = pos 2/4/6)
        if (!state!.Posiciones.TryGetValue(Context.ConnectionId, out var miPos))
            return;

        var miRol    = $"J{miPos}";
        var miEquipo = state.Mano.ObtenerEquipoDeJugador(miRol);

        //enviar la seña a los dos compañeros del equipo
        foreach (var kv in state.Posiciones)
        {
            if (kv.Key == Context.ConnectionId) continue;

            var rol = $"J{kv.Value}";
            if (state.Mano.ObtenerEquipoDeJugador(rol) != miEquipo) continue;

            await Clients.Client(kv.Key)
                .SendAsync("RecibirSenia3v3", tipo, miRol);
        }
    }p
    private async Task BroadcastTrucoEstado(string sala, TrucoMultiState state)
    {
        var mano    = state.Mano;
        var baseDto = new
        {
            mano.NumeroDeMano, mano.TurnoActual, mano.ManoIniciadaPor,
            mano.GanadorMano, mano.PartidaTerminada, mano.GanadorPartida,
            mano.PuntosHumano, mano.PuntosMaquina,
            mano.EstadoEnvido, mano.EstadoTruco, mano.EnvidoCantado, mano.EnvidoResuelto,
            mano.TipoEnvidoCantado, mano.CantorEnvido, mano.TantoHumano, mano.TantoMaquina,
            mano.TrucoCantado, mano.TrucoResuelto, mano.NivelTruco, mano.PuntosTrucoMano,
            mano.CantorTruco, mano.Bazas, mano.SonBuenasDeclarado,
            CartaPendienteJ1  = state.CartaPendienteJ1,
            CartaPendienteJ2  = mano.CartaMaquinaEnMesa,
            EnvidoPendienteJ1 = mano.EnvidoPendienteRespuestaHumano,
            EnvidoPendienteJ2 = state.EnvidoPendienteRespuestaJ2,
            TrucoPendienteJ1  = mano.TrucoPendienteRespuestaHumano,
            TrucoPendienteJ2  = state.TrucoPendienteRespuestaJ2,
        };

        await Clients.Client(state.Jugador1Id).SendAsync("TrucoEstado", new
        {
            miRol = "J1", misManos = mano.Humano.Mano, misJugadas = mano.Humano.Jugadas,
            cantidadCartasOponente = mano.Maquina.Mano.Count, estado = baseDto,
        });
        await Clients.Client(state.Jugador2Id).SendAsync("TrucoEstado", new
        {
            miRol = "J2", misManos = mano.Maquina.Mano, misJugadas = mano.Maquina.Jugadas,
            cantidadCartasOponente = mano.Humano.Mano.Count, estado = baseDto,
        });
    }

    private async Task BroadcastTrucoEstado2v2(string sala, TrucoMultiState2v2 state)
    {
        var mano    = state.Mano;
        var baseDto = new
        {
            mano.NumeroDeMano, mano.TurnoActual, mano.JugadorMano, mano.EquipoMano,
            mano.GanadorMano, mano.ManoTerminada, mano.PartidaTerminada, mano.GanadorPartida,
            mano.PuntosEquipoA, mano.PuntosEquipoB,
            mano.EstadoEnvido, mano.EstadoTruco, mano.EnvidoCantado, mano.EnvidoResuelto,
            mano.TipoEnvidoCantado, mano.CantorEnvido, mano.GanadorEnvido,
            mano.PuntosEnvido, mano.PuntosEnvidoNoQuiero, mano.FaseEnvido,
            mano.EnvidoPendienteRespuestaDe, mano.SonBuenasDeclarado, mano.TantosDeclarados,
            mano.TrucoCantado, mano.TrucoResuelto, mano.NivelTruco, mano.PuntosTrucoMano,
            mano.CantorTruco, mano.EquipoCantorTruco, mano.TrucoPendienteRespuestaDe,
            mano.PuedeEscalarTruco, Vueltas = mano.Vueltas, mano.VueltaActual,
        };

        foreach (var (connId, posicion) in state.Posiciones)
        {
            var jId      = $"J{posicion}";
            var jugador  = mano.ObtenerJugador(jId);
            if (jugador == null) continue;

            string equipoId  = mano.ObtenerEquipoDeJugador(jId);
            var equipo       = mano.ObtenerEquipo(equipoId);
            var companeroId  = equipo.Jugadores.FirstOrDefault(j => j.Id != jId)?.Id;
            var companero    = companeroId != null ? mano.ObtenerJugador(companeroId) : null;

            await Clients.Client(connId).SendAsync("TrucoEstado2v2", new
            {
                miRol = jId, miEquipo = equipoId,
                misCartas = jugador.Mano, misJugadas = jugador.Jugadas,
                cartasCompanero = companero?.Jugadas ?? new List<Carta>(),
                estado = baseDto,
            });
        }
    }

    private async Task BroadcastTrucoEstado3v3(string sala, TrucoMultiState3v3 state)
    {
        var mano    = state.Mano;
        var baseDto = new
        {
            mano.NumeroDeMano, mano.TurnoActual, mano.JugadorMano, mano.EquipoMano,
            mano.GanadorMano, mano.ManoTerminada, mano.PartidaTerminada, mano.GanadorPartida,
            mano.PuntosEquipoA, mano.PuntosEquipoB,
            mano.EstadoEnvido, mano.EstadoTruco, mano.EnvidoCantado, mano.EnvidoResuelto,
            mano.TipoEnvidoCantado, mano.CantorEnvido, mano.GanadorEnvido,
            mano.PuntosEnvido, mano.PuntosEnvidoNoQuiero, mano.FaseEnvido,
            mano.EnvidoPendienteRespuestaDe, mano.SonBuenasDeclarado, mano.TantosDeclarados,
            mano.TrucoCantado, mano.TrucoResuelto, mano.NivelTruco, mano.PuntosTrucoMano,
            mano.CantorTruco, mano.EquipoCantorTruco, mano.TrucoPendienteRespuestaDe,
            mano.PuedeEscalarTruco, Vueltas = mano.Vueltas, mano.VueltaActual,
            mano.PicaPica, mano.PicaPicaSlot, mano.JugadoresActivos,
        };

        foreach (var (connId, posicion) in state.Posiciones)
        {
            var jId     = $"J{posicion}";
            var jugador = mano.ObtenerJugador(jId);
            if (jugador == null) continue;

            string equipoId       = mano.ObtenerEquipoDeJugador(jId);
            var equipo            = mano.ObtenerEquipo(equipoId);
            var cartasCompaneros  = equipo.Jugadores
                .Where(j => j.Id != jId)
                .ToDictionary(j => j.Id, j => j.Jugadas);

            await Clients.Client(connId).SendAsync("TrucoEstado3v3", new
            {
                miRol = jId, miEquipo = equipoId,
                misCartas = jugador.Mano, misJugadas = jugador.Jugadas,
                cartasCompaneros,
                estado = baseDto,
            });
        }
    }
}
