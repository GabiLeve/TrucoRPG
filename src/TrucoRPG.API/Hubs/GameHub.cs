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
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> _listos = new();

    // ─── 2v2 ─────────────────────────────────────────────────────
    private static readonly ConcurrentDictionary<string, string> _salasModo = new();
    // sala -> connectionId -> "sanMartin" | "belgrano"
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _equiposJugadores = new();

    // ─────────────────────────────────────────────────────────────
    //  SALA — crear / unirse
    // ─────────────────────────────────────────────────────────────
    public async Task<string> CrearSala(string modo = "1v1")
    {
        var codigo = Guid.NewGuid().ToString("N")[..6].ToUpper();
        _salas[codigo] = new List<string> { Context.ConnectionId };
        _salasModo[codigo] = modo;
        _conexionASala[Context.ConnectionId] = codigo;
        await Groups.AddToGroupAsync(Context.ConnectionId, codigo);
        return codigo;
    }

    public async Task<bool> UnirseASala(string codigo)
    {
        if (!_salas.TryGetValue(codigo, out var jugadores)) return false;

        var modo = _salasModo.TryGetValue(codigo, out var m) ? m : "1v1";
        int maxJugadores = modo == "2v2" ? 4 : 2;

        if (jugadores.Count >= maxJugadores) return false;

        jugadores.Add(Context.ConnectionId);
        _conexionASala[Context.ConnectionId] = codigo;
        await Groups.AddToGroupAsync(Context.ConnectionId, codigo);

        if (modo == "1v1")
        {
            if (jugadores.Count == 2)
                await Clients.Group(codigo).SendAsync("SalaLista");
        }
        else // 2v2
        {
            await Clients.Group(codigo).SendAsync("LobbyActualizado", new
            {
                jugadoresEnSala = jugadores.Count,
                maxJugadores = 4
            });
            if (jugadores.Count == 4)
                await Clients.Group(codigo).SendAsync("SalaCompleta");
        }

        return true;
    }

    // ─────────────────────────────────────────────────────────────
    //  EQUIPOS — selección 2v2
    // ─────────────────────────────────────────────────────────────
    public async Task ElegirEquipo(string equipo)
    {
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out var sala)) return;
        if (!_salas.TryGetValue(sala, out var jugadores)) return;
        if (equipo != "sanMartin" && equipo != "belgrano") return;

        var equiposMap = _equiposJugadores.GetOrAdd(sala, _ => new ConcurrentDictionary<string, string>());

        // Verificar si el equipo destino tiene lugar
        equiposMap.TryGetValue(Context.ConnectionId, out var equipoActual);
        int countEnEquipo = equiposMap.Values.Count(v => v == equipo);

        // Si ya está en ese equipo, no hacer nada
        if (equipoActual == equipo) return;

        // Si el equipo destino está lleno (2 jugadores), rechazar
        if (countEnEquipo >= 2) return;

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
        int requiredPlayers = modo == "2v2" ? 4 : 2;

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

            if (_trucoGames.TryGetValue(sala, out var existing))
            {
                await BroadcastTrucoEstado(sala, existing);
                return;
            }

            // Por ahora la lógica de juego usa 2 jugadores; 2v2 se implementa en la próxima iteración
            var state = new TrucoMultiState
            {
                Jugador1Id = jugadores[0],
                Jugador2Id = jugadores[1],
            };
            IniciarNuevaMano(state, esPrimeraPartida: true);
            _trucoGames[sala] = state;
            await BroadcastTrucoEstado(sala, state);
        }
    }

    public async Task IniciarTruco()
    {
        if (!_conexionASala.TryGetValue(Context.ConnectionId, out var sala)) return;
        if (!_salas.TryGetValue(sala, out var jugadores) || jugadores.Count < 2) return;

        var state = new TrucoMultiState
        {
            Jugador1Id = jugadores[0],
            Jugador2Id = jugadores[1],
        };
        IniciarNuevaMano(state, esPrimeraPartida: true);
        _trucoGames[sala] = state;
        await BroadcastTrucoEstado(sala, state);
    }

    public async Task JugarCarta(int numero, string palo)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        var mano = state!.Mano;
        if (mano.GanadorMano != null || mano.PartidaTerminada) return;
        if (mano.EnvidoPendienteRespuestaHumano || state.EnvidoPendienteRespuestaJ2) return;
        if (mano.TrucoPendienteRespuestaHumano || state.TrucoPendienteRespuestaJ2) return;

        bool esJ1  = Context.ConnectionId == state.Jugador1Id;
        string rol = esJ1 ? "Humano" : "Maquina";
        if (mano.TurnoActual != rol) return;

        var manoJugador = esJ1 ? mano.Humano.Mano : mano.Maquina.Mano;
        var carta = manoJugador.FirstOrDefault(c =>
            c.Numero == numero && c.Palo.Equals(palo, StringComparison.OrdinalIgnoreCase));
        if (carta == null) return;

        manoJugador.Remove(carta);
        (esJ1 ? mano.Humano.Jugadas : mano.Maquina.Jugadas).Add(carta);

        if (esJ1)
        {
            if (mano.CartaMaquinaEnMesa != null)
            {
                var cartaJ2 = mano.CartaMaquinaEnMesa;
                mano.CartaMaquinaEnMesa = null;
                ResolverBazaMulti(mano, carta, cartaJ2);
            }
            else
            {
                state.CartaPendienteJ1 = carta;
                mano.TurnoActual = "Maquina";
            }
        }
        else
        {
            if (state.CartaPendienteJ1 != null)
            {
                var cartaJ1 = state.CartaPendienteJ1;
                state.CartaPendienteJ1 = null;
                ResolverBazaMulti(mano, cartaJ1, carta);
            }
            else
            {
                mano.CartaMaquinaEnMesa = carta;
                mano.TurnoActual = "Humano";
            }
        }

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task SolicitarEnvido(string tipo)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        var mano = state!.Mano;
        if (mano.EnvidoCantado || mano.EnvidoResuelto) return;
        if (mano.Bazas.Count > 0) return;
        if (mano.PartidaTerminada || mano.GanadorMano != null) return;

        bool esJ1 = Context.ConnectionId == state.Jugador1Id;
        mano.EnvidoCantado     = true;
        mano.CantorEnvido      = esJ1 ? "Humano" : "Maquina";
        mano.TipoEnvidoCantado = EnvidoServicio.NormalizarTipo(tipo);

        if (esJ1)
        {
            state.EnvidoPendienteRespuestaJ2 = true;
            mano.EstadoEnvido = $"Jugador 1 cantó {tipo}.";
        }
        else
        {
            mano.EnvidoPendienteRespuestaHumano = true;
            mano.EstadoEnvido = $"Jugador 2 cantó {tipo}.";
        }

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task ResponderEnvido(bool aceptar)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        var mano = state!.Mano;
        if (!mano.EnvidoCantado || mano.EnvidoResuelto) return;

        bool esJ1 = Context.ConnectionId == state.Jugador1Id;
        if (esJ1 && !mano.EnvidoPendienteRespuestaHumano) return;
        if (!esJ1 && !state.EnvidoPendienteRespuestaJ2) return;

        mano.EnvidoPendienteRespuestaHumano = false;
        state.EnvidoPendienteRespuestaJ2    = false;

        if (!aceptar)
        {
            mano.EnvidoResuelto = true;
            mano.GanadorEnvido  = mano.CantorEnvido;
            mano.PuntosEnvido   = 1;
            string ganNombre    = mano.CantorEnvido == "Humano" ? "Jugador 1" : "Jugador 2";
            mano.EstadoEnvido   = $"No quiso. {ganNombre} gana 1 punto de envido.";
            JuegoServicio.SumarPuntos(mano, mano.CantorEnvido!, 1);
        }
        else
        {
            int pts = EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado);
            mano.TantoHumano  = EnvidoServicio.CalcularTanto(mano.Humano.Mano);
            mano.TantoMaquina = EnvidoServicio.CalcularTanto(mano.Maquina.Mano);

            if (mano.TipoEnvidoCantado == "FaltaEnvido")
            {
                int ptsGanador = mano.TantoHumano >= mano.TantoMaquina
                    ? mano.PuntosHumano : mano.PuntosMaquina;
                pts = Math.Max(30 - ptsGanador, 1);
            }

            if (mano.TantoHumano > mano.TantoMaquina)       mano.GanadorEnvido = "Humano";
            else if (mano.TantoMaquina > mano.TantoHumano)  mano.GanadorEnvido = "Maquina";
            else                                             mano.GanadorEnvido = mano.ManoIniciadaPor;

            mano.PuntosEnvido   = pts;
            mano.EnvidoResuelto = true;
            string gan          = mano.GanadorEnvido == "Humano" ? "Jugador 1" : "Jugador 2";
            mano.EstadoEnvido   = $"Quiso. J1 tiene {mano.TantoHumano}, J2 tiene {mano.TantoMaquina}. Gana {gan} ({pts} pt).";
            JuegoServicio.SumarPuntos(mano, mano.GanadorEnvido, pts);
        }

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task EscalarEnvido(string tipo)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        var mano = state!.Mano;
        if (!mano.EnvidoCantado || mano.EnvidoResuelto) return;

        bool esJ1        = Context.ConnectionId == state.Jugador1Id;
        string rolActual = esJ1 ? "Humano" : "Maquina";
        if (mano.CantorEnvido == rolActual) return;
        string tipoNuevo = EnvidoServicio.NormalizarTipo(tipo);
        if (EnvidoServicio.OrdinalTipo(tipoNuevo) <= EnvidoServicio.OrdinalTipo(mano.TipoEnvidoCantado)) return;

        mano.TipoEnvidoCantado = tipoNuevo;
        mano.CantorEnvido      = rolActual;

        mano.EnvidoPendienteRespuestaHumano = !esJ1;
        state.EnvidoPendienteRespuestaJ2    = esJ1;

        mano.EstadoEnvido = $"{(esJ1 ? "J1" : "J2")} cantó {tipo}.";

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task SolicitarTruco()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        var mano = state!.Mano;
        if (mano.TrucoCantado || mano.GanadorMano != null || mano.PartidaTerminada) return;

        bool esJ1 = Context.ConnectionId == state.Jugador1Id;
        mano.TrucoCantado    = true;
        mano.NivelTruco      = 1;
        mano.PuntosTrucoMano = 2;
        mano.CantorTruco     = esJ1 ? "Humano" : "Maquina";

        if (esJ1)
        {
            state.TrucoPendienteRespuestaJ2 = true;
            mano.EstadoTruco = "Jugador 1 cantó Truco.";
        }
        else
        {
            mano.TrucoPendienteRespuestaHumano = true;
            mano.EstadoTruco = "Jugador 2 cantó Truco.";
        }

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task ResponderTruco(bool aceptar, string? escalarA)
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        var mano = state!.Mano;

        bool esJ1 = Context.ConnectionId == state.Jugador1Id;
        if (esJ1 && !mano.TrucoPendienteRespuestaHumano) return;
        if (!esJ1 && !state.TrucoPendienteRespuestaJ2) return;

        mano.TrucoPendienteRespuestaHumano = false;
        state.TrucoPendienteRespuestaJ2    = false;

        if (!aceptar)
        {
            int ptsRefusal       = mano.NivelTruco;
            mano.TrucoResuelto   = true;
            mano.GanadorMano     = mano.CantorTruco;
            mano.PuntosTrucoMano = ptsRefusal;
            string gan           = mano.CantorTruco == "Humano" ? "Jugador 1" : "Jugador 2";
            mano.EstadoTruco     = $"No quiso. {gan} gana {ptsRefusal} pt.";
            JuegoServicio.SumarPuntos(mano, mano.CantorTruco!, ptsRefusal);
        }
        else
        {
            var escalar        = escalarA?.Trim().ToLowerInvariant();
            string respondedor = esJ1 ? "Humano" : "Maquina";

            if (!string.IsNullOrEmpty(escalar) && mano.NivelTruco < 3)
            {
                mano.NivelTruco++;
                mano.PuntosTrucoMano = mano.NivelTruco == 2 ? 3 : 4;
                mano.CantorTruco     = respondedor;
                string nombreNivel   = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
                mano.EstadoTruco     = $"Quiso y cantó {nombreNivel}! Vale {mano.PuntosTrucoMano} pt.";
                if (esJ1) state.TrucoPendienteRespuestaJ2    = true;
                else      mano.TrucoPendienteRespuestaHumano = true;
            }
            else
            {
                mano.TrucoResuelto = true;
                mano.EstadoTruco   = $"Quiso. Vale {mano.PuntosTrucoMano} pt.";
            }
        }

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task EscalarTruco()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        var mano = state!.Mano;
        if (!mano.TrucoCantado || mano.NivelTruco >= 3) return;
        if (mano.TrucoPendienteRespuestaHumano || state.TrucoPendienteRespuestaJ2) return;
        if (mano.GanadorMano != null || mano.PartidaTerminada) return;

        bool esJ1        = Context.ConnectionId == state.Jugador1Id;
        string rolActual = esJ1 ? "Humano" : "Maquina";
        if (mano.CantorTruco == rolActual) return;

        mano.NivelTruco++;
        mano.TrucoResuelto   = false;
        mano.CantorTruco     = rolActual;
        mano.PuntosTrucoMano = mano.NivelTruco == 2 ? 3 : 4;
        string nombre        = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
        mano.EstadoTruco     = $"{(esJ1 ? "J1" : "J2")} cantó {nombre}!";

        if (esJ1) state.TrucoPendienteRespuestaJ2    = true;
        else      mano.TrucoPendienteRespuestaHumano = true;

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task IrseAlMazo()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        var mano = state!.Mano;
        if (mano.GanadorMano != null || mano.PartidaTerminada) return;

        bool esJ1      = Context.ConnectionId == state.Jugador1Id;
        string ganador = esJ1 ? "Maquina" : "Humano";
        int pts        = mano.TrucoCantado && !mano.TrucoResuelto ? mano.PuntosTrucoMano : 1;

        mano.GanadorMano   = ganador;
        mano.TrucoResuelto = true;
        mano.EstadoTruco   = $"{(esJ1 ? "J1" : "J2")} se fue al mazo. {(ganador == "Humano" ? "J1" : "J2")} gana {pts} pt.";
        JuegoServicio.SumarPuntos(mano, ganador, pts);

        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task NuevaMano()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        if (state!.Mano.GanadorMano == null && !state.Mano.PartidaTerminada) return;
        IniciarNuevaMano(state, esPrimeraPartida: false);
        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
    }

    public async Task NuevaPartida()
    {
        if (!ObtenerSalaYEstado(out var sala, out var state)) return;
        IniciarNuevaMano(state, esPrimeraPartida: true);
        _trucoGames[sala!] = state;
        await BroadcastTrucoEstado(sala!, state);
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
                jugadores.Remove(Context.ConnectionId);
                if (jugadores.Count == 0)
                {
                    _salas.TryRemove(sala, out _);
                    _salasModo.TryRemove(sala, out _);
                    _equiposJugadores.TryRemove(sala, out _);
                }
            }
            _trucoGames.TryRemove(sala, out _);
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

    private static void IniciarNuevaMano(TrucoMultiState state, bool esPrimeraPartida)
    {
        int numMano = esPrimeraPartida ? 1 : state.Mano.NumeroDeMano + 1;
        int ptsH    = esPrimeraPartida ? 0 : state.Mano.PuntosHumano;
        int ptsM    = esPrimeraPartida ? 0 : state.Mano.PuntosMaquina;

        var mano = PartidaServicio.CrearManoNueva(numMano, ptsH, ptsM);
        mano.Humano.Nombre  = "Jugador 1";
        mano.Maquina.Nombre = "Jugador 2";
        mano.PuntosTrucoMano = 1;

        state.Mano                       = mano;
        state.CartaPendienteJ1           = null;
        state.TrucoPendienteRespuestaJ2  = false;
        state.EnvidoPendienteRespuestaJ2 = false;
    }

    private static void ResolverBazaMulti(ManoTruco mano, Carta cartaJ1, Carta cartaJ2)
    {
        var ganador = JuegoServicio.ResolverBaza(cartaJ1, cartaJ2);
        mano.Bazas.Add(new Baza { CartaJugador = cartaJ1, CartaMaquina = cartaJ2, Ganador = ganador });
        mano.TurnoActual = ganador == "Parda" ? mano.ManoIniciadaPor : ganador;

        var ganadorMano = JuegoServicio.ResolverGanadorMano(mano.Bazas, mano.ManoIniciadaPor);
        if (ganadorMano != null)
        {
            mano.GanadorMano   = ganadorMano;
            int pts            = mano.PuntosTrucoMano > 0 ? mano.PuntosTrucoMano : 1;
            JuegoServicio.SumarPuntos(mano, ganadorMano, pts);
            mano.TrucoResuelto = true;
        }
    }

    private async Task BroadcastEstadoEquipos(string sala, List<string> jugadores, ConcurrentDictionary<string, string> equiposMap)
    {
        int countSanMartin = equiposMap.Values.Count(v => v == "sanMartin");
        int countBelgrano  = equiposMap.Values.Count(v => v == "belgrano");
        bool equiposListos = countSanMartin == 2 && countBelgrano == 2;

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
}
