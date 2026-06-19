using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.API.Hubs;

public class GameHub : Hub
{
    // ─────────────────────────────────────────────────────────────
    //  Estado compartido de salas
    // ─────────────────────────────────────────────────────────────
    private static readonly ConcurrentDictionary<string, List<string>> _salas = new();
    private static readonly ConcurrentDictionary<string, string> _conexionASala = new();
    private static readonly ConcurrentDictionary<string, TrucoMultiState> _trucoGames = new();
    private static readonly ConcurrentDictionary<string, TrucoMultiState2v2> _trucoGames2v2 = new();
    private static readonly ConcurrentDictionary<string, TrucoMultiState3v3> _trucoGames3v3 = new();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _listos = new();

    // sala -> true si es pública (aparece en "Buscar partida"); las privadas solo por código.
    private static readonly ConcurrentDictionary<string, bool> _salasPublicas = new();

    // ─── 2v2 ─────────────────────────────────────────────────────
    private static readonly ConcurrentDictionary<string, string> _salasModo = new();
    // sala -> connectionId -> "sanMartin" | "belgrano"
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _equiposJugadores = new();

    // ─────────────────────────────────────────────────────────────
    //  SALA — crear / unirse
    // ─────────────────────────────────────────────────────────────
    public async Task<string> CrearSala(string modo = "1v1", bool publica = false)
    {
        // TryAdd en loop: evita pisar una sala existente si el código (6 chars) colisiona.
        string codigo;
        do
        {
            codigo = Guid.NewGuid().ToString("N")[..6].ToUpper();
        } while (!_salas.TryAdd(codigo, new List<string> { Context.ConnectionId }));

        _salasModo[codigo] = modo;
        _salasPublicas[codigo] = publica;
        _conexionASala[Context.ConnectionId] = codigo;
        await Groups.AddToGroupAsync(Context.ConnectionId, codigo);
        return codigo;
    }

    /// <summary>Resumen de una sala pública disponible para unirse.</summary>
    public record SalaPublicaInfo(string Codigo, string Modo, int Jugadores, int MaxJugadores);

    /// <summary>
    /// Lista las salas públicas que todavía tienen lugar y no empezaron la partida.
    /// Por ahora solo 1v1 (el front filtra por modo); el resto se sumará después.
    /// </summary>
    public Task<List<SalaPublicaInfo>> ListarSalasPublicas(string modo = "1v1")
    {
        var resultado = new List<SalaPublicaInfo>();

        foreach (var (codigo, esPublica) in _salasPublicas)
        {
            if (!esPublica) continue;
            if (!_salas.TryGetValue(codigo, out var jugadores)) continue;

            var modoSala = _salasModo.TryGetValue(codigo, out var ms) ? ms : "1v1";
            if (modoSala != modo) continue;

            // No listar salas con la partida ya iniciada.
            bool enJuego = _trucoGames.ContainsKey(codigo)
                        || _trucoGames2v2.ContainsKey(codigo)
                        || _trucoGames3v3.ContainsKey(codigo);
            if (enJuego) continue;

            int max = JugadoresRequeridos(modoSala);
            int cantidad;
            lock (jugadores) { cantidad = jugadores.Count; }
            if (cantidad <= 0 || cantidad >= max) continue; // vacía o llena → no se ofrece

            resultado.Add(new SalaPublicaInfo(codigo, modoSala, cantidad, max));
        }

        return Task.FromResult(resultado);
    }

    public async Task<bool> UnirseASala(string codigo)
    {
        if (!_salas.TryGetValue(codigo, out var jugadores)) return false;

        var modo = _salasModo.TryGetValue(codigo, out var m) ? m : "1v1";
        int maxJugadores = JugadoresRequeridos(modo);

        int cantidad;
        // La List<string> dentro del ConcurrentDictionary no es thread-safe:
        // chequeo de cupo + alta deben ser atómicos para no superar el máximo.
        lock (jugadores)
        {
            if (jugadores.Count >= maxJugadores) return false;
            if (jugadores.Contains(Context.ConnectionId)) return false;
            jugadores.Add(Context.ConnectionId);
            cantidad = jugadores.Count;
        }

        _conexionASala[Context.ConnectionId] = codigo;
        await Groups.AddToGroupAsync(Context.ConnectionId, codigo);

        if (modo == "1v1")
        {
            if (cantidad == 2)
                await Clients.Group(codigo).SendAsync("SalaLista");
        }
        else // 2v2 / 3v3
        {
            await Clients.Group(codigo).SendAsync("LobbyActualizado", new
            {
                jugadoresEnSala = cantidad,
                maxJugadores
            });
            if (cantidad == maxJugadores)
                await Clients.Group(codigo).SendAsync("SalaCompleta");
        }

        return true;
    }

    /// <summary>Cantidad de jugadores requeridos según el modo de sala.</summary>
    private static int JugadoresRequeridos(string modo) => modo switch
    {
        "2v2" => 4,
        "3v3" => 6,
        _     => 2,
    };

    /// <summary>Jugadores por equipo según el modo (2 en 2v2, 3 en 3v3).</summary>
    private static int JugadoresPorEquipo(string modo) => modo == "3v3" ? 3 : 2;

    // ─────────────────────────────────────────────────────────────
    //  EQUIPOS — selección 2v2
    // ─────────────────────────────────────────────────────────────
    public async Task ElegirEquipo(string equipo)
    {
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out var sala)) return;
        if (!_salas.TryGetValue(sala, out var jugadores)) return;
        if (equipo != "sanMartin" && equipo != "belgrano") return;

        var modo = _salasModo.TryGetValue(sala, out var m) ? m : "2v2";
        int cupoPorEquipo = JugadoresPorEquipo(modo);

        var equiposMap = _equiposJugadores.GetOrAdd(sala, _ => new ConcurrentDictionary<string, string>());

        // Verificar si el equipo destino tiene lugar
        equiposMap.TryGetValue(Context.ConnectionId, out var equipoActual);
        int countEnEquipo = equiposMap.Values.Count(v => v == equipo);

        // Si ya está en ese equipo, no hacer nada
        if (equipoActual == equipo) return;

        // Si el equipo destino está lleno, rechazar
        if (countEnEquipo >= cupoPorEquipo) return;

        equiposMap[Context.ConnectionId] = equipo;
        await BroadcastEstadoEquipos(sala, jugadores, equiposMap);
    }

    // ─────────────────────────────────────────────────────────────
    //  MOVIMIENTO — sincronización de posición en el mundo
    // ─────────────────────────────────────────────────────────────
    public async Task ActualizarPosicion(float x, float y, string animacion, string sprite, string escena)
    {
        if (_conexionASala.TryGetValue(Context.ConnectionId, out var sala))
            await Clients.OthersInGroup(sala).SendAsync("PosicionActualizada", x, y, animacion, sprite, escena);
    }

    // ─────────────────────────────────────────────────────────────
    //  TRUCO MULTIJUGADOR
    // ─────────────────────────────────────────────────────────────
    public async Task ListoParaJugar()
    {
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out var sala)) return;
        if (!_salas.TryGetValue(sala, out var jugadores)) return;

        var modo = _salasModo.TryGetValue(sala, out var m) ? m : "1v1";
        int requiredPlayers = JugadoresRequeridos(modo);

        if (jugadores.Count < requiredPlayers) return;

        var readySet = _listos.GetOrAdd(sala, _ => new ConcurrentDictionary<string, bool>());
        readySet[Context.ConnectionId] = true;

        // Notificar cuántos están listos
        await Clients.Group(sala).SendAsync("LobbyListos", new
        {
            listos = readySet.Count,
            requeridos = requiredPlayers
        });

        if (readySet.Count >= requiredPlayers)
        {
            _listos.TryRemove(sala, out _);

            if (modo == "2v2")
            {
                if (_trucoGames2v2.TryGetValue(sala, out var existing2v2))
                {
                    await BroadcastTrucoEstado2v2(sala, existing2v2);
                    return;
                }
                var state2v2 = IniciarNuevaMano2v2(sala, jugadores, esPrimeraPartida: true);
                _trucoGames2v2[sala] = state2v2;
                await BroadcastTrucoEstado2v2(sala, state2v2);
                return;
            }

            if (modo == "3v3")
            {
                if (_trucoGames3v3.TryGetValue(sala, out var existing3v3))
                {
                    await BroadcastTrucoEstado3v3(sala, existing3v3);
                    return;
                }
                var state3v3 = IniciarNuevaMano3v3(sala, jugadores, esPrimeraPartida: true);
                _trucoGames3v3[sala] = state3v3;
                await BroadcastTrucoEstado3v3(sala, state3v3);
                return;
            }

            if (_trucoGames.TryGetValue(sala, out var existing))
            {
                await BroadcastTrucoEstado(sala, existing);
                return;
            }

            var state = new TrucoMultiState
            {
                Jugador1Id = jugadores[0],
                Jugador2Id = jugadores[1],
            };
            TrucoMulti1v1Servicio.IniciarNuevaMano(state, esPrimeraPartida: true);
            _trucoGames[sala] = state;
            await BroadcastTrucoEstado(sala, state);
        }
    }

    public async Task IniciarTruco()
    {
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out var sala)) return;
        if (!_salas.TryGetValue(sala, out var jugadores) || jugadores.Count < 2) return;

        var modo = _salasModo.TryGetValue(sala, out var m) ? m : "1v1";

        if (modo == "2v2")
        {
            if (jugadores.Count < 4) return;
            var state2v2 = IniciarNuevaMano2v2(sala, jugadores, esPrimeraPartida: true);
            _trucoGames2v2[sala] = state2v2;
            await BroadcastTrucoEstado2v2(sala, state2v2);
            return;
        }

        if (modo == "3v3")
        {
            if (jugadores.Count < 6) return;
            var state3v3 = IniciarNuevaMano3v3(sala, jugadores, esPrimeraPartida: true);
            _trucoGames3v3[sala] = state3v3;
            await BroadcastTrucoEstado3v3(sala, state3v3);
            return;
        }

        var state = new TrucoMultiState
        {
            Jugador1Id = jugadores[0],
            Jugador2Id = jugadores[1],
        };
        TrucoMulti1v1Servicio.IniciarNuevaMano(state, esPrimeraPartida: true);
        _trucoGames[sala] = state;
        await BroadcastTrucoEstado(sala, state);
    }

    // ─────────────────────────────────────────────────────────────
    //  1v1 — Jugar carta
    // ─────────────────────────────────────────────────────────────
    public async Task JugarCarta(int numero, string palo)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.JugarCarta(state, esJ1, numero, palo)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    // ─────────────────────────────────────────────────────────────
    //  2v2 — Jugar carta
    // ─────────────────────────────────────────────────────────────
    public async Task JugarCarta2v2(int numero, string palo)
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var jugadorId = state2v2!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        // Las validaciones (turno, cantos pendientes, carta en mano) viven en el dominio.
        if (!JuegoServicio2v2.JugarCartaPorValor(state2v2.Mano, jugadorId, numero, palo)) return;

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    // ─────────────────────────────────────────────────────────────
    //  1v1 — Envido
    // ─────────────────────────────────────────────────────────────
    public async Task SolicitarEnvido(string tipo)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.CantarEnvido(state, esJ1, tipo)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task ResponderEnvido(bool aceptar)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.ResponderEnvido(state, esJ1, aceptar)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    /// <summary>"Son buenas" en modo 1v1 multijugador: el que responde reconoce que pierde el envido.</summary>
    public async Task SonBuenas()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.SonBuenas(state, esJ1)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task EscalarEnvido(string tipo)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.EscalarEnvido(state, esJ1, tipo)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    // ─────────────────────────────────────────────────────────────
    //  2v2 — Envido
    // ─────────────────────────────────────────────────────────────
    public async Task SolicitarEnvido2v2(string tipo)
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var jugadorId = state2v2!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!EnvidoServicio2v2.Cantar(state2v2.Mano, jugadorId, tipo, TurnoServicio2v2.ObtenerResponsableCanto)) return;

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    public async Task ResponderEnvido2v2(bool aceptar)
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var jugadorId = state2v2!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!EnvidoServicio2v2.Responder(state2v2.Mano, jugadorId, aceptar)) return;

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    public async Task DeclararTanto2v2(int tanto)
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var mano = state2v2!.Mano;
        if (mano.FaseEnvido != "declarando_tantos") return;

        var jugadorId = state2v2.GetJugadorId(Context.ConnectionId);
        if (mano.EnvidoPendienteRespuestaDe != jugadorId) return;

        EnvidoServicio2v2.ProcesarDeclaracion(mano, jugadorId, tanto, sonBuenas: false);

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    public async Task SonBuenas2v2()
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var mano = state2v2!.Mano;
        if (mano.FaseEnvido != "declarando_tantos") return;

        var jugadorId = state2v2.GetJugadorId(Context.ConnectionId);
        if (mano.EnvidoPendienteRespuestaDe != jugadorId) return;

        EnvidoServicio2v2.ProcesarDeclaracion(mano, jugadorId, null, sonBuenas: true);

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    /// <summary>Escala el envido en 2v2 (Envido → Envido Envido → Real Envido → Falta Envido).</summary>
    public async Task EscalarEnvido2v2(string tipo)
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var jugadorId = state2v2!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!EnvidoServicio2v2.Escalar(state2v2.Mano, jugadorId, tipo, TurnoServicio2v2.ObtenerResponsableCanto)) return;

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    // ─────────────────────────────────────────────────────────────
    //  1v1 — Truco
    // ─────────────────────────────────────────────────────────────
    public async Task SolicitarTruco()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.CantarTruco(state, esJ1)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task ResponderTruco(bool aceptar, string? escalarA)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.ResponderTruco(state, esJ1, aceptar, escalarA)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task EscalarTruco()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.EscalarTruco(state, esJ1)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    // ─────────────────────────────────────────────────────────────
    //  2v2 — Truco
    // ─────────────────────────────────────────────────────────────
    public async Task SolicitarTruco2v2()
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var jugadorId = state2v2!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!TrucoServicio2v2.Cantar(state2v2.Mano, jugadorId, TurnoServicio2v2.ObtenerResponsableCanto)) return;

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    public async Task ResponderTruco2v2(bool aceptar, string? escalarA)
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var jugadorId = state2v2!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!TrucoServicio2v2.Responder(state2v2.Mano, jugadorId, aceptar, escalarA, TurnoServicio2v2.ObtenerResponsableCanto)) return;

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    /// <summary>Irse al mazo en 2v2: el equipo del que se va pierde la mano.</summary>
    public async Task IrseAlMazo2v2()
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var jugadorId = state2v2!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!TrucoServicio2v2.IrseAlMazo(state2v2.Mano, jugadorId)) return;

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    /// <summary>Subir la apuesta del truco en tu turno (retruco / vale cuatro) tras haberlo aceptado.</summary>
    public async Task EscalarTruco2v2()
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var jugadorId = state2v2!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!TrucoServicio2v2.Escalar(state2v2.Mano, jugadorId, TurnoServicio2v2.ObtenerResponsableCanto)) return;

        _trucoGames2v2[sala!] = state2v2;
        await BroadcastTrucoEstado2v2(sala!, state2v2);
    }

    // ─────────────────────────────────────────────────────────────
    //  Irse al mazo (1v1)
    // ─────────────────────────────────────────────────────────────
    public async Task IrseAlMazo()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        bool esJ1 = Context.ConnectionId == state!.Jugador1Id;
        if (!TrucoMulti1v1Servicio.IrseAlMazo(state, esJ1)) return;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    // ─────────────────────────────────────────────────────────────
    //  Nueva mano / partida
    // ─────────────────────────────────────────────────────────────
    public async Task NuevaMano()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        if (state!.Mano.GanadorMano == null && !state.Mano.PartidaTerminada) return;
        TrucoMulti1v1Servicio.IniciarNuevaMano(state, esPrimeraPartida: false);
        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task NuevaPartida()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        TrucoMulti1v1Servicio.IniciarNuevaMano(state, esPrimeraPartida: true);
        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task NuevaMano2v2()
    {
        if (!ObtenerSalaYEstado2v2(out var sala, out var state2v2)) return;
        var mano = state2v2!.Mano;
        if (mano.GanadorMano == null && !mano.PartidaTerminada) return;

        if (!_salas.TryGetValue(sala!, out var jugadores)) return;
        var nuevoState = IniciarNuevaMano2v2(sala!, jugadores, esPrimeraPartida: false, estadoAnterior: mano);
        _trucoGames2v2[sala!] = nuevoState;
        await BroadcastTrucoEstado2v2(sala!, nuevoState);
    }

    // ─────────────────────────────────────────────────────────────
    //  3v3 — Jugar carta / Envido / Truco (multijugador, 6 reales)
    // ─────────────────────────────────────────────────────────────
    public async Task JugarCarta3v3(int numero, string palo)
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var jugadorId = state3v3!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        // Las validaciones (turno, cantos pendientes, carta en mano) viven en el dominio.
        if (!JuegoServicio3v3.JugarCartaPorValor(state3v3.Mano, jugadorId, numero, palo)) return;

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task SolicitarEnvido3v3(string tipo)
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var jugadorId = state3v3!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!EnvidoServicio3v3.Cantar(state3v3.Mano, jugadorId, tipo, TurnoServicio3v3.ObtenerResponsableCanto)) return;

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task ResponderEnvido3v3(bool aceptar)
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var jugadorId = state3v3!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!EnvidoServicio3v3.Responder(state3v3.Mano, jugadorId, aceptar)) return;

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task DeclararTanto3v3(int tanto)
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var mano = state3v3!.Mano;
        if (mano.FaseEnvido != "declarando_tantos") return;

        var jugadorId = state3v3.GetJugadorId(Context.ConnectionId);
        if (mano.EnvidoPendienteRespuestaDe != jugadorId) return;

        EnvidoServicio3v3.ProcesarDeclaracion(mano, jugadorId, tanto, sonBuenas: false);

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task SonBuenas3v3()
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var mano = state3v3!.Mano;
        if (mano.FaseEnvido != "declarando_tantos") return;

        var jugadorId = state3v3.GetJugadorId(Context.ConnectionId);
        if (mano.EnvidoPendienteRespuestaDe != jugadorId) return;

        EnvidoServicio3v3.ProcesarDeclaracion(mano, jugadorId, null, sonBuenas: true);

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task EscalarEnvido3v3(string tipo)
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var jugadorId = state3v3!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!EnvidoServicio3v3.Escalar(state3v3.Mano, jugadorId, tipo, TurnoServicio3v3.ObtenerResponsableCanto)) return;

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task SolicitarTruco3v3()
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var jugadorId = state3v3!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!TrucoServicio3v3.Cantar(state3v3.Mano, jugadorId, TurnoServicio3v3.ObtenerResponsableCanto)) return;

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task ResponderTruco3v3(bool aceptar, string? escalarA)
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var jugadorId = state3v3!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!TrucoServicio3v3.Responder(state3v3.Mano, jugadorId, aceptar, escalarA, TurnoServicio3v3.ObtenerResponsableCanto)) return;

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task IrseAlMazo3v3()
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var jugadorId = state3v3!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!TrucoServicio3v3.IrseAlMazo(state3v3.Mano, jugadorId)) return;

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task EscalarTruco3v3()
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var jugadorId = state3v3!.GetJugadorId(Context.ConnectionId);
        if (string.IsNullOrEmpty(jugadorId)) return;

        if (!TrucoServicio3v3.Escalar(state3v3.Mano, jugadorId, TurnoServicio3v3.ObtenerResponsableCanto)) return;

        _trucoGames3v3[sala!] = state3v3;
        await BroadcastTrucoEstado3v3(sala!, state3v3);
    }

    public async Task NuevaMano3v3()
    {
        if (!ObtenerSalaYEstado3v3(out var sala, out var state3v3)) return;
        var mano = state3v3!.Mano;
        if (mano.GanadorMano == null && !mano.PartidaTerminada) return;

        if (!_salas.TryGetValue(sala!, out var jugadores)) return;
        var nuevoState = IniciarNuevaMano3v3(sala!, jugadores, esPrimeraPartida: false, estadoAnterior: mano);
        _trucoGames3v3[sala!] = nuevoState;
        await BroadcastTrucoEstado3v3(sala!, nuevoState);
    }

    // ─────────────────────────────────────────────────────────────
    //  Desconexión
    // ─────────────────────────────────────────────────────────────
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_conexionASala.TryRemove(Context.ConnectionId, out var sala))
        {
            if (_salas.TryGetValue(sala, out var jugadores))
            {
                bool salaVacia;
                lock (jugadores)
                {
                    jugadores.Remove(Context.ConnectionId);
                    salaVacia = jugadores.Count == 0;
                }
                if (salaVacia)
                {
                    _salas.TryRemove(sala, out _);
                    _salasModo.TryRemove(sala, out _);
                    _salasPublicas.TryRemove(sala, out _);
                    _equiposJugadores.TryRemove(sala, out _);
                    _listos.TryRemove(sala, out _);
                }
            }
            _trucoGames.TryRemove(sala, out _);
            _trucoGames2v2.TryRemove(sala, out _);
            _trucoGames3v3.TryRemove(sala, out _);
            if (_listos.TryGetValue(sala, out var readySet))
                readySet.TryRemove(Context.ConnectionId, out _);

            // Limpiar equipo del jugador desconectado y notificar al grupo
            if (_equiposJugadores.TryGetValue(sala, out var equiposMap))
            {
                equiposMap.TryRemove(Context.ConnectionId, out _);
                if (_salas.TryGetValue(sala, out var restantes) && restantes.Count > 0)
                    await BroadcastEstadoEquipos(sala, restantes, equiposMap);
            }

            await Clients.Group(sala).SendAsync("JugadorDesconectado");
        }
        await base.OnDisconnectedAsync(exception);
    }

    // ─────────────────────────────────────────────────────────────
    //  Helpers privados
    // ─────────────────────────────────────────────────────────────
    private bool ObtenerSalaYEstado(out string? sala, out TrucoMultiState? state)
    {
        sala = null; state = null;
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out sala)) return false;
        if (!_trucoGames.TryGetValue(sala, out state)) return false;
        return true;
    }

    private bool ObtenerSalaYEstado2v2(out string? sala, out TrucoMultiState2v2? state)
    {
        sala = null; state = null;
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out sala)) return false;
        if (!_trucoGames2v2.TryGetValue(sala, out state)) return false;
        return true;
    }

    private bool ObtenerSalaYEstado3v3(out string? sala, out TrucoMultiState3v3? state)
    {
        sala = null; state = null;
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out sala)) return false;
        if (!_trucoGames3v3.TryGetValue(sala, out state)) return false;
        return true;
    }

    private static TrucoMultiState2v2 IniciarNuevaMano2v2(
        string sala,
        List<string> jugadores,
        bool esPrimeraPartida,
        ManoTruco2v2? estadoAnterior = null)
    {
        int numMano    = esPrimeraPartida ? 1 : (estadoAnterior?.NumeroDeMano ?? 0) + 1;
        int ptsA       = esPrimeraPartida ? 0 : estadoAnterior?.PuntosEquipoA ?? 0;
        int ptsB       = esPrimeraPartida ? 0 : estadoAnterior?.PuntosEquipoB ?? 0;

        // Crear jugadores con ids basados en posición (J1..J4)
        var jugadoresEntidad = jugadores.Take(4).Select((connId, i) => new Jugador
        {
            Id       = $"J{i + 1}",
            Nombre   = $"Jugador {i + 1}",
            EsMaquina = false
        }).ToArray();

        var mano = PartidaServicio2v2.CrearManoNueva(
            numeroDeMano: numMano,
            puntosEquipoA: ptsA,
            puntosEquipoB: ptsB,
            pos1: jugadoresEntidad.Length > 0 ? jugadoresEntidad[0] : null,
            pos2: jugadoresEntidad.Length > 1 ? jugadoresEntidad[1] : null,
            pos3: jugadoresEntidad.Length > 2 ? jugadoresEntidad[2] : null,
            pos4: jugadoresEntidad.Length > 3 ? jugadoresEntidad[3] : null);

        var state2v2 = new TrucoMultiState2v2 { Mano = mano };

        // Mapear connectionId → posición
        for (int i = 0; i < Math.Min(jugadores.Count, 4); i++)
            state2v2.Posiciones[jugadores[i]] = i + 1;

        state2v2.JugadoresIds = jugadores.Take(4).ToArray();

        return state2v2;
    }

    private static TrucoMultiState3v3 IniciarNuevaMano3v3(
        string sala,
        List<string> jugadores,
        bool esPrimeraPartida,
        ManoTruco3v3? estadoAnterior = null)
    {
        int numMano  = esPrimeraPartida ? 1 : (estadoAnterior?.NumeroDeMano ?? 0) + 1;
        int ptsA     = esPrimeraPartida ? 0 : estadoAnterior?.PuntosEquipoA ?? 0;
        int ptsB     = esPrimeraPartida ? 0 : estadoAnterior?.PuntosEquipoB ?? 0;
        int prevSlot = esPrimeraPartida ? -1 : estadoAnterior?.PicaPicaSlot ?? -1;

        // Crear jugadores con ids basados en posición (J1..J6).
        // Se generan siempre los 6 (CrearProximaMano los requiere no-nulos).
        var jugadoresEntidad = Enumerable.Range(1, 6).Select(i => new Jugador
        {
            Id        = $"J{i}",
            Nombre    = $"Jugador {i}",
            EsMaquina = false
        }).ToArray();

        // CrearProximaMano aplica el ciclo Pica-Pica (igual que el modo solo):
        // redondas hasta 5 pts, luego ciclos de 3 duelos 1v1 + 1 redonda hasta 25,
        // y de ahí solo redondas hasta 30.
        var mano = PartidaServicio3v3.CrearProximaMano(
            numMano, ptsA, ptsB, prevSlot,
            jugadoresEntidad[0], jugadoresEntidad[1], jugadoresEntidad[2],
            jugadoresEntidad[3], jugadoresEntidad[4], jugadoresEntidad[5]);

        var state3v3 = new TrucoMultiState3v3 { Mano = mano };

        for (int i = 0; i < Math.Min(jugadores.Count, 6); i++)
            state3v3.Posiciones[jugadores[i]] = i + 1;

        state3v3.JugadoresIds = jugadores.Take(6).ToArray();

        return state3v3;
    }

    private async Task BroadcastEstadoEquipos(string sala, List<string> jugadores, ConcurrentDictionary<string, string> equiposMap)
    {
        var modo = _salasModo.TryGetValue(sala, out var mm) ? mm : "2v2";
        int cupoPorEquipo = JugadoresPorEquipo(modo);
        int countSanMartin = equiposMap.Values.Count(v => v == "sanMartin");
        int countBelgrano  = equiposMap.Values.Count(v => v == "belgrano");
        bool equiposListos = countSanMartin == cupoPorEquipo && countBelgrano == cupoPorEquipo;

        var jugadoresDto = jugadores.Select((cId, i) => new
        {
            posicion = i + 1,
            equipo   = equiposMap.TryGetValue(cId, out var eq) ? eq : (string?)null
        }).ToList();

        for (int i = 0; i < jugadores.Count; i++)
        {
            await Clients.Client(jugadores[i]).SendAsync("EstadoEquipos", new
            {
                miPosicion     = i + 1,
                jugadores      = jugadoresDto,
                equiposListos,
                countSanMartin,
                countBelgrano
            });
        }
    }

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

    private async Task BroadcastTrucoEstado(string sala, TrucoMultiState state)
    {
        var mano = state.Mano;
        var baseDto = new
        {
            mano.NumeroDeMano,
            mano.TurnoActual,
            mano.ManoIniciadaPor,
            mano.GanadorMano,
            mano.PartidaTerminada,
            mano.GanadorPartida,
            mano.PuntosHumano,
            mano.PuntosMaquina,
            mano.EstadoEnvido,
            mano.EstadoTruco,
            mano.EnvidoCantado,
            mano.EnvidoResuelto,
            mano.TipoEnvidoCantado,
            mano.CantorEnvido,
            mano.TantoHumano,
            mano.TantoMaquina,
            mano.TrucoCantado,
            mano.TrucoResuelto,
            mano.NivelTruco,
            mano.PuntosTrucoMano,
            mano.CantorTruco,
            mano.Bazas,
            mano.SonBuenasDeclarado,
            CartaPendienteJ1  = state.CartaPendienteJ1,
            CartaPendienteJ2  = mano.CartaMaquinaEnMesa,
            EnvidoPendienteJ1 = mano.EnvidoPendienteRespuestaHumano,
            EnvidoPendienteJ2 = state.EnvidoPendienteRespuestaJ2,
            TrucoPendienteJ1  = mano.TrucoPendienteRespuestaHumano,
            TrucoPendienteJ2  = state.TrucoPendienteRespuestaJ2,
        };

        await Clients.Client(state.Jugador1Id).SendAsync("TrucoEstado", new
        {
            miRol                  = "J1",
            misManos               = mano.Humano.Mano,
            misJugadas             = mano.Humano.Jugadas,
            cantidadCartasOponente = mano.Maquina.Mano.Count,
            estado                 = baseDto
        });

        await Clients.Client(state.Jugador2Id).SendAsync("TrucoEstado", new
        {
            miRol                  = "J2",
            misManos               = mano.Maquina.Mano,
            misJugadas             = mano.Maquina.Jugadas,
            cantidadCartasOponente = mano.Humano.Mano.Count,
            estado                 = baseDto
        });
    }

    private async Task BroadcastTrucoEstado2v2(string sala, TrucoMultiState2v2 state2v2)
    {
        var mano = state2v2.Mano;
        var baseDto = new
        {
            mano.NumeroDeMano,
            mano.TurnoActual,
            mano.JugadorMano,
            mano.EquipoMano,
            mano.GanadorMano,
            mano.ManoTerminada,
            mano.PartidaTerminada,
            mano.GanadorPartida,
            mano.PuntosEquipoA,
            mano.PuntosEquipoB,
            mano.EstadoEnvido,
            mano.EstadoTruco,
            mano.EnvidoCantado,
            mano.EnvidoResuelto,
            mano.TipoEnvidoCantado,
            mano.CantorEnvido,
            mano.GanadorEnvido,
            mano.PuntosEnvido,
            mano.PuntosEnvidoNoQuiero,
            mano.FaseEnvido,
            mano.EnvidoPendienteRespuestaDe,
            mano.SonBuenasDeclarado,
            mano.TantosDeclarados,
            mano.TrucoCantado,
            mano.TrucoResuelto,
            mano.NivelTruco,
            mano.PuntosTrucoMano,
            mano.CantorTruco,
            mano.EquipoCantorTruco,
            mano.TrucoPendienteRespuestaDe,
            mano.PuedeEscalarTruco,
            Vueltas = mano.Vueltas,
            mano.VueltaActual,
        };

        // Enviar estado personalizado a cada jugador (solo ve sus propias cartas)
        foreach (var (connId, posicion) in state2v2.Posiciones)
        {
            var jugadorId = $"J{posicion}";
            var jugador   = mano.ObtenerJugador(jugadorId);
            if (jugador == null) continue;

            string equipoId = mano.ObtenerEquipoDeJugador(jugadorId);
            var equipo      = mano.ObtenerEquipo(equipoId);
            var companeroId = equipo.Jugadores.FirstOrDefault(j => j.Id != jugadorId)?.Id;
            var companero   = companeroId != null ? mano.ObtenerJugador(companeroId) : null;

            await Clients.Client(connId).SendAsync("TrucoEstado2v2", new
            {
                miRol            = jugadorId,
                miEquipo         = equipoId,
                misCartas        = jugador.Mano,
                misJugadas       = jugador.Jugadas,
                cartasCompanero  = companero?.Jugadas ?? new List<Carta>(),
                estado           = baseDto
            });
        }
    }

    private async Task BroadcastTrucoEstado3v3(string sala, TrucoMultiState3v3 state3v3)
    {
        var mano = state3v3.Mano;
        var baseDto = new
        {
            mano.NumeroDeMano,
            mano.TurnoActual,
            mano.JugadorMano,
            mano.EquipoMano,
            mano.GanadorMano,
            mano.ManoTerminada,
            mano.PartidaTerminada,
            mano.GanadorPartida,
            mano.PuntosEquipoA,
            mano.PuntosEquipoB,
            mano.EstadoEnvido,
            mano.EstadoTruco,
            mano.EnvidoCantado,
            mano.EnvidoResuelto,
            mano.TipoEnvidoCantado,
            mano.CantorEnvido,
            mano.GanadorEnvido,
            mano.PuntosEnvido,
            mano.PuntosEnvidoNoQuiero,
            mano.FaseEnvido,
            mano.EnvidoPendienteRespuestaDe,
            mano.SonBuenasDeclarado,
            mano.TantosDeclarados,
            mano.TrucoCantado,
            mano.TrucoResuelto,
            mano.NivelTruco,
            mano.PuntosTrucoMano,
            mano.CantorTruco,
            mano.EquipoCantorTruco,
            mano.TrucoPendienteRespuestaDe,
            mano.PuedeEscalarTruco,
            Vueltas = mano.Vueltas,
            mano.VueltaActual,
            mano.PicaPica,
            mano.PicaPicaSlot,
            mano.JugadoresActivos,
        };

        // Enviar estado personalizado a cada jugador (solo ve sus propias cartas)
        foreach (var (connId, posicion) in state3v3.Posiciones)
        {
            var jugadorId = $"J{posicion}";
            var jugador   = mano.ObtenerJugador(jugadorId);
            if (jugador == null) continue;

            string equipoId = mano.ObtenerEquipoDeJugador(jugadorId);
            var equipo      = mano.ObtenerEquipo(equipoId);
            // Jugadas de los DOS compañeros (resto del equipo), keyed por rol.
            var cartasCompaneros = equipo.Jugadores
                .Where(j => j.Id != jugadorId)
                .ToDictionary(j => j.Id, j => j.Jugadas);

            await Clients.Client(connId).SendAsync("TrucoEstado3v3", new
            {
                miRol            = jugadorId,
                miEquipo         = equipoId,
                misCartas        = jugador.Mano,
                misJugadas       = jugador.Jugadas,
                cartasCompaneros = cartasCompaneros,
                estado           = baseDto
            });
        }
    }
}
