using System.Collections.Concurrent;
using TrucoRPG.API.Hubs;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.API.Services;

// ─── DTOs compartidos ─────────────────────────────────────────────────────────

/// <summary>Info de una sala pública disponible para unirse.</summary>
public record SalaPublicaInfo(string Codigo, string Modo, int Jugadores, int MaxJugadores);

// ─── DTOs de resultado ────────────────────────────────────────────────────────

public record ResultadoUnirse(
    bool Ok,
    string Modo,
    int Cantidad,
    int Max);

public record ResultadoAbandonar(
    string? Sala,
    bool SalaVacia,
    List<string> JugadoresRestantes);

public record ResultadoEquipo(
    bool Ok,
    string Sala,
    List<string> Jugadores,
    ConcurrentDictionary<string, string> EquiposMap,
    string Modo);

public record ResultadoListoParaJugar(
    bool TodosListos,
    string Modo,
    string Sala,
    List<string> Jugadores,
    int CantidadListos,
    int Requeridos);

public record ResultadoDesconexion(
    string? Sala,
    bool SalaVacia,
    List<string> JugadoresRestantes,
    ConcurrentDictionary<string, string>? EquiposMap,
    string Modo);

// ─── Servicio ─────────────────────────────────────────────────────────────────

/// <summary>
/// Singleton que gestiona todo el estado de salas multijugador.
/// El GameHub solo orquesta SignalR (recibe mensajes y hace broadcast);
/// toda la lógica de ciclo de vida de salas vive aquí.
/// </summary>
public class SalaService
{
    // ── Estado ────────────────────────────────────────────────────────────────
    private readonly ConcurrentDictionary<string, List<string>> _salas          = new();
    private readonly ConcurrentDictionary<string, string>       _conexionASala  = new();
    private readonly ConcurrentDictionary<string, bool>         _salasPublicas  = new();
    private readonly ConcurrentDictionary<string, string>       _salasModo      = new();

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _equiposJugadores = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>   _listos           = new();

    private readonly ConcurrentDictionary<string, TrucoMultiState>    _trucoGames    = new();
    private readonly ConcurrentDictionary<string, TrucoMultiState2v2> _trucoGames2v2 = new();
    private readonly ConcurrentDictionary<string, TrucoMultiState3v3> _trucoGames3v3 = new();

    // ── Sala: crear ───────────────────────────────────────────────────────────

    public virtual string CrearSala(string connectionId, string modo, bool publica)
    {
        string codigo;
        do { codigo = Guid.NewGuid().ToString("N")[..6].ToUpper(); }
        while (!_salas.TryAdd(codigo, new List<string> { connectionId }));

        _salasModo[codigo]          = modo;
        _salasPublicas[codigo]      = publica;
        _conexionASala[connectionId] = codigo;
        return codigo;
    }

    // ── Sala: unirse ──────────────────────────────────────────────────────────

    public virtual ResultadoUnirse UnirseASala(string connectionId, string codigo)
    {
        if (!_salas.TryGetValue(codigo, out var jugadores))
            return new ResultadoUnirse(false, "", 0, 0);

        var modo = _salasModo.TryGetValue(codigo, out var m) ? m : "1v1";
        int max  = ModoJuegoConfig.JugadoresRequeridos(modo);

        int cantidad;
        lock (jugadores)
        {
            if (jugadores.Count >= max || jugadores.Contains(connectionId))
                return new ResultadoUnirse(false, modo, jugadores.Count, max);
            jugadores.Add(connectionId);
            cantidad = jugadores.Count;
        }

        _conexionASala[connectionId] = codigo;
        return new ResultadoUnirse(true, modo, cantidad, max);
    }

    // ── Sala: abandonar ───────────────────────────────────────────────────────

    public ResultadoAbandonar AbandonarSala(string connectionId)
    {
        if (!_conexionASala.TryRemove(connectionId, out var sala))
            return new ResultadoAbandonar(null, false, new List<string>());

        List<string> restantes = new();
        bool salaVacia = false;

        if (_salas.TryGetValue(sala, out var jugadores))
        {
            lock (jugadores)
            {
                jugadores.Remove(connectionId);
                salaVacia = jugadores.Count == 0;
                restantes = new List<string>(jugadores);
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
            readySet.TryRemove(connectionId, out _);

        // Liberar el cupo de equipo del jugador que se fue (si no, el slot
        // queda "ocupado" y bloquea a los próximos que quieran ese equipo).
        if (_equiposJugadores.TryGetValue(sala, out var equipos))
            equipos.TryRemove(connectionId, out _);

        return new ResultadoAbandonar(sala, salaVacia, restantes);
    }

    // ── Sala: listar públicas ─────────────────────────────────────────────────

    public List<SalaPublicaInfo> ListarSalasPublicas(string modo)
    {
        var resultado = new List<SalaPublicaInfo>();

        foreach (var (codigo, esPublica) in _salasPublicas)
        {
            if (!esPublica) continue;
            if (!_salas.TryGetValue(codigo, out var jugadores)) continue;

            var modoSala = _salasModo.TryGetValue(codigo, out var ms) ? ms : "1v1";
            if (modoSala != modo) continue;

            bool enJuego = _trucoGames.ContainsKey(codigo)
                        || _trucoGames2v2.ContainsKey(codigo)
                        || _trucoGames3v3.ContainsKey(codigo);
            if (enJuego) continue;

            int cantidad;
            lock (jugadores)
            {
                jugadores.RemoveAll(c => !_conexionASala.ContainsKey(c));
                cantidad = jugadores.Count;
            }

            if (cantidad <= 0)
            {
                _salas.TryRemove(codigo, out _);
                _salasModo.TryRemove(codigo, out _);
                _salasPublicas.TryRemove(codigo, out _);
                continue;
            }

            int max = ModoJuegoConfig.JugadoresRequeridos(modoSala);
            if (cantidad >= max) continue;

            resultado.Add(new SalaPublicaInfo(codigo, modoSala, cantidad, max));
        }

        return resultado;
    }

    // ── Equipos ───────────────────────────────────────────────────────────────

    public ResultadoEquipo? ElegirEquipo(string connectionId, string equipo)
    {
        if (!_conexionASala.TryGetValue(connectionId, out var sala)) return null;
        if (!_salas.TryGetValue(sala, out var jugadores)) return null;
        if (equipo != "sanMartin" && equipo != "belgrano") return null;

        var modo = _salasModo.TryGetValue(sala, out var m) ? m : "2v2";
        int cupo = ModoJuegoConfig.JugadoresPorEquipo(modo);

        var map = _equiposJugadores.GetOrAdd(sala, _ => new ConcurrentDictionary<string, string>());

        map.TryGetValue(connectionId, out var actual);
        if (actual == equipo) return null;                                    // ya está en ese equipo
        if (map.Values.Count(v => v == equipo) >= cupo) return null;          // equipo lleno

        map[connectionId] = equipo;
        List<string> jugadoresSnapshot;
        lock (jugadores) { jugadoresSnapshot = new List<string>(jugadores); }

        return new ResultadoEquipo(true, sala, jugadoresSnapshot, map, modo);
    }

    // ── Listo para jugar ──────────────────────────────────────────────────────

    public ResultadoListoParaJugar? MarcarListo(string connectionId)
    {
        if (!_conexionASala.TryGetValue(connectionId, out var sala)) return null;
        if (!_salas.TryGetValue(sala, out var jugadores)) return null;

        var modo = _salasModo.TryGetValue(sala, out var m) ? m : "1v1";
        int requeridos = ModoJuegoConfig.JugadoresRequeridos(modo);

        if (jugadores.Count < requeridos) return null;

        var readySet = _listos.GetOrAdd(sala, _ => new ConcurrentDictionary<string, bool>());
        readySet[connectionId] = true;
        int cantidadListos = readySet.Count;

        if (cantidadListos < requeridos)
            return new ResultadoListoParaJugar(false, modo, sala, new List<string>(jugadores), cantidadListos, requeridos);

        _listos.TryRemove(sala, out _);
        return new ResultadoListoParaJugar(true, modo, sala, new List<string>(jugadores), cantidadListos, requeridos);
    }

    // ── Desconexión ───────────────────────────────────────────────────────────

    public ResultadoDesconexion OnDesconectado(string connectionId)
    {
        var resultado = AbandonarSala(connectionId);

        ConcurrentDictionary<string, string>? equiposMap = null;
        string modo = "1v1";

        if (resultado.Sala != null)
        {
            modo = _salasModo.TryGetValue(resultado.Sala, out var m) ? m : "1v1";

            if (!resultado.SalaVacia && _equiposJugadores.TryGetValue(resultado.Sala, out var em))
            {
                em.TryRemove(connectionId, out _);
                equiposMap = em;
            }
        }

        return new ResultadoDesconexion(resultado.Sala, resultado.SalaVacia, resultado.JugadoresRestantes, equiposMap, modo);
    }

    // ── Estado de juego: accessors ────────────────────────────────────────────

    public bool TryGetEstado1v1(string connectionId, out string? sala, out TrucoMultiState? state)
    {
        sala = null; state = null;
        return _conexionASala.TryGetValue(connectionId, out sala)
            && _trucoGames.TryGetValue(sala, out state);
    }

    public bool TryGetEstado2v2(string connectionId, out string? sala, out TrucoMultiState2v2? state)
    {
        sala = null; state = null;
        return _conexionASala.TryGetValue(connectionId, out sala)
            && _trucoGames2v2.TryGetValue(sala, out state);
    }

    public bool TryGetEstado3v3(string connectionId, out string? sala, out TrucoMultiState3v3? state)
    {
        sala = null; state = null;
        return _conexionASala.TryGetValue(connectionId, out sala)
            && _trucoGames3v3.TryGetValue(sala, out state);
    }

    public void SetEstado1v1(string sala, TrucoMultiState state)    => _trucoGames[sala]    = state;
    public void SetEstado2v2(string sala, TrucoMultiState2v2 state) => _trucoGames2v2[sala] = state;
    public void SetEstado3v3(string sala, TrucoMultiState3v3 state) => _trucoGames3v3[sala] = state;

    public bool HayJuego1v1(string sala)  => _trucoGames.ContainsKey(sala);
    public bool HayJuego2v2(string sala)  => _trucoGames2v2.ContainsKey(sala);
    public bool HayJuego3v3(string sala)  => _trucoGames3v3.ContainsKey(sala);

    public TrucoMultiState?    GetEstado1v1(string sala) => _trucoGames.TryGetValue(sala, out var s) ? s : null;
    public TrucoMultiState2v2? GetEstado2v2(string sala) => _trucoGames2v2.TryGetValue(sala, out var s) ? s : null;
    public TrucoMultiState3v3? GetEstado3v3(string sala) => _trucoGames3v3.TryGetValue(sala, out var s) ? s : null;

    public string? GetSala(string connectionId) =>
        _conexionASala.TryGetValue(connectionId, out var s) ? s : null;

    public string GetModo(string sala) =>
        _salasModo.TryGetValue(sala, out var m) ? m : "1v1";

    public List<string> GetJugadores(string sala) =>
        _salas.TryGetValue(sala, out var j) ? new List<string>(j) : new List<string>();

    public ConcurrentDictionary<string, string>? GetEquiposMap(string sala) =>
        _equiposJugadores.TryGetValue(sala, out var m) ? m : null;

    // ── Inicializar nueva mano (lógica extraída del Hub) ──────────────────────

    /// <summary>
    /// Ordena las conexiones respetando los equipos elegidos en el lobby:
    /// "sanMartin" ocupa las posiciones IMPARES (1,3,5 → EquipoA) y "belgrano"
    /// las PARES (2,4,6 → EquipoB), porque PartidaServicio2v2/3v3 arma EquipoA
    /// con las posiciones impares y EquipoB con las pares. Si los equipos no
    /// están completos (p. ej. partida iniciada sin lobby), cae al orden de llegada.
    /// </summary>
    private List<string> OrdenarJugadoresPorEquipos(string sala, int porEquipo)
    {
        var jugadores = GetJugadores(sala);
        if (!_equiposJugadores.TryGetValue(sala, out var equipos))
            return jugadores;

        var equipoA = jugadores.Where(j => equipos.TryGetValue(j, out var e) && e == "sanMartin").ToList();
        var equipoB = jugadores.Where(j => equipos.TryGetValue(j, out var e) && e == "belgrano").ToList();
        if (equipoA.Count != porEquipo || equipoB.Count != porEquipo)
            return jugadores; // equipos incompletos → orden de llegada

        var ordenados = new List<string>(porEquipo * 2);
        for (int i = 0; i < porEquipo; i++)
        {
            ordenados.Add(equipoA[i]); // posición impar → EquipoA
            ordenados.Add(equipoB[i]); // posición par   → EquipoB
        }
        return ordenados;
    }

    public TrucoMultiState2v2 IniciarNuevaMano2v2(string sala, bool esPrimeraPartida, ManoTruco2v2? estadoAnterior = null)
    {
        var jugadores = OrdenarJugadoresPorEquipos(sala, porEquipo: 2);
        int numMano = esPrimeraPartida ? 1 : (estadoAnterior?.NumeroDeMano ?? 0) + 1;
        int ptsA    = esPrimeraPartida ? 0 : estadoAnterior?.PuntosEquipoA ?? 0;
        int ptsB    = esPrimeraPartida ? 0 : estadoAnterior?.PuntosEquipoB ?? 0;

        var entidades = jugadores.Take(4).Select((_, i) => new Jugador
        {
            Id        = $"J{i + 1}",
            Nombre    = $"Jugador {i + 1}",
            EsMaquina = false,
        }).ToArray();

        var mano = PartidaServicio2v2.CrearManoNueva(
            numeroDeMano: numMano,
            puntosEquipoA: ptsA,
            puntosEquipoB: ptsB,
            pos1: entidades.Length > 0 ? entidades[0] : null,
            pos2: entidades.Length > 1 ? entidades[1] : null,
            pos3: entidades.Length > 2 ? entidades[2] : null,
            pos4: entidades.Length > 3 ? entidades[3] : null);

        var state = new TrucoMultiState2v2 { Mano = mano };
        for (int i = 0; i < Math.Min(jugadores.Count, 4); i++)
            state.Posiciones[jugadores[i]] = i + 1;
        state.JugadoresIds = jugadores.Take(4).ToArray();

        return state;
    }

    public TrucoMultiState3v3 IniciarNuevaMano3v3(string sala, bool esPrimeraPartida, ManoTruco3v3? estadoAnterior = null)
    {
        var jugadores = OrdenarJugadoresPorEquipos(sala, porEquipo: 3);
        int numMano  = esPrimeraPartida ? 1 : (estadoAnterior?.NumeroDeMano ?? 0) + 1;
        int ptsA     = esPrimeraPartida ? 0 : estadoAnterior?.PuntosEquipoA ?? 0;
        int ptsB     = esPrimeraPartida ? 0 : estadoAnterior?.PuntosEquipoB ?? 0;
        int prevSlot = esPrimeraPartida ? -1 : estadoAnterior?.PicaPicaSlot ?? -1;

        var entidades = Enumerable.Range(1, 6).Select(i => new Jugador
        {
            Id        = $"J{i}",
            Nombre    = $"Jugador {i}",
            EsMaquina = false,
        }).ToArray();

        var mano = PartidaServicio3v3.CrearProximaMano(
            numMano, ptsA, ptsB, prevSlot,
            entidades[0], entidades[1], entidades[2],
            entidades[3], entidades[4], entidades[5]);

        var state = new TrucoMultiState3v3 { Mano = mano };
        for (int i = 0; i < Math.Min(jugadores.Count, 6); i++)
            state.Posiciones[jugadores[i]] = i + 1;
        state.JugadoresIds = jugadores.Take(6).ToArray();

        return state;
    }
}
