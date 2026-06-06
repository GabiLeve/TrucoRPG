using Microsoft.AspNetCore.Mvc;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.API.Controllers
{
    // ── Request models ────────────────────────────────────────────
    public record Truco2v2Request(Guid ManoId);
    public record Truco2v2CartaRequest(Guid ManoId, int Numero, string Palo);
    public record Truco2v2EnvidoRequest(Guid ManoId, string Tipo);
    public record Truco2v2ResponderEnvidoRequest(Guid ManoId, bool Aceptar, string? EscalarA = null);
    public record Truco2v2TantoRequest(Guid ManoId, int Tanto);
    public record Truco2v2ResponderTrucoRequest(Guid ManoId, bool Aceptar, string? EscalarA = null);
    public record Truco2v2NuevaPartidaRequest(int? NumeroDeMano = null, int? PuntosA = null, int? PuntosB = null);
    public record Truco2v2ConsultaEnvidoRequest(Guid ManoId, bool Aceptar);
    public record Truco2v2ConsultaTrucoRequest(Guid ManoId, bool Voy);

    // Evento de una acción de máquina, para mostrar diálogos en el front.
    // Tipo: "carta" | "truco" | "envido" | "truco-resp" | "envido-resp" | "tanto"
    public record EventoMaquina(string Jugador, string Tipo, string Texto);
    public record Truco2v2PasoResponse(ManoTruco2v2 Mano, EventoMaquina? Evento);

    [ApiController]
    [Route("api/[controller]")]
    public class Truco2v2Controller : ControllerBase
    {
        // ── IDs fijos de jugadores ─────────────────────────────────
        private const string J1 = "J1"; // humano
        private const string J2 = "J2"; // rival 1
        private const string J3 = "J3"; // compañero
        private const string J4 = "J4"; // rival 2
        private static readonly Random _rng = new Random();

        // ─────────────────────────────────────────────────────────
        //  Nueva partida
        // ─────────────────────────────────────────────────────────
        [HttpPost("nueva-partida")]
        public ActionResult<ManoTruco2v2> NuevaPartida([FromBody] Truco2v2NuevaPartidaRequest? req)
        {
            int num   = req?.NumeroDeMano  ?? 1;
            int ptsA  = req?.PuntosA       ?? 0;
            int ptsB  = req?.PuntosB       ?? 0;

            var mano = PartidaServicio2v2.CrearManoNueva(
                numeroDeMano:  num,
                puntosEquipoA: ptsA,
                puntosEquipoB: ptsB,
                pos1: new Jugador { Id = J1, Nombre = "Vos",       EsMaquina = false },
                pos2: new Jugador { Id = J2, Nombre = "Rival 1",   EsMaquina = true  },
                pos3: new Jugador { Id = J3, Nombre = "Compañero", EsMaquina = true  },
                pos4: new Jugador { Id = J4, Nombre = "Rival 2",   EsMaquina = true  }
            );

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Guardar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Jugar carta
        // ─────────────────────────────────────────────────────────
        [HttpPost("jugar-carta")]
        public ActionResult<ManoTruco2v2> JugarCarta([FromBody] Truco2v2CartaRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            ValidarTurnoHumano(mano);

            var jugador = mano.ObtenerJugador(J1)!;
            var carta = jugador.Mano.FirstOrDefault(c =>
                c.Numero == req.Numero &&
                c.Palo.Equals(req.Palo, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException("Carta no encontrada en tu mano.");

            JuegoServicio2v2.JugarCarta(mano, J1, carta);
            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Envido — cantar
        // ─────────────────────────────────────────────────────────
        [HttpPost("cantar-envido")]
        public ActionResult<ManoTruco2v2> CantarEnvido([FromBody] Truco2v2EnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (mano.EnvidoCantado || mano.EnvidoResuelto)
                throw new InvalidOperationException("El envido ya fue cantado o resuelto.");
            // El envido se puede cantar durante toda la primera vuelta (aún con cartas en mesa).
            if (mano.Vueltas.Count > 0)
                throw new InvalidOperationException("El envido solo se puede cantar en la primera vuelta.");
            // Si el truco ya fue cantado y resuelto, la ventana del envido se cerró.
            if (mano.TrucoCantado && mano.TrucoPendienteRespuestaDe == null)
                throw new InvalidOperationException("El envido ya no se puede cantar: el truco fue resuelto.");
            // "El envido va primero": se permite también si te deben una respuesta de truco.
            bool esTuTurno  = mano.TurnoActual == J1;
            bool debesTruco = mano.TrucoPendienteRespuestaDe == J1;
            if (!esTuTurno && !debesTruco)
                throw new InvalidOperationException("No es tu turno.");

            var tipo = EnvidoServicio.NormalizarTipo(req.Tipo);
            mano.EnvidoCantado     = true;
            mano.CantorEnvido      = J1;
            mano.TipoEnvidoCantado = tipo;
            mano.PuntosEnvido      = EnvidoServicio2v2.ObtenerPuntosEnJuego(tipo);
            mano.FaseEnvido        = "pendiente_respuesta";
            mano.EstadoEnvido      = $"Cantaste {req.Tipo}.";

            // Primer jugador del equipo contrario (EquipoB) responde
            mano.EnvidoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, "EquipoA");

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Envido — responder (quiero / no quiero / escalar)
        // ─────────────────────────────────────────────────────────
        [HttpPost("responder-envido")]
        public ActionResult<ManoTruco2v2> ResponderEnvido([FromBody] Truco2v2ResponderEnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!mano.EnvidoCantado || mano.EnvidoResuelto)
                throw new InvalidOperationException("No hay envido pendiente de respuesta.");
            if (mano.EnvidoPendienteRespuestaDe != J1)
                throw new InvalidOperationException("No sos vos quien debe responder el envido.");

            // Escalación
            var escalar = req.EscalarA?.Trim().ToLowerInvariant();
            if (req.Aceptar && !string.IsNullOrEmpty(escalar))
            {
                var tipoActual = (mano.TipoEnvidoCantado ?? "Envido").ToLowerInvariant();
                bool valida = escalar switch
                {
                    "envido" or "envido envido" or "envidoenvido" => tipoActual == "envido",
                    "real envido"  => tipoActual is "envido" or "envidoenvido" or "envido envido",
                    "falta envido" => tipoActual is "envido" or "envidoenvido" or "envido envido" or "realenvido" or "real envido",
                    _ => false
                };
                if (!valida)
                    throw new InvalidOperationException($"No podés escalar a '{req.EscalarA}' desde '{mano.TipoEnvidoCantado}'.");

                int ptsAntes = mano.PuntosEnvido;
                mano.TipoEnvidoCantado = EnvidoServicio.NormalizarTipo(escalar);
                mano.PuntosEnvido      = EnvidoServicio2v2.ObtenerPuntosEnJuego(mano.TipoEnvidoCantado);
                mano.CantorEnvido      = J1;
                mano.EstadoEnvido      = $"Cantaste {req.EscalarA}.";
                mano.EnvidoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, "EquipoA");

                // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
                Truco2v2MemoriaServicio.Actualizar(mano);
                return Ok(mano);
            }

            mano.EnvidoPendienteRespuestaDe = null;
            if (!req.Aceptar)
            {
                EnvidoServicio2v2.ResolverNoQuiero(mano);
            }
            else
            {
                mano.FaseEnvido = "aceptado";
                EnvidoServicio2v2.IniciarDeclaracionTantos(mano);
            }

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Envido — declarar tanto
        // ─────────────────────────────────────────────────────────
        [HttpPost("declarar-tanto")]
        public ActionResult<ManoTruco2v2> DeclararTanto([FromBody] Truco2v2TantoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (mano.FaseEnvido != "declarando_tantos")
                throw new InvalidOperationException("No estás en la fase de declaración de tantos.");
            if (mano.EnvidoPendienteRespuestaDe != J1)
                throw new InvalidOperationException("No sos vos quien debe declarar el tanto ahora.");

            EnvidoServicio2v2.ProcesarDeclaracion(mano, J1, req.Tanto, sonBuenas: false);
            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Envido — son buenas
        // ─────────────────────────────────────────────────────────
        [HttpPost("son-buenas")]
        public ActionResult<ManoTruco2v2> SonBuenas([FromBody] Truco2v2Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (mano.FaseEnvido != "declarando_tantos")
                throw new InvalidOperationException("'Son buenas' solo se puede decir durante la declaración de tantos.");
            if (mano.EnvidoPendienteRespuestaDe != J1)
                throw new InvalidOperationException("No sos vos quien debe declarar el tanto ahora.");

            EnvidoServicio2v2.ProcesarDeclaracion(mano, J1, null, sonBuenas: true);
            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Truco — cantar
        // ─────────────────────────────────────────────────────────
        [HttpPost("cantar-truco")]
        public ActionResult<ManoTruco2v2> CantarTruco([FromBody] Truco2v2Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (mano.TrucoCantado)
                throw new InvalidOperationException("El truco ya fue cantado.");
            if (mano.GanadorMano != null || mano.ManoTerminada || mano.PartidaTerminada)
                throw new InvalidOperationException("La mano ya terminó.");
            if (mano.TurnoActual != J1)
                throw new InvalidOperationException("No es tu turno.");

            mano.TrucoCantado      = true;
            mano.CantorTruco       = J1;
            mano.EquipoCantorTruco = "EquipoA";
            mano.NivelTruco        = 1;
            mano.PuntosTrucoMano   = 2;
            mano.EstadoTruco       = "Cantaste Truco.";
            mano.TrucoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, "EquipoA");
            mano.PuedeEscalarTruco = TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoA");

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Truco — responder (quiero / no quiero / escalar)
        // ─────────────────────────────────────────────────────────
        [HttpPost("responder-truco")]
        public ActionResult<ManoTruco2v2> ResponderTruco([FromBody] Truco2v2ResponderTrucoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!mano.TrucoCantado || mano.TrucoResuelto)
                throw new InvalidOperationException("No hay truco pendiente de respuesta.");
            if (mano.TrucoPendienteRespuestaDe != J1)
                throw new InvalidOperationException("No sos vos quien debe responder el truco.");

            mano.TrucoPendienteRespuestaDe = null;

            if (!req.Aceptar)
            {
                int pts = mano.NivelTruco;
                mano.TrucoResuelto   = true;
                mano.GanadorMano     = mano.EquipoCantorTruco;
                mano.ManoTerminada   = true;
                mano.PuntosTrucoMano = pts;
                mano.EstadoTruco     = $"No quisiste truco. {mano.EquipoCantorTruco} gana {pts} pt.";
                JuegoServicio2v2.SumarPuntos(mano, mano.EquipoCantorTruco!, pts);
            }
            else
            {
                var escalar = req.EscalarA?.Trim().ToLowerInvariant();
                if (!string.IsNullOrEmpty(escalar) && mano.NivelTruco < 3 &&
                    TurnoServicio2v2.PuedeEscalarTruco(mano, J1))
                {
                    mano.NivelTruco++;
                    mano.PuntosTrucoMano   = mano.NivelTruco == 2 ? 3 : 4;
                    mano.EquipoCantorTruco = "EquipoA";
                    mano.CantorTruco       = J1;
                    string nombre          = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
                    mano.EstadoTruco       = $"Quisiste y cantaste {nombre}! Vale {mano.PuntosTrucoMano} pt.";
                    mano.TrucoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, "EquipoA");
                }
                else
                {
                    mano.TrucoResuelto = true;
                    mano.EstadoTruco   = $"Quisiste el truco. Vale {mano.PuntosTrucoMano} pt.";
                }
            }

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Irse al mazo
        // ─────────────────────────────────────────────────────────
        [HttpPost("irse-al-mazo")]
        public ActionResult<ManoTruco2v2> IrseAlMazo([FromBody] Truco2v2Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (mano.GanadorMano != null || mano.ManoTerminada || mano.PartidaTerminada)
                throw new InvalidOperationException("La mano ya terminó.");
            if (mano.TurnoActual != J1)
                throw new InvalidOperationException("No es tu turno.");

            int pts = mano.TrucoCantado && !mano.TrucoResuelto ? mano.PuntosTrucoMano : 1;
            mano.GanadorMano     = "EquipoB";
            mano.ManoTerminada   = true;
            mano.TrucoResuelto   = true;
            mano.EstadoTruco     = $"Te fuiste al mazo. EquipoB gana {pts} pt.";
            JuegoServicio2v2.SumarPuntos(mano, "EquipoB", pts);

            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Nueva mano
        // ─────────────────────────────────────────────────────────
        [HttpPost("nueva-mano")]
        public ActionResult<ManoTruco2v2> NuevaMano([FromBody] Truco2v2Request req)
        {
            var anterior = ObtenerMano(req.ManoId);
            if (anterior.GanadorMano == null && !anterior.PartidaTerminada)
                throw new InvalidOperationException("La mano actual aún no terminó.");
            if (anterior.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó. Iniciá una nueva partida.");

            var nueva = PartidaServicio2v2.CrearManoNueva(
                numeroDeMano:  anterior.NumeroDeMano + 1,
                puntosEquipoA: anterior.PuntosEquipoA,
                puntosEquipoB: anterior.PuntosEquipoB,
                pos1: new Jugador { Id = J1, Nombre = "Vos",       EsMaquina = false },
                pos2: new Jugador { Id = J2, Nombre = "Rival 1",   EsMaquina = true  },
                pos3: new Jugador { Id = J3, Nombre = "Compañero", EsMaquina = true  },
                pos4: new Jugador { Id = J4, Nombre = "Rival 2",   EsMaquina = true  }
            );

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Guardar(nueva);
            return Ok(nueva);
        }

        // ─────────────────────────────────────────────────────────
        //  Compañero pregunta: ¿canto los tantos?
        // ─────────────────────────────────────────────────────────
        [HttpPost("responder-consulta-envido")]
        public ActionResult<ManoTruco2v2> ResponderConsultaEnvido([FromBody] Truco2v2ConsultaEnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!mano.CompaConsultaEnvido)
                throw new InvalidOperationException("Tu compañero no está preguntando por el envido.");

            mano.CompaConsultaEnvido   = false;
            mano.CompaEnvidoConsultado = true;

            if (req.Aceptar)
            {
                // El compañero (J3) canta el envido.
                mano.EnvidoCantado     = true;
                mano.CantorEnvido      = J3;
                mano.TipoEnvidoCantado = "Envido";
                mano.PuntosEnvido      = EnvidoServicio2v2.ObtenerPuntosEnJuego("Envido");
                mano.FaseEnvido        = "pendiente_respuesta";
                mano.EstadoEnvido      = "Tu compañero cantó Envido.";
                mano.EnvidoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, "EquipoA");
            }

            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Compañero pregunta: ¿voy o pongo? (truco)
        // ─────────────────────────────────────────────────────────
        [HttpPost("responder-consulta-truco")]
        public ActionResult<ManoTruco2v2> ResponderConsultaTruco([FromBody] Truco2v2ConsultaTrucoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!mano.CompaConsultaTruco)
                throw new InvalidOperationException("Tu compañero no está preguntando por el truco.");

            mano.CompaConsultaTruco   = false;
            mano.CompaTrucoConsultado = true;

            if (req.Voy)
            {
                // "Voy": el compañero (J3) canta truco.
                mano.TrucoCantado      = true;
                mano.CantorTruco       = J3;
                mano.EquipoCantorTruco = mano.ObtenerEquipoDeJugador(J3);
                mano.NivelTruco        = 1;
                mano.PuntosTrucoMano   = 2;
                mano.TrucoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, mano.EquipoCantorTruco!);
                mano.EstadoTruco       = "Tu compañero cantó Truco.";
            }
            else
            {
                // "Pongo": el compañero juega su carta más alta.
                var jugador = mano.ObtenerJugador(J3);
                if (jugador != null && jugador.Mano.Count > 0)
                {
                    var alta = jugador.Mano.OrderByDescending(c => c.ValorTruco).First();
                    JuegoServicio2v2.JugarCarta(mano, J3, alta);
                }
            }

            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Helpers privados
        // ─────────────────────────────────────────────────────────
        private static ManoTruco2v2 ObtenerMano(Guid id) =>
            Truco2v2MemoriaServicio.Obtener(id)
            ?? throw new KeyNotFoundException($"No se encontró la mano {id}.");

        private static void ValidarTurnoHumano(ManoTruco2v2 mano)
        {
            if (mano.PartidaTerminada || mano.ManoTerminada)
                throw new InvalidOperationException("La mano/partida ya terminó.");
            if (mano.TrucoPendienteRespuestaDe == J1)
                throw new InvalidOperationException("Debés responder el truco antes de jugar.");
            if (mano.EnvidoPendienteRespuestaDe == J1)
                throw new InvalidOperationException("Debés responder el envido antes de jugar.");
            if (mano.TurnoActual != J1)
                throw new InvalidOperationException("No es tu turno.");
        }

        // ─────────────────────────────────────────────────────────
        //  Avanzar UNA sola acción de máquina (para delay/diálogos en el front)
        // ─────────────────────────────────────────────────────────
        [HttpPost("avanzar-maquina")]
        public ActionResult<Truco2v2PasoResponse> AvanzarMaquina([FromBody] Truco2v2Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            var evento = AvanzarUnPaso(mano);
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(new Truco2v2PasoResponse(mano, evento));
        }

        /// <summary>
        /// Ejecuta exactamente UNA acción del próximo jugador máquina y devuelve
        /// un evento describiendo qué hizo (para mostrar el diálogo). Devuelve null
        /// si no hay máquina por actuar (turno del humano o mano/partida terminada).
        /// </summary>
        private static EventoMaquina? AvanzarUnPaso(ManoTruco2v2 mano)
        {
            if (mano.PartidaTerminada || mano.ManoTerminada || mano.GanadorMano != null) return null;

            string? actor = ProximoActor(mano);
            if (actor == null || actor == J1) return null;

            var jugador = mano.ObtenerJugador(actor);
            if (jugador == null || !jugador.EsMaquina) return null;

            // ── Responder truco ───────────────────────────────────────
            if (mano.TrucoPendienteRespuestaDe == actor)
            {
                MaquinaServicio2v2.ResponderTruco(mano, actor);
                bool noQuiso = (mano.EstadoTruco ?? "").Contains("no quiso");
                return new EventoMaquina(actor, "truco-resp", noQuiso ? "¡No quiero!" : "¡Quiero!");
            }

            // ── Responder envido ──────────────────────────────────────
            if (mano.FaseEnvido == "pendiente_respuesta" && mano.EnvidoPendienteRespuestaDe == actor)
            {
                MaquinaServicio2v2.ResponderEnvido(mano, actor);
                bool quiso = mano.FaseEnvido == "declarando_tantos" || mano.FaseEnvido == "aceptado";
                return new EventoMaquina(actor, "envido-resp", quiso ? "¡Quiero!" : "¡No quiero!");
            }

            // ── Declarar tanto ────────────────────────────────────────
            if (mano.FaseEnvido == "declarando_tantos" && mano.EnvidoPendienteRespuestaDe == actor)
            {
                MaquinaServicio2v2.DeclararTanto(mano, actor);
                if (mano.JugadorQueDijoSonBuenas == actor)
                    return new EventoMaquina(actor, "tanto", "¡Son buenas!");
                string texto = mano.TantosDeclarados.TryGetValue(actor, out var t) && t.HasValue
                    ? t.Value.ToString()
                    : "¡Son buenas!";
                return new EventoMaquina(actor, "tanto", texto);
            }

            // ── Turno normal: cantar o jugar carta ────────────────────
            if (mano.TurnoActual == actor)
            {
                // El compañero (J3) le pregunta al humano si quiere que cante los tantos.
                if (actor == J3
                    && J3 == TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoA")
                    && !mano.CompaEnvidoConsultado
                    && !mano.EnvidoCantado && !mano.EnvidoResuelto
                    && mano.Vueltas.Count == 0
                    && (!mano.TrucoCantado || mano.TrucoPendienteRespuestaDe != null)
                    && MaquinaServicio2v2.DebeCantarEnvido(jugador.Mano))
                {
                    mano.CompaConsultaEnvido = true;
                    return new EventoMaquina(actor, "consulta-envido", "¿Canto los tantos?");
                }

                // El compañero (J3) pregunta antes de cantar truco: ¿voy o pongo?
                // Solo aplica si el compañero juega ANTES que vos (todavía no jugaste tu carta).
                if (actor == J3 && !mano.CompaTrucoConsultado
                    && (mano.ObtenerJugador(J1)?.Jugadas.Count ?? 1) == 0
                    && !mano.TrucoCantado && !mano.TrucoResuelto
                    && mano.TrucoPendienteRespuestaDe == null
                    && jugador.Mano.Count > 0 && jugador.Mano.Max(c => c.ValorTruco) >= 10)
                {
                    mano.CompaConsultaTruco = true;
                    return new EventoMaquina(actor, "consulta-truco", "¿Voy o pongo?");
                }

                bool envidoAntes = mano.EnvidoCantado;
                bool trucoAntes  = mano.TrucoCantado;

                MaquinaServicio2v2.ProcesarTurnoMaquina(mano, actor);

                if (!envidoAntes && mano.EnvidoCantado)
                {
                    // A veces (no siempre) el compañero te tira una pista de su envido.
                    if (mano.EnvidoPendienteRespuestaDe == J1 && _rng.Next(100) < 50)
                    {
                        var compa = mano.ObtenerJugador(J3);
                        int tantoCompa = compa != null ? EnvidoServicio2v2.TantoOriginal(compa) : 0;
                        mano.CompaPista = tantoCompa >= 28 ? "Tengo mucho"
                                        : tantoCompa >= 23 ? "Tengo algo"
                                        : "Tengo poco";
                    }
                    return new EventoMaquina(actor, "envido", "¡" + (mano.TipoEnvidoCantado ?? "Envido") + "!");
                }
                if (!trucoAntes && mano.TrucoCantado)
                    return new EventoMaquina(actor, "truco", "¡Truco!");

                return new EventoMaquina(actor, "carta", "");
            }

            return null;
        }

        /// <summary>
        /// Avanza los turnos de los jugadores máquina hasta que sea el turno
        /// del humano (J1) o la partida/mano termine.
        /// Conservado por compatibilidad; el front ahora usa avanzar-maquina.
        /// </summary>
        private static void AvanzarMaquinas(ManoTruco2v2 mano)
        {
            const int MAX_ITER = 30;

            for (int i = 0; i < MAX_ITER; i++)
            {
                if (mano.PartidaTerminada || mano.ManoTerminada || mano.GanadorMano != null) break;

                string? actor = ProximoActor(mano);
                if (actor == null || actor == J1) break;

                var jugador = mano.ObtenerJugador(actor);
                if (jugador == null || !jugador.EsMaquina) break;

                if (mano.TrucoPendienteRespuestaDe == actor)
                {
                    MaquinaServicio2v2.ResponderTruco(mano, actor);
                }
                else if ((mano.FaseEnvido == "pendiente_respuesta") && mano.EnvidoPendienteRespuestaDe == actor)
                {
                    MaquinaServicio2v2.ResponderEnvido(mano, actor);
                }
                else if (mano.FaseEnvido == "declarando_tantos" && mano.EnvidoPendienteRespuestaDe == actor)
                {
                    MaquinaServicio2v2.DeclararTanto(mano, actor);
                }
                else if (mano.TurnoActual == actor)
                {
                    MaquinaServicio2v2.ProcesarTurnoMaquina(mano, actor);
                }
                else
                {
                    break;
                }
            }
        }

        private static string? ProximoActor(ManoTruco2v2 mano)
        {
            // El envido va primero: si hay un envido pendiente de resolución (respuesta o
            // declaración de tantos), se resuelve ANTES que la respuesta al truco.
            if (mano.EnvidoPendienteRespuestaDe != null &&
                (mano.FaseEnvido == "pendiente_respuesta" || mano.FaseEnvido == "declarando_tantos"))
                return mano.EnvidoPendienteRespuestaDe;
            if (mano.TrucoPendienteRespuestaDe != null)    return mano.TrucoPendienteRespuestaDe;
            if (mano.EnvidoPendienteRespuestaDe != null)   return mano.EnvidoPendienteRespuestaDe;
            if (!mano.ManoTerminada && mano.GanadorMano == null) return mano.TurnoActual;
            return null;
        }
    }
}
