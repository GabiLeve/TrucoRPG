using Microsoft.AspNetCore.Mvc;
using TrucoDemo.Clases;
using TrucoDemo.Models;
using TrucoDemo.Servicios;

namespace TrucoDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrucoController : ControllerBase
    {
        [HttpPost("nueva-mano")]
        public ActionResult<ManoTruco> NuevaMano([FromBody] NuevaManoRequest? request)
        {
            var mano = CrearManoNueva();

            if (request?.ManoAnteriorId != null)
            {
                var manoAnterior = PartidaMemoriaServicio.Obtener(request.ManoAnteriorId.Value);
                if (manoAnterior != null)
                {
                    mano.PuntosHumano = manoAnterior.PuntosHumano;
                    mano.PuntosMaquina = manoAnterior.PuntosMaquina;
                    mano.PartidaTerminada = manoAnterior.PartidaTerminada;
                    mano.GanadorPartida = manoAnterior.GanadorPartida;
                    mano.NivelMentiraEnvidoMaquina = manoAnterior.NivelMentiraEnvidoMaquina;
                    mano.NivelMentiraTrucoMaquina = manoAnterior.NivelMentiraTrucoMaquina;
                    mano.NumeroDeMano = manoAnterior.NumeroDeMano + 1;
                }
            }

            if (mano.PartidaTerminada)
                return BadRequest("La partida ya terminó. El primero en llegar a 30 gana. Iniciá una nueva partida.");

            mano.ManoIniciadaPor = mano.NumeroDeMano % 2 == 1 ? "Humano" : "Maquina";
            mano.TurnoActual = mano.ManoIniciadaPor;

            if (mano.ManoIniciadaPor == "Maquina")
                ProcesarIniciativaMaquina(mano);

            PartidaMemoriaServicio.Guardar(mano);
            return Ok(mano);
        }

        [HttpPost("nueva-partida")]
        public ActionResult<ManoTruco> NuevaPartida()
        {
            var mano = CrearManoNueva();
            PartidaMemoriaServicio.Guardar(mano);
            return Ok(mano);
        }

        [HttpPost("configurar-nivel-mentira-envido")]
        public ActionResult<ManoTruco> ConfigurarNivelMentiraEnvido([FromBody] ConfigurarNivelMentiraEnvidoRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null)
                return NotFound("No se encontró la mano.");

            mano.NivelMentiraEnvidoMaquina = Math.Clamp(request.NivelMentira, 0, 100);
            PartidaMemoriaServicio.Actualizar(mano);

            return Ok(mano);
        }

        [HttpPost("configurar-nivel-mentira-truco")]
        public ActionResult<ManoTruco> ConfigurarNivelMentiraTruco([FromBody] ConfigurarNivelMentiraTrucoRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null)
                return NotFound("No se encontró la mano.");

            mano.NivelMentiraTrucoMaquina = Math.Clamp(request.NivelMentira, 0, 100);
            PartidaMemoriaServicio.Actualizar(mano);

            return Ok(mano);
        }

        [HttpPost("cantar-envido")]
        public ActionResult<ManoTruco> CantarEnvido([FromBody] CantarEnvidoRequest request)
        {
            var requestTipo = new CantarEnvidoTipoRequest
            {
                ManoId = request.ManoId,
                Tipo = "Envido"
            };

            return CantarEnvidoTipo(requestTipo);
        }

        [HttpPost("cantar-envido-tipo")]
        public ActionResult<ManoTruco> CantarEnvidoTipo([FromBody] CantarEnvidoTipoRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null)
                return NotFound("No se encontró la mano.");

            if (mano.PartidaTerminada)
                return BadRequest("La partida ya terminó. El primero en llegar a 30 gana.");

            if (mano.EnvidoCantado || mano.EnvidoResuelto)
                return BadRequest("El envido ya fue cantado.");

            if (mano.Bazas.Count > 0)
                return BadRequest("El envido solo puede cantarse antes de jugar la primera baza.");

            int puntosEnJuego = ObtenerPuntosSegunTipoEnvido(request.Tipo);

            mano.TipoEnvidoCantado = NormalizarTipoEnvido(request.Tipo);

            mano.EnvidoCantado = true;
            mano.CantorEnvido = "Humano";
            mano.EnvidoPendienteRespuestaMaquina = true;
            mano.EstadoEnvido = $"Humano cantó {request.Tipo}.";

            bool aceptaMaquina = DecisionMaquinaServicio.AceptarEnvido(
                mano.Maquina.Mano,
                mano.NivelMentiraEnvidoMaquina
            );

            mano.EnvidoPendienteRespuestaMaquina = false;

            if (!aceptaMaquina)
            {

                mano.EnvidoResuelto = true;
                mano.GanadorEnvido = "Humano";
                mano.PuntosEnvido = 1;
                mano.EstadoEnvido = "La máquina no quiso. Ganaste 1 punto de envido.";
                SumarPuntos(mano, mano.GanadorEnvido, mano.PuntosEnvido);
                LimpiarDatosDeEnvido(mano);

                PartidaMemoriaServicio.Actualizar(mano);
                return Ok(mano);
            }

            ResolverEnvido(mano, puntosEnJuego, "La máquina quiso");

            PartidaMemoriaServicio.Actualizar(mano);

            return Ok(mano);
        }

        [HttpPost("responder-envido")]
        public ActionResult<ManoTruco> ResponderEnvido([FromBody] ResponderEnvidoRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null)
                return NotFound("No se encontró la mano.");

            if (mano.PartidaTerminada)
                return BadRequest("La partida ya terminó. El primero en llegar a 30 gana.");

            if (!mano.EnvidoCantado)
                return BadRequest("No hay un envido pendiente.");

            if (!mano.EnvidoPendienteRespuestaHumano)
                return BadRequest("No hay respuesta pendiente del humano.");

            mano.EnvidoPendienteRespuestaHumano = false;

            if (!request.Aceptar)
            {

                mano.EnvidoResuelto = true;
                mano.GanadorEnvido = "Maquina";
                mano.PuntosEnvido = 1;
                mano.EstadoEnvido = "No quisiste el envido. La máquina ganó 1 punto.";
                SumarPuntos(mano, mano.GanadorEnvido, mano.PuntosEnvido);
                LimpiarDatosDeEnvido(mano);

                AvanzarTurnoMaquina(mano);

                PartidaMemoriaServicio.Actualizar(mano);
                return Ok(mano);
            }

            int puntosEnJuego = ObtenerPuntosSegunTipoEnvido(mano.TipoEnvidoCantado ?? "Envido");
            ResolverEnvido(mano, puntosEnJuego, "Aceptaste el envido de la máquina");

            AvanzarTurnoMaquina(mano);

            PartidaMemoriaServicio.Actualizar(mano);

            return Ok(mano);
        }

        [HttpPost("cantar-truco")]
        public ActionResult<ManoTruco> CantarTruco([FromBody] CantarEnvidoRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null)
                return NotFound("No se encontró la mano.");

            if (mano.PartidaTerminada)
                return BadRequest("La partida ya terminó. El primero en llegar a 30 gana.");

            if (mano.GanadorMano != null)
                return BadRequest("La mano ya terminó.");

            if (mano.TrucoCantado)
                return BadRequest("El truco ya fue cantado en esta mano.");

            mano.TrucoCantado = true;
            mano.NivelTruco = 1;
            mano.CantorTruco = "Humano";
            mano.EstadoTruco = "Cantaste Truco.";

            bool aceptaMaquina = DecisionMaquinaServicio.AceptarTruco(
                mano.Maquina.Mano,
                mano.NivelMentiraTrucoMaquina
            );

            if (!aceptaMaquina)
            {

                mano.TrucoResuelto = true;
                mano.GanadorMano = "Humano";
                mano.PuntosTrucoMano = 1;
                mano.EstadoTruco = "La máquina no quiso el truco. Ganaste 1 punto.";
                SumarPuntos(mano, mano.GanadorMano, mano.PuntosTrucoMano);

                PartidaMemoriaServicio.Actualizar(mano);
                return Ok(mano);
            }

            bool escalaARetruco = DecisionMaquinaServicio.EscalarTruco(
                mano.Maquina.Mano,
                mano.NivelMentiraTrucoMaquina,
                1
            );

            if (escalaARetruco)
            {
                mano.NivelTruco = 2;
                mano.PuntosTrucoMano = 3;
                mano.CantorTruco = "Maquina";
                mano.TrucoPendienteRespuestaHumano = true;
                mano.EstadoTruco = "¡La máquina aceptó y cantó Retruco! Esta mano vale 3 puntos. ¿Querés?";
            }
            else
            {
                mano.PuntosTrucoMano = 2;
                mano.TrucoResuelto = true; // negociación cerrada: máquina aceptó sin escalar
                mano.EstadoTruco = "La máquina quiso el truco. Esta mano vale 2 puntos.";
                if (!mano.TrucoPendienteRespuestaHumano)
                    AvanzarTurnoMaquina(mano);
            }

            PartidaMemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("responder-truco")]
        public ActionResult<ManoTruco> ResponderTruco([FromBody] ResponderTrucoRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null)
                return NotFound("No se encontró la mano.");

            if (!mano.TrucoPendienteRespuestaHumano)
                return BadRequest("No hay respuesta pendiente para truco.");

            mano.TrucoPendienteRespuestaHumano = false;

            if (!request.Aceptar)
            {

                int puntosRefusal = mano.NivelTruco;
                mano.TrucoResuelto = true;
                mano.GanadorMano = mano.CantorTruco;
                mano.PuntosTrucoMano = puntosRefusal;
                string ganadorNombre = mano.GanadorMano == "Humano" ? "Vos ganaste" : "La máquina ganó";
                mano.EstadoTruco = $"No quisiste. {ganadorNombre} {puntosRefusal} punto(s).";
                SumarPuntos(mano, mano.GanadorMano, puntosRefusal);

                PartidaMemoriaServicio.Actualizar(mano);
                return Ok(mano);
            }

            var escalacion = request.EscalarA?.Trim().ToLowerInvariant();

            if (!string.IsNullOrEmpty(escalacion))
            {

                if (escalacion == "retruco" && mano.NivelTruco == 1)
                {
                    mano.NivelTruco = 2;
                    mano.PuntosTrucoMano = 3;
                    mano.CantorTruco = "Humano";

                    bool aceptaMaquina = DecisionMaquinaServicio.AceptarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina);
                    if (!aceptaMaquina)
                    {

                        mano.TrucoResuelto = true;
                        mano.GanadorMano = "Humano";
                        mano.PuntosTrucoMano = 2;
                        mano.EstadoTruco = "La máquina no quiso el retruco. ¡Ganaste 2 puntos!";
                        SumarPuntos(mano, "Humano", 2);
                    }
                    else
                    {

                        bool escalaAValeC = DecisionMaquinaServicio.EscalarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina, 2);
                        if (escalaAValeC)
                        {
                            mano.NivelTruco = 3;
                            mano.PuntosTrucoMano = 4;
                            mano.CantorTruco = "Maquina";
                            mano.TrucoPendienteRespuestaHumano = true;
                            mano.EstadoTruco = "¡La máquina aceptó y cantó Vale Cuatro! Esta mano vale 4 puntos. ¿Querés?";
                        }
                        else
                        {
                            mano.TrucoResuelto = true; // negociación cerrada
                            mano.EstadoTruco = "La máquina quiso el retruco. Esta mano vale 3 puntos.";
                        }
                    }
                }
                else if (escalacion == "valecuatro" && mano.NivelTruco == 2)
                {
                    mano.NivelTruco = 3;
                    mano.PuntosTrucoMano = 4;
                    mano.CantorTruco = "Humano";

                    bool aceptaMaquina = DecisionMaquinaServicio.AceptarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina);
                    if (!aceptaMaquina)
                    {

                        mano.TrucoResuelto = true;
                        mano.GanadorMano = "Humano";
                        mano.PuntosTrucoMano = 3;
                        mano.EstadoTruco = "La máquina no quiso el vale cuatro. ¡Ganaste 3 puntos!";
                        SumarPuntos(mano, "Humano", 3);
                    }
                    else
                    {
                        mano.TrucoResuelto = true; // negociación cerrada
                        mano.EstadoTruco = "La máquina quiso el vale cuatro. Esta mano vale 4 puntos.";
                    }
                }
                else
                {
                    PartidaMemoriaServicio.Actualizar(mano);
                    return BadRequest($"Escalación '{request.EscalarA}' inválida para el nivel actual de truco (nivel {mano.NivelTruco}).");
                }
            }
            else
            {
                mano.TrucoResuelto = true; // negociación cerrada: humano aceptó sin escalar
                mano.EstadoTruco = $"Quisiste. Esta mano vale {mano.PuntosTrucoMano} punto(s).";
            }

            if (!mano.TrucoPendienteRespuestaHumano)
                AvanzarTurnoMaquina(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("escalar-truco")]
        public ActionResult<ManoTruco> EscalarTruco([FromBody] CantarEnvidoRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null) return NotFound("No se encontró la mano.");
            if (mano.PartidaTerminada) return BadRequest("La partida ya terminó.");
            if (mano.GanadorMano != null) return BadRequest("La mano ya terminó.");
            if (!mano.TrucoCantado || mano.TrucoResuelto) return BadRequest("No hay truco activo para escalar.");
            if (mano.TrucoPendienteRespuestaHumano || mano.EnvidoPendienteRespuestaHumano) return BadRequest("Hay un canto pendiente de respuesta.");
            if (mano.NivelTruco >= 3) return BadRequest("El truco ya está en su nivel máximo.");

            mano.NivelTruco++;
            mano.CantorTruco = "Humano";
            string nombreNivel = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
            mano.PuntosTrucoMano = mano.NivelTruco == 2 ? 3 : 4;
            mano.EstadoTruco = $"Cantaste {nombreNivel}.";

            bool aceptaMaquina = DecisionMaquinaServicio.AceptarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina);
            if (!aceptaMaquina)
            {

                mano.TrucoResuelto = true;
                mano.GanadorMano = "Humano";
                mano.PuntosTrucoMano = mano.NivelTruco;
                mano.EstadoTruco = $"La máquina no quiso el {nombreNivel}. ¡Ganaste {mano.NivelTruco} punto(s)!";
                mano.CartaMaquinaEnMesa = null;
                SumarPuntos(mano, "Humano", mano.NivelTruco);
            }
            else if (mano.NivelTruco < 3 && DecisionMaquinaServicio.EscalarTruco(mano.Maquina.Mano, mano.NivelMentiraTrucoMaquina, mano.NivelTruco))
            {

                mano.NivelTruco++;
                mano.CantorTruco = "Maquina";
                mano.PuntosTrucoMano = mano.NivelTruco == 3 ? 4 : 3;
                string nombreContracanto = mano.NivelTruco == 3 ? "Vale Cuatro" : "Retruco";
                mano.TrucoPendienteRespuestaHumano = true;
                mano.EstadoTruco = $"¡La máquina aceptó y cantó {nombreContracanto}! Esta mano vale {mano.PuntosTrucoMano} punto(s). ¿Querés?";
            }
            else
            {
                mano.TrucoResuelto = true; // negociación cerrada: máquina aceptó sin escalar
                mano.EstadoTruco = $"La máquina quiso el {nombreNivel}. Esta mano vale {mano.PuntosTrucoMano} punto(s).";
            }

            if (!mano.TrucoPendienteRespuestaHumano)
                AvanzarTurnoMaquina(mano);

            PartidaMemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("irse-al-mazo")]
        public ActionResult<ManoTruco> IrseAlMazo([FromBody] CantarEnvidoRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null) return NotFound("No se encontró la mano.");
            if (mano.PartidaTerminada) return BadRequest("La partida ya terminó.");
            if (mano.GanadorMano != null) return BadRequest("La mano ya terminó.");
            if (mano.EnvidoPendienteRespuestaHumano || mano.TrucoPendienteRespuestaHumano)
                return BadRequest("Respondé el canto pendiente antes de irte al mazo.");

            // Dar los puntos del nivel negociado (si hubo truco) o 1 punto base
            int puntosParaMaquina = mano.TrucoCantado && mano.PuntosTrucoMano > 0
                ? mano.PuntosTrucoMano
                : 1;

            mano.GanadorMano = "Maquina";
            mano.TrucoResuelto = true;
            mano.CartaMaquinaEnMesa = null;
            mano.EstadoTruco = $"Te fuiste al mazo. La máquina gana {puntosParaMaquina} punto(s).";

            SumarPuntos(mano, "Maquina", puntosParaMaquina);

            PartidaMemoriaServicio.Actualizar(mano);
            return Ok(mano);
        }

        [HttpPost("jugar-carta")]
        public ActionResult<ManoTruco> JugarCarta([FromBody] JugarCartaRequest request)
        {
            var mano = PartidaMemoriaServicio.Obtener(request.ManoId);

            if (mano == null)
                return NotFound("No se encontró la mano.");

            if (mano.PartidaTerminada)
                return BadRequest("La partida ya terminó. El primero en llegar a 30 gana.");

            if (mano.GanadorMano != null)
                return BadRequest("La mano ya terminó.");

            if (mano.EnvidoPendienteRespuestaHumano)
                return BadRequest("Primero tenés que responder el envido de la máquina.");

            if (mano.TrucoPendienteRespuestaHumano)
                return BadRequest("Primero tenés que responder el truco de la máquina.");

            var cartaHumano = mano.Humano.Mano.FirstOrDefault(c =>
                c.Numero == request.Numero &&
                c.Palo.Equals(request.Palo, StringComparison.OrdinalIgnoreCase));

            if (cartaHumano == null)
                return BadRequest("La carta no existe en la mano del jugador.");

            mano.Humano.Mano.Remove(cartaHumano);
            mano.Humano.Jugadas.Add(cartaHumano);

            Carta cartaMaquina;
            if (mano.CartaMaquinaEnMesa != null)
            {

                cartaMaquina = mano.CartaMaquinaEnMesa;
                mano.CartaMaquinaEnMesa = null;
            }
            else
            {

                cartaMaquina = MaquinaServicio.ElegirCarta(mano.Maquina.Mano, cartaHumano);
                mano.Maquina.Mano.Remove(cartaMaquina);
                mano.Maquina.Jugadas.Add(cartaMaquina);
            }

            var ganadorBaza = JuegoServicio.ResolverBaza(cartaHumano, cartaMaquina);

            mano.Bazas.Add(new Baza
            {
                CartaJugador = cartaHumano,
                CartaMaquina = cartaMaquina,
                Ganador = ganadorBaza
            });

            if (ganadorBaza == "Parda")
                mano.TurnoActual = mano.ManoIniciadaPor;
            else
                mano.TurnoActual = ganadorBaza;

            mano.GanadorMano = JuegoServicio.ResolverGanadorMano(mano.Bazas, mano.ManoIniciadaPor);

            if (mano.GanadorMano == "Humano" || mano.GanadorMano == "Maquina")
            {
                if (!mano.TrucoCantado)
                    mano.EstadoTruco = "No se cantó truco. La mano vale 1 punto.";

                int puntosMano = mano.PuntosTrucoMano > 0 ? mano.PuntosTrucoMano : 1;
                SumarPuntos(mano, mano.GanadorMano, puntosMano);
                mano.TrucoResuelto = true;
            }
            else
            {

                AvanzarTurnoMaquina(mano);
            }

            PartidaMemoriaServicio.Actualizar(mano);

            return Ok(mano);
        }

        private ManoTruco CrearManoNueva()
        {
            var mano = new ManoTruco
            {
                Humano = new Jugador
                {
                    Nombre = "Usuario",
                    EsMaquina = false
                },
                Maquina = new Jugador
                {
                    Nombre = "Maquina",
                    EsMaquina = true
                },
                EstadoEnvido = null,
                EstadoTruco = null,
                NumeroDeMano = 1,
                ManoIniciadaPor = "Humano",
                TurnoActual = "Humano"
            };

            RepartoServicio.Repartir(mano);
            return mano;
        }

        private void ProcesarIniciativaMaquina(ManoTruco mano)
        {

            if (!mano.EnvidoCantado && !mano.EnvidoResuelto && mano.Bazas.Count == 0)
            {
                bool cantaEnvido = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(
                    mano.Maquina.Mano,
                    mano.NivelMentiraEnvidoMaquina
                );

                if (cantaEnvido)
                {
                    mano.EnvidoCantado = true;
                    mano.CantorEnvido = "Maquina";
                    mano.TipoEnvidoCantado = "Envido";
                    mano.EnvidoPendienteRespuestaHumano = true;
                    mano.EstadoEnvido = "La máquina cantó Envido.";
                }
            }

            AvanzarTurnoMaquina(mano);
        }

        private void AvanzarTurnoMaquina(ManoTruco mano)
        {
            if (mano.GanadorMano != null || mano.PartidaTerminada) return;
            if (mano.TurnoActual != "Maquina") return;
            if (mano.EnvidoPendienteRespuestaHumano || mano.TrucoPendienteRespuestaHumano) return;
            if (mano.CartaMaquinaEnMesa != null) return;
            if (mano.Maquina.Mano.Count == 0) return;

            if (!mano.TrucoCantado && !mano.TrucoResuelto)
            {
                bool cantaTruco = IniciativaMaquinaTrucoServicio.DebeCantarTruco(
                    mano.Maquina.Mano,
                    mano.NivelMentiraTrucoMaquina
                );

                if (cantaTruco)
                {
                    mano.TrucoCantado = true;
                    mano.NivelTruco = 1;
                    mano.PuntosTrucoMano = 2;
                    mano.TrucoPendienteRespuestaHumano = true;
                    mano.CantorTruco = "Maquina";
                    mano.EstadoTruco = "La máquina cantó Truco.";
                    return;
                }
            }

            var carta = MaquinaServicio.ElegirCarta(mano.Maquina.Mano, null);
            mano.Maquina.Mano.Remove(carta);
            mano.Maquina.Jugadas.Add(carta);
            mano.CartaMaquinaEnMesa = carta;
        }

        private void ResolverEnvido(ManoTruco mano, int puntosEnJuego, string prefijoEstado)
        {
            mano.TantoHumano = EnvidoServicio.CalcularTanto(mano.Humano.Mano);
            mano.TantoMaquina = EnvidoServicio.CalcularTanto(mano.Maquina.Mano);

            mano.TantoCantadoMaquina = MentiraEnvidoServicio.ObtenerTantoCantado(
                mano.TantoMaquina.Value,
                mano.NivelMentiraEnvidoMaquina,
                out bool mintio
            );

            mano.MaquinaMintioEnvido = mintio;
            mano.TipoCantoEnvidoMaquina = ClasificarActitudEnvido(mano.TantoMaquina.Value, mintio);

            if (mano.TantoHumano > mano.TantoMaquina)
                mano.GanadorEnvido = "Humano";
            else if (mano.TantoMaquina > mano.TantoHumano)
                mano.GanadorEnvido = "Maquina";
            else
                mano.GanadorEnvido = mano.ManoIniciadaPor;

            if (mano.TipoEnvidoCantado == "FaltaEnvido")
            {
                int puntosActualesGanador = mano.GanadorEnvido == "Humano"
                    ? mano.PuntosHumano
                    : mano.PuntosMaquina;
                puntosEnJuego = Math.Max(30 - puntosActualesGanador, 1);
            }

            mano.PuntosEnvido = puntosEnJuego;
            mano.EnvidoResuelto = true;
            mano.EstadoEnvido =
                $"{prefijoEstado}. Tu tanto: {mano.TantoHumano}. La máquina cantó: {mano.TantoCantadoMaquina} " +
                $"(real: {mano.TantoMaquina}). Ganador del envido: {mano.GanadorEnvido} ({mano.PuntosEnvido} pto/s).";
            SumarPuntos(mano, mano.GanadorEnvido, mano.PuntosEnvido);
        }

        private void LimpiarDatosDeEnvido(ManoTruco mano)
        {
            mano.TantoHumano = null;
            mano.TantoMaquina = null;
            mano.TantoCantadoMaquina = null;
            mano.MaquinaMintioEnvido = false;
            mano.TipoCantoEnvidoMaquina = null;
        }

        private string ClasificarActitudEnvido(int tantoReal, bool mintio)
        {
            if (mintio)
                return "mintio";

            if (tantoReal < 23)
                return "se_jugo";

            return "tenia";
        }

        private int ObtenerPuntosSegunTipoEnvido(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return 2;

            return tipo.Trim().ToLowerInvariant() switch
            {
                "envido"       => 2,
                "real envido"  => 3,
                "realenvido"   => 3,
                "falta envido" => 0,
                "faltaenvido"  => 0,
                _              => 2
            };
        }

        private string NormalizarTipoEnvido(string? tipo)
        {
            return tipo?.Trim().ToLowerInvariant() switch
            {
                "real envido"  => "RealEnvido",
                "realenvido"   => "RealEnvido",
                "falta envido" => "FaltaEnvido",
                "faltaenvido"  => "FaltaEnvido",
                _              => "Envido"
            };
        }

        private void SumarPuntos(ManoTruco mano, string? ganador, int puntos)
        {
            if (puntos <= 0)
                return;

            if (ganador == "Humano")
                mano.PuntosHumano += puntos;
            else if (ganador == "Maquina")
                mano.PuntosMaquina += puntos;

            ActualizarEstadoPartida(mano);
        }

        private void ActualizarEstadoPartida(ManoTruco mano)
        {
            const int puntosObjetivo = 30;

            if (mano.PuntosHumano >= puntosObjetivo || mano.PuntosMaquina >= puntosObjetivo)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida = mano.PuntosHumano >= puntosObjetivo ? "Humano" : "Maquina";
            }
        }
    }
}
