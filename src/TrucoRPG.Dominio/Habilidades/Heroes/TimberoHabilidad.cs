using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.Habilidades.Heroes
{
    public sealed class TimberoHabilidad : HabilidadHeroeBase
    {
        public override ClaseHeroe Tipo => ClaseHeroe.Timbero;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            var estado = contexto.EstadoDe(IdJugador.Humano, ClaseHeroe.Timbero);

            if (evento == EventoPartida.ManoIniciada)
            {
                ReglasHabilidadActiva.ReiniciarUsoEnMano(estado);
                estado.ActivaDisponible = true;

                if (AzarServicio.MonedaCara(AzarServicio.TimberoProbCara))
                {
                    JuegoServicio.SumarPuntos(
                        contexto.Mano, IdJugador.Humano, 1, OrigenPuntos.PasivaTimbero);
                    contexto.Mano.UltimoMensajeHabilidad =
                        $"Timbero: ¡Cara! (20%) +1 punto al iniciar la mano {contexto.Mano.NumeroDeMano}.";
                }
                else
                {
                    contexto.Mano.UltimoMensajeHabilidad =
                        $"Timbero: Cruz (80%). Sin bonus en la mano {contexto.Mano.NumeroDeMano}.";
                }

                return;
            }

            if (evento == EventoPartida.AntesDeSumarPuntos
                && datos is ModificadorPuntos mod
                && mod.Origen == OrigenPuntos.TrucoMano
                && estado.TimberoApuestaActiva)
            {
                if (mod.GanadorId == IdJugador.Humano)
                    mod.DuplicarPuntosGanador = true;
                else if (mod.GanadorId == IdJugador.Maquina)
                    mod.BonusAlRival = 2;

                estado.TimberoApuestaActiva = false;
                contexto.Mano.UltimoMensajeHabilidad = mod.GanadorId == IdJugador.Humano
                    ? "Timbero: ¡Ganaste la apuesta! Puntos de la mano duplicados."
                    : "Timbero: Perdiste la apuesta. La máquina recibe +2 puntos extra.";
            }
        }

        public override ResultadoActivarHabilidad? IntentarActivar(
            ContextoPartida contexto,
            SolicitudActivarHabilidad solicitud)
        {
            var estado = contexto.EstadoDe(IdJugador.Humano, ClaseHeroe.Timbero);
            if (!ReglasHabilidadActiva.ValidarActivaBase(contexto, estado, out var error))
                return ResultadoActivarHabilidad.Error(error!);

            if (!ReglasHabilidadActiva.EsInicioDeMano(contexto.Mano))
                return ResultadoActivarHabilidad.Error(
                    "La apuesta del Timbero solo puede activarse antes de jugar la primera carta.");

            estado.TimberoApuestaActiva = true;
            estado.ActivaUsadaEnEstaMano = true;
            estado.ActivaDisponible = false;
            contexto.Mano.UltimoMensajeHabilidad =
                "Timbero: Apuesta activa. Si ganás la mano duplicás puntos; si perdés, la máquina suma +2 extra.";

            return ResultadoActivarHabilidad.Ok(contexto.Mano.UltimoMensajeHabilidad);
        }
    }
}
