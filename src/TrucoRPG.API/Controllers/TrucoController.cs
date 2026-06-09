using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    //[Authorize(Roles = "Jugador")] <-- DESCOMENTAR CUANDO ESTÉ IMPLEMENTADA LA SESIÓN DE USUARIOS
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
            
                return Ok(_nuevaMano.Ejecutar(request?.ManoAnteriorId));
            
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

                return Ok(_configurarMentira.EjecutarEnvido(request.ManoId, request.NivelMentira));
            
        }

        [HttpPost("configurar-nivel-mentira-truco")]
        public ActionResult<ManoTruco> ConfigurarNivelMentiraTruco(
            [FromBody] ConfigurarNivelMentiraTrucoRequest request)
        {
            
                return Ok(_configurarMentira.EjecutarTruco(request.ManoId, request.NivelMentira));
            
        }

        // ── Envido ────────────────────────────────────────────────────

        [HttpPost("cantar-envido")]
        public ActionResult<ManoTruco> CantarEnvido([FromBody] CantarEnvidoRequest request)
        {
            
                return Ok(_cantarEnvido.Ejecutar(request.ManoId, "Envido"));
            
        }

        [HttpPost("cantar-envido-tipo")]
        public ActionResult<ManoTruco> CantarEnvidoTipo([FromBody] CantarEnvidoTipoRequest request)
        {
            
                return Ok(_cantarEnvido.Ejecutar(request.ManoId, request.Tipo));
            
        }

        [HttpPost("responder-envido")]
        public ActionResult<ManoTruco> ResponderEnvido([FromBody] ResponderEnvidoRequest request)
        {
            
                return Ok(_responderEnvido.Ejecutar(request.ManoId, request.Aceptar, request.EscalarA));
            
        }

        // ── Truco ─────────────────────────────────────────────────────

        [HttpPost("cantar-truco")]
        public ActionResult<ManoTruco> CantarTruco([FromBody] CantarEnvidoRequest request)
        {
            
                return Ok(_cantarTruco.Ejecutar(request.ManoId));
            
        }

        [HttpPost("responder-truco")]
        public ActionResult<ManoTruco> ResponderTruco([FromBody] ResponderTrucoRequest request)
        {
            
                return Ok(_responderTruco.Ejecutar(request.ManoId, request.Aceptar, request.EscalarA));
           
        }

        [HttpPost("escalar-truco")]
        public ActionResult<ManoTruco> EscalarTruco([FromBody] CantarEnvidoRequest request)
        {
            
                return Ok(_escalarTruco.Ejecutar(request.ManoId));
            
        }

        // ── Juego ─────────────────────────────────────────────────────

        [HttpPost("irse-al-mazo")]
        public ActionResult<ManoTruco> IrseAlMazo([FromBody] CantarEnvidoRequest request)
        {
            
                return Ok(_irseAlMazo.Ejecutar(request.ManoId));
            
        }

        [HttpPost("jugar-carta")]
        public ActionResult<ManoTruco> JugarCarta([FromBody] JugarCartaRequest request)
        {
            
                return Ok(_jugarCarta.Ejecutar(request.ManoId, request.Numero, request.Palo));
            
        }

        [HttpPost("activar-habilidad")]
        public ActionResult<ManoTruco> ActivarHabilidad([FromBody] ActivarHabilidadRequest request)
        {
            
                return Ok(_activarHabilidad.Ejecutar(
                    request.ManoId, request.NumeroCarta, request.PaloCarta));
            
        }
    }
}
