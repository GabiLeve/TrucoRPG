using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Heroes
{
    public sealed class FanfarronHabilidad : HabilidadHeroeBase
    {
        public override ClaseHeroe Tipo => ClaseHeroe.Fanfarron;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            var estado = contexto.EstadoDe(IdJugador.Humano, ClaseHeroe.Fanfarron);

            if (evento == EventoPartida.ManoIniciada)
            {
                ReglasHabilidadActiva.ReiniciarUsoEnMano(estado);
                estado.ActivaDisponible = true;
                return;
            }

            if (evento == EventoPartida.EmpateEnvido && datos is ResolucionEmpateEnvido resolucion)
            {
                resolucion.GanadorFinal = IdJugador.Humano;
                contexto.Mano.UltimoMensajeHabilidad =
                    "Fanfarrón: ganás el empate de envido automáticamente.";
                return;
            }

            if (evento == EventoPartida.AntesDeSumarPuntos
                && datos is ModificadorPuntos mod
                && mod.Origen == OrigenPuntos.Envido
                && estado.FanfarronBonusPendiente
                && mod.CantorId == IdJugador.Humano
                && mod.PuntosBase > 1)
            {
                mod.PuntosFinales += 1;
                estado.FanfarronBonusPendiente = false;
                contexto.Mano.UltimoMensajeHabilidad =
                    "Fanfarrón: +1 punto extra en el envido aceptado.";
                return;
            }

            if (evento == EventoPartida.TrucoAceptado
                && datos is CantoTrucoAceptado canto
                && estado.FanfarronBonusPendiente
                && canto.CantorId == IdJugador.Humano)
            {
                contexto.Mano.PuntosTrucoMano += 1;
                estado.FanfarronBonusPendiente = false;
                contexto.Mano.UltimoMensajeHabilidad =
                    $"Fanfarrón: +1 punto extra. Esta mano vale {contexto.Mano.PuntosTrucoMano} puntos.";
            }
        }

        public override ResultadoActivarHabilidad? IntentarActivar(
            ContextoPartida contexto,
            SolicitudActivarHabilidad solicitud)
        {
            var estado = contexto.EstadoDe(IdJugador.Humano, ClaseHeroe.Fanfarron);
            if (!ReglasHabilidadActiva.ValidarActivaBase(contexto, estado, out var error))
                return ResultadoActivarHabilidad.Error(error!);

            if (estado.FanfarronBonusPendiente)
                return ResultadoActivarHabilidad.Error("Ya tenés un bonus de canto pendiente.");

            estado.FanfarronBonusPendiente = true;
            estado.ActivaUsadaEnEstaMano = true;
            estado.ActivaDisponible = false;

            var msg = "Fanfarrón: tu próximo envido o truco aceptado vale +1 punto extra.";
            contexto.Mano.UltimoMensajeHabilidad = msg;

            return ResultadoActivarHabilidad.Ok(msg);
        }
    }
}
