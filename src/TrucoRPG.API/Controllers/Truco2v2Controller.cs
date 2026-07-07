using Microsoft.AspNetCore.Authorization;
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
    public record Truco2v2OrdenMayorRequest(Guid ManoId, string JugadorId);

    public record Truco2v2PasoResponse(ManoTruco2v2 Mano, EventoMaquina2v2? Evento);

    /// <summary>
    /// Partida de Truco 2v2 contra la máquina (modo solo). El humano (J1) decide por su
    /// equipo y las máquinas avanzan paso a paso desde el front. Cada endpoint es una
    /// acción de juego y devuelve el estado actualizado de la mano. Requieren JWT (rol Jugador).
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Jugador")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class Truco2v2Controller : ControllerBase
    {
        // ── IDs fijos de jugadores ─────────────────────────────────
        private const string J1 = "J1"; // humano
        private const string J2 = "J2"; // rival 1
        private const string J3 = "J3"; // compañero
        private const string J4 = "J4"; // rival 2

        // En el modo solo, el humano (J1) decide quiero/no quiero por todo su equipo.
        private static readonly Func<ManoTruco2v2, string, string> Responsable =
            (m, j) => TurnoServicio2v2.ObtenerResponsableTruco(m, m.ObtenerEquipoDeJugador(j));

        // ─────────────────────────────────────────────────────────
        //  Nueva partida
        // ─────────────────────────────────────────────────────────
        /// <summary>Crea una nueva partida 2v2 (vos + compañero bot vs. dos rivales bot) y reparte la primera mano.</summary>
        /// <param name="req">Opcional: número de mano y puntos iniciales de cada equipo.</param>
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
        /// <summary>El humano (J1) juega una carta (número y palo) sobre la mesa.</summary>
        [HttpPost("jugar-carta")]
        public ActionResult<ManoTruco2v2> JugarCarta([FromBody] Truco2v2CartaRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            JuegoServicio2v2.ValidarAccionJugador(mano, J1);

            if (!JuegoServicio2v2.JugarCartaPorValor(mano, J1, req.Numero, req.Palo))
                throw new InvalidOperationException("Carta no encontrada en tu mano.");

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Envido — cantar
        // ─────────────────────────────────────────────────────────
        /// <summary>El humano canta el envido (Envido, Real Envido, Falta Envido, etc.).</summary>
        [HttpPost("cantar-envido")]
        public ActionResult<ManoTruco2v2> CantarEnvido([FromBody] Truco2v2EnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!EnvidoServicio2v2.Cantar(mano, J1, req.Tipo, Responsable))
                throw new InvalidOperationException("No se puede cantar el envido ahora.");

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Envido — responder (quiero / no quiero / escalar)
        // ─────────────────────────────────────────────────────────
        /// <summary>Responde el envido por el equipo del humano (quiero / no quiero), con opción de escalar.</summary>
        [HttpPost("responder-envido")]
        public ActionResult<ManoTruco2v2> ResponderEnvido([FromBody] Truco2v2ResponderEnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);

            var escalar = req.EscalarA?.Trim();
            bool ok = (req.Aceptar && !string.IsNullOrEmpty(escalar))
                ? EnvidoServicio2v2.Escalar(mano, J1, escalar!, Responsable)
                : EnvidoServicio2v2.Responder(mano, J1, req.Aceptar);
            if (!ok)
                throw new InvalidOperationException("No podés responder el envido ahora.");

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Envido — declarar tanto
        // ─────────────────────────────────────────────────────────
        /// <summary>Declara los tantos del humano durante la fase de declaración del envido.</summary>
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
        /// <summary>El humano dice "son buenas": reconoce que el rival tiene más tantos y no declara los suyos.</summary>
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
        /// <summary>El humano canta Truco.</summary>
        [HttpPost("cantar-truco")]
        public ActionResult<ManoTruco2v2> CantarTruco([FromBody] Truco2v2Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!TrucoServicio2v2.Cantar(mano, J1, Responsable))
                throw new InvalidOperationException("No se puede cantar el truco ahora.");

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Truco — escalar en tu turno (retruco / vale cuatro)
        //  Cuando ya aceptaste un truco, tu equipo tiene "la palabra"
        //  y puede subir la apuesta en su turno.
        // ─────────────────────────────────────────────────────────
        /// <summary>Sube la apuesta del truco (Retruco / Vale Cuatro) cuando tu equipo tiene la palabra.</summary>
        [HttpPost("escalar-truco")]
        public ActionResult<ManoTruco2v2> EscalarTruco([FromBody] Truco2v2Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!TrucoServicio2v2.Escalar(mano, J1, Responsable))
                throw new InvalidOperationException("No podés escalar el truco ahora.");

            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Truco — responder (quiero / no quiero / escalar)
        // ─────────────────────────────────────────────────────────
        /// <summary>Responde el truco por el equipo del humano (quiero / no quiero), con opción de escalar.</summary>
        [HttpPost("responder-truco")]
        public ActionResult<ManoTruco2v2> ResponderTruco([FromBody] Truco2v2ResponderTrucoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!TrucoServicio2v2.Responder(mano, J1, req.Aceptar, req.EscalarA, Responsable))
                throw new InvalidOperationException("No podés responder el truco ahora.");

            // Las máquinas avanzan paso a paso desde el front (endpoint avanzar-maquina).
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Irse al mazo
        // ─────────────────────────────────────────────────────────
        /// <summary>El humano se va al mazo y abandona la mano para su equipo.</summary>
        [HttpPost("irse-al-mazo")]
        public ActionResult<ManoTruco2v2> IrseAlMazo([FromBody] Truco2v2Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!TrucoServicio2v2.IrseAlMazo(mano, J1))
                throw new InvalidOperationException("No podés irte al mazo ahora.");

            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Nueva mano
        // ─────────────────────────────────────────────────────────
        /// <summary>Reparte una nueva mano dentro de la partida en curso (arrastra el puntaje). Falla si la mano actual no terminó o la partida ya terminó.</summary>
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
        /// <summary>Responde la consulta del compañero bot sobre cantar el envido (acepta o no).</summary>
        [HttpPost("responder-consulta-envido")]
        public ActionResult<ManoTruco2v2> ResponderConsultaEnvido([FromBody] Truco2v2ConsultaEnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            // La regla (flags + canto del compañero) vive en el dominio.
            MaquinaServicio2v2.ResolverConsultaEnvido(mano, req.Aceptar, Responsable);

            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Compañero pregunta: ¿voy o pongo? (truco)
        // ─────────────────────────────────────────────────────────
        /// <summary>Responde la consulta del compañero bot sobre el truco ("voy" carta baja / "pongo" carta alta).</summary>
        [HttpPost("responder-consulta-truco")]
        public ActionResult<ManoTruco2v2> ResponderConsultaTruco([FromBody] Truco2v2ConsultaTrucoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!mano.CompaConsultaTruco)
                throw new InvalidOperationException("Tu compañero no está preguntando por el truco.");

            // El compañero juega (voy = carta baja / pongo = carta alta); la regla vive en el dominio.
            MaquinaServicio2v2.ResolverConsultaTruco(mano, req.Voy);

            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Orden del humano a su compañero bot (botón Acciones)
        // ─────────────────────────────────────────────────────────
        /// <summary>El humano ordena a su compañero bot que juegue su carta más alta.</summary>
        [HttpPost("ordenar-mayor")]
        public ActionResult<ManoTruco2v2> OrdenarMayor([FromBody] Truco2v2OrdenMayorRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            // La regla (quién puede recibir la orden) vive en el dominio.
            MaquinaServicio2v2.OrdenarJugarMayor(mano, req.JugadorId);
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Avanzar UNA sola acción de máquina (para delay/diálogos en el front)
        // ─────────────────────────────────────────────────────────
        /// <summary>Avanza UNA sola acción de máquina y devuelve la mano más el evento (para delays/diálogos en el front).</summary>
        [HttpPost("avanzar-maquina")]
        public ActionResult<Truco2v2PasoResponse> AvanzarMaquina([FromBody] Truco2v2Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            var evento = MaquinaServicio2v2.AvanzarUnPaso(mano);
            Truco2v2MemoriaServicio.Actualizar(mano);
            return Ok(new Truco2v2PasoResponse(mano, evento));
        }

        // ─────────────────────────────────────────────────────────
        //  Helpers privados
        // ─────────────────────────────────────────────────────────
        private static ManoTruco2v2 ObtenerMano(Guid id) =>
            Truco2v2MemoriaServicio.Obtener(id)
            ?? throw new KeyNotFoundException($"No se encontró la mano {id}.");
    }
}
