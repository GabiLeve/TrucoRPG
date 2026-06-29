using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.Habilidades.Heroes
{
    public sealed class MentirosoHabilidad : HabilidadHeroeBase
    {
        public override ClaseHeroe Tipo => ClaseHeroe.Mentiroso;

        /// <summary>Manos de espera tras usar la activa (recién entonces empieza a contar).</summary>
        private const int CooldownManos = 2;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            var estado = contexto.EstadoDe(IdJugador.Humano, ClaseHeroe.Mentiroso);

            if (evento == EventoPartida.ManoIniciada)
            {
                ReglasHabilidadActiva.ReiniciarUsoEnMano(estado);
                ReglasHabilidadActiva.ActualizarDisponibilidadPorCooldown(estado, CooldownManos);
            }
        }

        public override ResultadoActivarHabilidad? IntentarActivar(
            ContextoPartida contexto,
            SolicitudActivarHabilidad solicitud)
        {
            var estado = contexto.EstadoDe(IdJugador.Humano, ClaseHeroe.Mentiroso);
            if (!ReglasHabilidadActiva.ValidarActivaBase(contexto, estado, out var error))
                return ResultadoActivarHabilidad.Error(error!);

            if (!ReglasHabilidadActiva.EsInicioDeMano(contexto.Mano))
                return ResultadoActivarHabilidad.Error(
                    "Solo podés revelar una carta al inicio de la mano.");

            var rival = contexto.Jugador(IdJugador.Maquina);
            if (rival.Mano.Count == 0)
                return ResultadoActivarHabilidad.Error("El rival no tiene cartas en la mano.");

            var random = contexto.Mano.RepartoContext?.Random ?? Random.Shared;
            var revelada = rival.Mano[random.Next(rival.Mano.Count)];

            estado.CartaReveladaRival = new Carta
            {
                Numero = revelada.Numero,
                Palo = revelada.Palo,
                ValorTruco = revelada.ValorTruco
            };
            ReglasHabilidadActiva.RegistrarUsoActiva(estado);

            var msg =
                $"Mentiroso: revelaste {revelada.Numero} de {revelada.Palo} del rival.";
            contexto.Mano.UltimoMensajeHabilidad = msg;

            return ResultadoActivarHabilidad.Ok(msg);
        }
    }
}
