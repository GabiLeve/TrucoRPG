
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Jugador")]
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
        private readonly ConfirmarSalpicaduraUseCase   _confirmarSalpicadura;
        private readonly ConfirmarTravesuraUseCase     _confirmarTravesura;
        private readonly ConfirmarRasgunoUseCase       _confirmarRasguno;
        private readonly AvanzarMaquinaHistoriaUseCase _avanzarMaquinaHistoria;
        private readonly GanarAutomaticoDebugUseCase _ganarAutomaticoDebug;
        private readonly HistoriaValidacionServicio    _historiaValidacion;
        private readonly IUsuarioActualServicio        _usuarioActual;

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
            ActivarHabilidadUseCase       activarHabilidad,
            ConfirmarSalpicaduraUseCase   confirmarSalpicadura,
            ConfirmarTravesuraUseCase     confirmarTravesura,
            ConfirmarRasgunoUseCase       confirmarRasguno,
            AvanzarMaquinaHistoriaUseCase avanzarMaquinaHistoria,
            GanarAutomaticoDebugUseCase   ganarAutomaticoDebug,
            HistoriaValidacionServicio    historiaValidacion,
            IUsuarioActualServicio        usuarioActual)
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
            _confirmarSalpicadura = confirmarSalpicadura;
            _confirmarTravesura = confirmarTravesura;
            _confirmarRasguno = confirmarRasguno;
            _avanzarMaquinaHistoria = avanzarMaquinaHistoria;
            _ganarAutomaticoDebug = ganarAutomaticoDebug;
            _historiaValidacion = historiaValidacion;
            _usuarioActual      = usuarioActual;
        }

        // ── Partida / Mano ────────────────────────────────────────────

        [HttpPost("nueva-mano")]
        public ActionResult<ManoTruco> NuevaMano([FromBody] NuevaManoRequest? request)
        {
            
                return Ok(_nuevaMano.Ejecutar(request?.ManoAnteriorId));
            
        }

        [HttpPost("nueva-partida")]
        public async Task<ActionResult<ManoTruco>> NuevaPartida([FromBody] NuevaPartidaRequest? request)
        {
            var configuracion = request == null
                ? new ConfiguracionPartida()
                : new ConfiguracionPartida
                {
                    Modo = request.Modo,
                    HeroeDelHumano = request.ClaseHeroe
                };

            if (request?.Modo == ModoJuego.Historia && request.RivalNivel.HasValue)
            {
                await _historiaValidacion.ValidarPuedeIniciarPartidaAsync(
                    _usuarioActual.ObtenerId(),
                    request.RivalNivel.Value);

                var rival = await _historiaValidacion.ObtenerRivalOErrorAsync(request.RivalNivel.Value);
                configuracion.RivalNivel = rival.Nivel;
                configuracion.RivalDeLaMaquina = rival.TipoRival;
            }

            return Ok(_nuevaMano.EjecutarNuevaPartida(configuracion));
        }

        [HttpPost("confirmar-salpicadura")]
        public ActionResult<ManoTruco> ConfirmarSalpicadura([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarSalpicadura.Ejecutar(request.ManoId));

        [HttpPost("confirmar-travesura")]
        public ActionResult<ManoTruco> ConfirmarTravesura([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarTravesura.Ejecutar(request.ManoId));

        [HttpPost("confirmar-rasguno")]
        public ActionResult<ManoTruco> ConfirmarRasguno([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarRasguno.Ejecutar(request.ManoId));

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

        /// <summary>
        /// "Son buenas" en 1v1: el humano reconoce que la máquina tiene más tantos y pierde el envido.
        /// Solo válido cuando la máquina cantó el envido y el humano ya lo aceptó (quiero).
        /// </summary>
        [HttpPost("son-buenas")]
        public ActionResult<ManoTruco> SonBuenas([FromBody] CantarEnvidoRequest request)
        {
            return Ok(_responderEnvido.EjecutarSonBuenas(request.ManoId));
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

        [HttpPost("avanzar-maquina")]
        public ActionResult<Truco1v1PasoResponse> AvanzarMaquina([FromBody] CantarEnvidoRequest request)
        {
            var (mano, evento) = _avanzarMaquinaHistoria.Ejecutar(request.ManoId);
            return Ok(new Truco1v1PasoResponse { Mano = mano, Evento = evento });
        }

        // SOLO PRUEBAS — Botón debug de victoria automática en historia. Eliminar antes de producción.
        [HttpPost("ganar-automatico-debug")]
        public ActionResult<ManoTruco> GanarAutomaticoDebug([FromBody] CantarEnvidoRequest request) =>
            Ok(_ganarAutomaticoDebug.Ejecutar(request.ManoId));
    }
}
