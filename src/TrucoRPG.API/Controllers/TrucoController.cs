
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    /// <summary>
    /// Partida de Truco 1v1 contra la máquina. Cada endpoint representa una acción del
    /// juego (cantar, responder, jugar carta, avanzar a la máquina, etc.) y devuelve el
    /// estado actualizado de la mano. Todos requieren JWT (rol Jugador).
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Jugador")]
    [Route("api/[controller]")]
    [Produces("application/json")]
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
        private readonly ConfirmarAullidoUseCase       _confirmarAullido;
        private readonly ConfirmarDestelloUseCase      _confirmarDestello;
        private readonly ConfirmarEspejismoUseCase     _confirmarEspejismo;
        private readonly ConfirmarMandingaEspejoUseCase  _confirmarMandingaEspejo;
        private readonly ConfirmarMandingaEnganoUseCase  _confirmarMandingaEngano;
        private readonly ConfirmarMandingaMaldicionUseCase _confirmarMandingaMaldicion;
        private readonly AvanzarMaquinaHistoriaUseCase _avanzarMaquinaHistoria;
        private readonly GanarAutomaticoDebugUseCase _ganarAutomaticoDebug;
        private readonly SumarPuntosHumanoDebugUseCase _sumarPuntosHumanoDebug;
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
            ConfirmarAullidoUseCase       confirmarAullido,
            ConfirmarDestelloUseCase      confirmarDestello,
            ConfirmarEspejismoUseCase     confirmarEspejismo,
            ConfirmarMandingaEspejoUseCase confirmarMandingaEspejo,
            ConfirmarMandingaEnganoUseCase confirmarMandingaEngano,
            ConfirmarMandingaMaldicionUseCase confirmarMandingaMaldicion,
            AvanzarMaquinaHistoriaUseCase avanzarMaquinaHistoria,
            GanarAutomaticoDebugUseCase   ganarAutomaticoDebug,
            SumarPuntosHumanoDebugUseCase sumarPuntosHumanoDebug,
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
            _confirmarAullido = confirmarAullido;
            _confirmarDestello = confirmarDestello;
            _confirmarEspejismo = confirmarEspejismo;
            _confirmarMandingaEspejo = confirmarMandingaEspejo;
            _confirmarMandingaEngano = confirmarMandingaEngano;
            _confirmarMandingaMaldicion = confirmarMandingaMaldicion;
            _avanzarMaquinaHistoria = avanzarMaquinaHistoria;
            _ganarAutomaticoDebug = ganarAutomaticoDebug;
            _sumarPuntosHumanoDebug = sumarPuntosHumanoDebug;
            _historiaValidacion = historiaValidacion;
            _usuarioActual      = usuarioActual;
        }

        // ── Partida / Mano ────────────────────────────────────────────

        /// <summary>Reparte una nueva mano dentro de la partida en curso.</summary>
        /// <param name="request">Opcional: id de la mano anterior para encadenar el puntaje.</param>
        [HttpPost("nueva-mano")]
        public ActionResult<ManoTruco> NuevaMano([FromBody] NuevaManoRequest? request)
        {

                return Ok(_nuevaMano.Ejecutar(request?.ManoAnteriorId));

        }

        /// <summary>Crea una partida nueva (modo libre o historia).</summary>
        /// <param name="request">Modo de juego, héroe del humano y, en historia, el nivel del rival.</param>
        /// <remarks>En modo historia valida que el jugador pueda pelear contra ese rival y configura al rival de la máquina.</remarks>
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

        /// <summary>Confirma el efecto de la habilidad "Salpicadura" y devuelve la mano actualizada.</summary>
        [HttpPost("confirmar-salpicadura")]
        public ActionResult<ManoTruco> ConfirmarSalpicadura([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarSalpicadura.Ejecutar(request.ManoId));

        /// <summary>Confirma el efecto de la habilidad "Travesura" y devuelve la mano actualizada.</summary>
        [HttpPost("confirmar-travesura")]
        public ActionResult<ManoTruco> ConfirmarTravesura([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarTravesura.Ejecutar(request.ManoId));

        /// <summary>Confirma el efecto de la habilidad "Rasguño" y devuelve la mano actualizada.</summary>
        [HttpPost("confirmar-rasguno")]
        public ActionResult<ManoTruco> ConfirmarRasguno([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarRasguno.Ejecutar(request.ManoId));

        /// <summary>Confirma el efecto de la habilidad "Aullido" y devuelve la mano actualizada.</summary>
        [HttpPost("confirmar-aullido")]
        public ActionResult<ManoTruco> ConfirmarAullido([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarAullido.Ejecutar(request.ManoId));

        /// <summary>Confirma el efecto de la habilidad "Destello" y devuelve la mano actualizada.</summary>
        [HttpPost("confirmar-destello")]
        public ActionResult<ManoTruco> ConfirmarDestello([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarDestello.Ejecutar(request.ManoId));

        /// <summary>Confirma el overlay de la pasiva "Espejismo" y habilita el parpadeo visual.</summary>
        [HttpPost("confirmar-espejismo")]
        public ActionResult<ManoTruco> ConfirmarEspejismo([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarEspejismo.Ejecutar(request.ManoId));

        [HttpPost("confirmar-mandinga-espejo")]
        public ActionResult<ManoTruco> ConfirmarMandingaEspejo([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarMandingaEspejo.Ejecutar(request.ManoId));

        [HttpPost("confirmar-mandinga-engano")]
        public ActionResult<ManoTruco> ConfirmarMandingaEngano([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarMandingaEngano.Ejecutar(request.ManoId));

        [HttpPost("confirmar-mandinga-maldicion")]
        public ActionResult<ManoTruco> ConfirmarMandingaMaldicion([FromBody] ConfirmarSalpicaduraRequest request) =>
            Ok(_confirmarMandingaMaldicion.Ejecutar(request.ManoId));

        // ── Configuración ─────────────────────────────────────────────

        /// <summary>Ajusta cuánto "miente" la máquina al jugar el envido (nivel de mentira) en esa mano.</summary>
        [HttpPost("configurar-nivel-mentira-envido")]
        public ActionResult<ManoTruco> ConfigurarNivelMentiraEnvido(
            [FromBody] ConfigurarNivelMentiraEnvidoRequest request)
        {

                return Ok(_configurarMentira.EjecutarEnvido(request.ManoId, request.NivelMentira));

        }

        /// <summary>Ajusta cuánto "miente" la máquina al jugar el truco (nivel de mentira) en esa mano.</summary>
        [HttpPost("configurar-nivel-mentira-truco")]
        public ActionResult<ManoTruco> ConfigurarNivelMentiraTruco(
            [FromBody] ConfigurarNivelMentiraTrucoRequest request)
        {

                return Ok(_configurarMentira.EjecutarTruco(request.ManoId, request.NivelMentira));

        }

        // ── Envido ────────────────────────────────────────────────────

        /// <summary>El humano canta "Envido".</summary>
        [HttpPost("cantar-envido")]
        public ActionResult<ManoTruco> CantarEnvido([FromBody] CantarEnvidoRequest request)
        {

                return Ok(_cantarEnvido.Ejecutar(request.ManoId, "Envido"));

        }

        /// <summary>El humano canta un tipo específico de envido (Envido, Real Envido, Falta Envido, etc.).</summary>
        [HttpPost("cantar-envido-tipo")]
        public ActionResult<ManoTruco> CantarEnvidoTipo([FromBody] CantarEnvidoTipoRequest request)
        {

                return Ok(_cantarEnvido.Ejecutar(request.ManoId, request.Tipo));

        }

        /// <summary>Responde un envido cantado (quiero / no quiero), con opción de escalar a otro canto.</summary>
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

        /// <summary>El humano canta Truco.</summary>
        [HttpPost("cantar-truco")]
        public ActionResult<ManoTruco> CantarTruco([FromBody] CantarEnvidoRequest request)
        {

                return Ok(_cantarTruco.Ejecutar(request.ManoId));

        }

        /// <summary>Responde el truco (quiero / no quiero), con opción de escalar (Retruco / Vale Cuatro).</summary>
        [HttpPost("responder-truco")]
        public ActionResult<ManoTruco> ResponderTruco([FromBody] ResponderTrucoRequest request)
        {

                return Ok(_responderTruco.Ejecutar(request.ManoId, request.Aceptar, request.EscalarA));

        }

        /// <summary>Sube la apuesta del truco al siguiente nivel (Truco → Retruco → Vale Cuatro).</summary>
        [HttpPost("escalar-truco")]
        public ActionResult<ManoTruco> EscalarTruco([FromBody] CantarEnvidoRequest request)
        {

                return Ok(_escalarTruco.Ejecutar(request.ManoId));

        }

        // ── Juego ─────────────────────────────────────────────────────

        /// <summary>El humano se va al mazo y abandona la mano (la máquina se lleva los puntos en juego).</summary>
        [HttpPost("irse-al-mazo")]
        public ActionResult<ManoTruco> IrseAlMazo([FromBody] CantarEnvidoRequest request)
        {

                return Ok(_irseAlMazo.Ejecutar(request.ManoId));

        }

        /// <summary>Juega una carta (número y palo) sobre la mesa.</summary>
        [HttpPost("jugar-carta")]
        public ActionResult<ManoTruco> JugarCarta([FromBody] JugarCartaRequest request)
        {

                return Ok(_jugarCarta.Ejecutar(request.ManoId, request.Numero, request.Palo));

        }

        /// <summary>Activa la habilidad del héroe sobre una carta determinada.</summary>
        [HttpPost("activar-habilidad")]
        public ActionResult<ManoTruco> ActivarHabilidad([FromBody] ActivarHabilidadRequest request)
        {

                return Ok(_activarHabilidad.Ejecutar(
                    request.ManoId, request.NumeroCarta, request.PaloCarta));

        }

        /// <summary>Hace avanzar un paso a la máquina y devuelve la mano más el evento resultante.</summary>
        [HttpPost("avanzar-maquina")]
        public ActionResult<Truco1v1PasoResponse> AvanzarMaquina([FromBody] CantarEnvidoRequest request)
        {
            var (mano, evento) = _avanzarMaquinaHistoria.Ejecutar(request.ManoId);
            return Ok(new Truco1v1PasoResponse { Mano = mano, Evento = evento });
        }

        /// <summary>SOLO PRUEBAS: fuerza la victoria automática en historia. Eliminar antes de producción.</summary>
        // SOLO PRUEBAS — Botón debug de victoria automática en historia. Eliminar antes de producción.
        [HttpPost("ganar-automatico-debug")]
        public ActionResult<ManoTruco> GanarAutomaticoDebug([FromBody] CantarEnvidoRequest request) =>
            Ok(_ganarAutomaticoDebug.Ejecutar(request.ManoId));

        /// <summary>SOLO PRUEBAS: suma 10 puntos al humano contra El Mandinga. Eliminar antes de producción.</summary>
        [HttpPost("sumar-puntos-humano-debug")]
        public ActionResult<ManoTruco> SumarPuntosHumanoDebug([FromBody] CantarEnvidoRequest request) =>
            Ok(_sumarPuntosHumanoDebug.Ejecutar(request.ManoId));
    }
}
