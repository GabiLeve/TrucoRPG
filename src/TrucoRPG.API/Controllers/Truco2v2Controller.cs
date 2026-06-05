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

    [ApiController]
    [Route("api/[controller]")]
    public class Truco2v2Controller : ControllerBase
    {
        // ── IDs fijos de jugadores ─────────────────────────────────
        private const string J1 = "J1"; // humano
        private const string J2 = "J2"; // rival 1
        private const string J3 = "J3"; // compañero
        private const string J4 = "J4"; // rival 2

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

            AvanzarMaquinas(mano);
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
            AvanzarMaquinas(mano);
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

            AvanzarMaquinas(mano);
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

                AvanzarMaquinas(mano);
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

            AvanzarMaquinas(mano);
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
            AvanzarMaquinas(mano);
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
            AvanzarMaquinas(mano);
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

            AvanzarMaquinas(mano);
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

            AvanzarMaquinas(mano);
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

            AvanzarMaquinas(nueva);
            Truco2v2MemoriaServicio.Guardar(nueva);
            return Ok(nueva);
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

        /// <summary>
        /// Avanza los turnos de los jugadores máquina hasta que sea el turno
        /// del humano (J1) o la partida/mano termine.
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
            if (mano.TrucoPendienteRespuestaDe != null)    return mano.TrucoPendienteRespuestaDe;
            if (mano.EnvidoPendienteRespuestaDe != null)   return mano.EnvidoPendienteRespuestaDe;
            if (!mano.ManoTerminada && mano.GanadorMano == null) return mano.TurnoActual;
            return null;
        }
    }
}
