using Microsoft.AspNetCore.Mvc;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.API.Controllers
{
    // ── Request models ────────────────────────────────────────────
    public record Truco3v3Request(Guid ManoId);
    public record Truco3v3CartaRequest(Guid ManoId, int Numero, string Palo);
    public record Truco3v3EnvidoRequest(Guid ManoId, string Tipo);
    public record Truco3v3ResponderEnvidoRequest(Guid ManoId, bool Aceptar, string? EscalarA = null);
    public record Truco3v3TantoRequest(Guid ManoId, int Tanto);
    public record Truco3v3ResponderTrucoRequest(Guid ManoId, bool Aceptar, string? EscalarA = null);
    public record Truco3v3NuevaPartidaRequest(int? NumeroDeMano = null, int? PuntosA = null, int? PuntosB = null);
    public record Truco3v3ConsultaEnvidoRequest(Guid ManoId, bool Aceptar);
    public record Truco3v3ConsultaTrucoRequest(Guid ManoId, bool Voy);
    public record Truco3v3OrdenMayorRequest(Guid ManoId, string JugadorId);

    public record Truco3v3PasoResponse(ManoTruco3v3 Mano, EventoMaquina3v3? Evento);

    [ApiController]
    [Route("api/[controller]")]
    public class Truco3v3Controller : ControllerBase
    {
        // ── IDs fijos de jugadores ─────────────────────────────────
        // EquipoA = J1 (humano) + J3, J5 (compañeros bot)
        // EquipoB = J2, J4, J6 (rivales bot)
        private const string J1 = "J1";

        // En el modo solo, el humano (J1) decide quiero/no quiero por todo su equipo.
        private static readonly Func<ManoTruco3v3, string, string> Responsable =
            TurnoServicio3v3.ObtenerResponsableParaJugador;

        private static Jugador[] CrearJugadores() => new[]
        {
            new Jugador { Id = "J1", Nombre = "Vos",         EsMaquina = false },
            new Jugador { Id = "J2", Nombre = "Rival 1",     EsMaquina = true  },
            new Jugador { Id = "J3", Nombre = "Compañero 1", EsMaquina = true  },
            new Jugador { Id = "J4", Nombre = "Rival 2",     EsMaquina = true  },
            new Jugador { Id = "J5", Nombre = "Compañero 2", EsMaquina = true  },
            new Jugador { Id = "J6", Nombre = "Rival 3",     EsMaquina = true  },
        };

        private static ManoTruco3v3 CrearMano(int num, int ptsA, int ptsB)
        {
            var j = CrearJugadores();
            return PartidaServicio3v3.CrearManoNueva(num, ptsA, ptsB, j[0], j[1], j[2], j[3], j[4], j[5]);
        }

        private static ManoTruco3v3 CrearProximaMano(int num, int ptsA, int ptsB, int prevSlot)
        {
            var j = CrearJugadores();
            return PartidaServicio3v3.CrearProximaMano(num, ptsA, ptsB, prevSlot, j[0], j[1], j[2], j[3], j[4], j[5]);
        }

        // ─────────────────────────────────────────────────────────
        [HttpPost("nueva-partida")]
        public ActionResult<ManoTruco3v3> NuevaPartida([FromBody] Truco3v3NuevaPartidaRequest? req)
        {
            var mano = CrearMano(req?.NumeroDeMano ?? 1, req?.PuntosA ?? 0, req?.PuntosB ?? 0);
            Truco3v3MemoriaServicio.Guardar(mano);
            return Ok(mano);
        }

        [HttpPost("jugar-carta")]
        public ActionResult<ManoTruco3v3> JugarCarta([FromBody] Truco3v3CartaRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            JuegoServicio3v3.ValidarAccionJugador(mano, J1);

            if (!JuegoServicio3v3.JugarCartaPorValor(mano, J1, req.Numero, req.Palo))
                throw new InvalidOperationException("Carta no encontrada en tu mano.");

            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("cantar-envido")]
        public ActionResult<ManoTruco3v3> CantarEnvido([FromBody] Truco3v3EnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!EnvidoServicio3v3.Cantar(mano, J1, req.Tipo, Responsable))
                throw new InvalidOperationException("No se puede cantar el envido ahora.");
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("responder-envido")]
        public ActionResult<ManoTruco3v3> ResponderEnvido([FromBody] Truco3v3ResponderEnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            var escalar = req.EscalarA?.Trim();
            bool ok = (req.Aceptar && !string.IsNullOrEmpty(escalar))
                ? EnvidoServicio3v3.Escalar(mano, J1, escalar!, Responsable)
                : EnvidoServicio3v3.Responder(mano, J1, req.Aceptar);
            if (!ok)
                throw new InvalidOperationException("No podés responder el envido ahora.");
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("declarar-tanto")]
        public ActionResult<ManoTruco3v3> DeclararTanto([FromBody] Truco3v3TantoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            // Validamos pre-condiciones explícitamente: ProcesarDeclaracion devuelve false tanto
            // cuando falla la validación COMO cuando J1 declara OK pero todavía quedan rivales.
            if (mano.FaseEnvido != "declarando_tantos" || mano.EnvidoPendienteRespuestaDe != J1)
                throw new InvalidOperationException("No podés declarar el tanto ahora.");
            EnvidoServicio3v3.ProcesarDeclaracion(mano, J1, req.Tanto, sonBuenas: false);
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("son-buenas")]
        public ActionResult<ManoTruco3v3> SonBuenas([FromBody] Truco3v3Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (mano.FaseEnvido != "declarando_tantos" || mano.EnvidoPendienteRespuestaDe != J1)
                throw new InvalidOperationException("No podés decir 'son buenas' ahora.");
            EnvidoServicio3v3.ProcesarDeclaracion(mano, J1, null, sonBuenas: true);
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("cantar-truco")]
        public ActionResult<ManoTruco3v3> CantarTruco([FromBody] Truco3v3Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!TrucoServicio3v3.Cantar(mano, J1, Responsable))
                throw new InvalidOperationException("No se puede cantar el truco ahora.");
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("escalar-truco")]
        public ActionResult<ManoTruco3v3> EscalarTruco([FromBody] Truco3v3Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!TrucoServicio3v3.Escalar(mano, J1, Responsable))
                throw new InvalidOperationException("No podés escalar el truco ahora.");
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("responder-truco")]
        public ActionResult<ManoTruco3v3> ResponderTruco([FromBody] Truco3v3ResponderTrucoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!TrucoServicio3v3.Responder(mano, J1, req.Aceptar, req.EscalarA, Responsable))
                throw new InvalidOperationException("No podés responder el truco ahora.");
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("irse-al-mazo")]
        public ActionResult<ManoTruco3v3> IrseAlMazo([FromBody] Truco3v3Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            if (!TrucoServicio3v3.IrseAlMazo(mano, J1))
                throw new InvalidOperationException("No podés irte al mazo ahora.");
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("nueva-mano")]
        public ActionResult<ManoTruco3v3> NuevaMano([FromBody] Truco3v3Request req)
        {
            var anterior = ObtenerMano(req.ManoId);
            if (anterior.GanadorMano == null && !anterior.PartidaTerminada)
                throw new InvalidOperationException("La mano actual aún no terminó.");
            if (anterior.PartidaTerminada)
                throw new InvalidOperationException("La partida ya terminó. Iniciá una nueva partida.");

            var nueva = CrearProximaMano(anterior.NumeroDeMano + 1, anterior.PuntosEquipoA, anterior.PuntosEquipoB, anterior.PicaPicaSlot);
            Truco3v3MemoriaServicio.Guardar(nueva);
            return Ok(nueva);
        }

        // ─────────────────────────────────────────────────────────
        //  Compañero pregunta: ¿canto los tantos? / ¿voy o pongo?
        // ─────────────────────────────────────────────────────────
        [HttpPost("responder-consulta-envido")]
        public ActionResult<ManoTruco3v3> ResponderConsultaEnvido([FromBody] Truco3v3ConsultaEnvidoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            MaquinaServicio3v3.ResolverConsultaEnvido(mano, req.Aceptar);
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("responder-consulta-truco")]
        public ActionResult<ManoTruco3v3> ResponderConsultaTruco([FromBody] Truco3v3ConsultaTrucoRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            MaquinaServicio3v3.ResolverConsultaTruco(mano, req.Voy);
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Órdenes del humano a sus compañeros bot
        // ─────────────────────────────────────────────────────────
        [HttpPost("ordenar-mayor")]
        public ActionResult<ManoTruco3v3> OrdenarMayor([FromBody] Truco3v3OrdenMayorRequest req)
        {
            var mano = ObtenerMano(req.ManoId);
            // La regla (quién puede recibir la orden) vive en el dominio.
            MaquinaServicio3v3.OrdenarJugarMayor(mano, req.JugadorId);
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        // ─────────────────────────────────────────────────────────
        //  Avanzar UNA sola acción de máquina (delay/diálogos en el front)
        // ─────────────────────────────────────────────────────────
        [HttpPost("avanzar-maquina")]
        public ActionResult<Truco3v3PasoResponse> AvanzarMaquina([FromBody] Truco3v3Request req)
        {
            var mano = ObtenerMano(req.ManoId);
            var evento = MaquinaServicio3v3.AvanzarUnPaso(mano);
            Truco3v3MemoriaServicio.Actualizar(mano);
            return Ok(new Truco3v3PasoResponse(mano, evento));
        }

        // ── Helpers ──
        private static ManoTruco3v3 ObtenerMano(Guid id) =>
            Truco3v3MemoriaServicio.Obtener(id)
            ?? throw new KeyNotFoundException($"No se encontró la mano {id}.");

    }
}
