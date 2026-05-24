using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrucoController : ControllerBase
    {
        private readonly NuevaManoUseCase              _nuevaMano;
        private readonly ConfigurarNivelMentiraUseCase _configurarMentira;
        private readonly CantarEnvidoUseCase           _cantarEnvido;
        private readonly ResponderEnvidoUseCase        _responderEnvido;
        private readonly CantarTrucoUseCase            _cantarTruco;
        private readonly ResponderTrucoUseCase         _responderTruco;
        private readonly EscalarTrucoUseCase           _escalarTruco;
        private readonly IrseAlMazoUseCase             _irseAlMazo;
        private readonly JugarCartaUseCase             _jugarCarta;
        private readonly ActivarHabilidadUseCase       _activarHabilidad;

        public TrucoController(
            NuevaManoUseCase              nuevaMano,
            ConfigurarNivelMentiraUseCase configurarMentira,
            CantarEnvidoUseCase           cantarEnvido,
            ResponderEnvidoUseCase        responderEnvido,
            CantarTrucoUseCase            cantarTruco,
            ResponderTrucoUseCase         responderTruco,
            EscalarTrucoUseCase           escalarTruco,
            IrseAlMazoUseCase             irseAlMazo,
            JugarCartaUseCase             jugarCarta,
            ActivarHabilidadUseCase       activarHabilidad)
        {
            _nuevaMano         = nuevaMano;
            _configurarMentira = configurarMentira;
            _cantarEnvido      = cantarEnvido;
            _responderEnvido   = responderEnvido;
            _cantarTruco       = cantarTruco;
            _responderTruco    = responderTruco;
            _escalarTruco      = escalarTruco;
            _irseAlMazo        = irseAlMazo;
            _jugarCarta        = jugarCarta;
            _activarHabilidad  = activarHabilidad;
        }

        // ── Partida / Mano ────────────────────────────────────────────

        [HttpPost("nueva-mano")]
        public ActionResult<ManoTruco> NuevaMano([FromBody] NuevaManoRequest? request)
        {
            try
            {
                return Ok(_nuevaMano.Ejecutar(request?.ManoAnteriorId));
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("nueva-partida")]
        public ActionResult<ManoTruco> NuevaPartida([FromBody] NuevaPartidaRequest? request)
        {
            var configuracion = request == null
                ? new ConfiguracionPartida()
                : new ConfiguracionPartida
                {
                    Modo = request.Modo,
                    HeroeDelHumano = request.ClaseHeroe
                };

            return Ok(_nuevaMano.EjecutarNuevaPartida(configuracion));
        }

        // ── Configuración ─────────────────────────────────────────────

        [HttpPost("configurar-nivel-mentira-envido")]
        public ActionResult<ManoTruco> ConfigurarNivelMentiraEnvido(
            [FromBody] ConfigurarNivelMentiraEnvidoRequest request)
        {
            try
            {
                return Ok(_configurarMentira.EjecutarEnvido(request.ManoId, request.NivelMentira));
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPost("configurar-nivel-mentira-truco")]
        public ActionResult<ManoTruco> ConfigurarNivelMentiraTruco(
            [FromBody] ConfigurarNivelMentiraTrucoRequest request)
        {
            try
            {
                return Ok(_configurarMentira.EjecutarTruco(request.ManoId, request.NivelMentira));
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        // ── Envido ────────────────────────────────────────────────────

        [HttpPost("cantar-envido")]
        public ActionResult<ManoTruco> CantarEnvido([FromBody] CantarEnvidoRequest request)
        {
            try
            {
                return Ok(_cantarEnvido.Ejecutar(request.ManoId, "Envido"));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("cantar-envido-tipo")]
        public ActionResult<ManoTruco> CantarEnvidoTipo([FromBody] CantarEnvidoTipoRequest request)
        {
            try
            {
                return Ok(_cantarEnvido.Ejecutar(request.ManoId, request.Tipo));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("responder-envido")]
        public ActionResult<ManoTruco> ResponderEnvido([FromBody] ResponderEnvidoRequest request)
        {
            try
            {
                return Ok(_responderEnvido.Ejecutar(request.ManoId, request.Aceptar, request.EscalarA));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // ── Truco ─────────────────────────────────────────────────────

        [HttpPost("cantar-truco")]
        public ActionResult<ManoTruco> CantarTruco([FromBody] CantarEnvidoRequest request)
        {
            try
            {
                return Ok(_cantarTruco.Ejecutar(request.ManoId));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("responder-truco")]
        public ActionResult<ManoTruco> ResponderTruco([FromBody] ResponderTrucoRequest request)
        {
            try
            {
                return Ok(_responderTruco.Ejecutar(request.ManoId, request.Aceptar, request.EscalarA));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("escalar-truco")]
        public ActionResult<ManoTruco> EscalarTruco([FromBody] CantarEnvidoRequest request)
        {
            try
            {
                return Ok(_escalarTruco.Ejecutar(request.ManoId));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // ── Juego ─────────────────────────────────────────────────────

        [HttpPost("irse-al-mazo")]
        public ActionResult<ManoTruco> IrseAlMazo([FromBody] CantarEnvidoRequest request)
        {
            try
            {
                return Ok(_irseAlMazo.Ejecutar(request.ManoId));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("jugar-carta")]
        public ActionResult<ManoTruco> JugarCarta([FromBody] JugarCartaRequest request)
        {
            try
            {
                return Ok(_jugarCarta.Ejecutar(request.ManoId, request.Numero, request.Palo));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("activar-habilidad")]
        public ActionResult<ManoTruco> ActivarHabilidad([FromBody] ActivarHabilidadRequest request)
        {
            try
            {
                return Ok(_activarHabilidad.Ejecutar(
                    request.ManoId, request.NumeroCarta, request.PaloCarta));
            }
            catch (KeyNotFoundException ex)      { return NotFound(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
