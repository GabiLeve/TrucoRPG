using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Heroes
{
    public sealed class ManipuladorHabilidad : HabilidadHeroeBase
    {
        public override ClaseHeroe Tipo => ClaseHeroe.Manipulador;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
            var estado = contexto.EstadoDe(IdJugador.Humano, ClaseHeroe.Manipulador);

            if (evento == EventoPartida.ManoIniciada)
            {
                ReglasHabilidadActiva.ReiniciarUsoEnMano(estado);
                estado.ActivaDisponible = contexto.Mano.NumeroDeMano % 3 == 1;
                return;
            }

            if (evento == EventoPartida.AntesDeRepartir)
            {
                contexto.Mano.RepartoContext ??= new RepartoContext();
                contexto.Mano.RepartoContext.ProbMejorarCartaPorJugador[IdJugador.Humano] =
                    RepartoContext.ManipuladorProbMejorarCarta;
                return;
            }

            if (evento != EventoPartida.DespuesDeRepartir)
                return;

            var suma = contexto.Jugador(IdJugador.Humano).Mano.Sum(c => c.ValorTruco);
            contexto.Mano.UltimoMensajeHabilidad =
                $"Pasiva: 10% de mejorar cada carta al repartir. Fuerza de tu mano: {suma}. " +
                (estado.ActivaDisponible
                    ? "Activa disponible: podés cambiar 1 carta."
                    : $"Activa en la mano {((contexto.Mano.NumeroDeMano / 3) * 3) + 1}.");
        }

        public override ResultadoActivarHabilidad? IntentarActivar(
            ContextoPartida contexto,
            SolicitudActivarHabilidad solicitud)
        {
            var estado = contexto.EstadoDe(IdJugador.Humano, ClaseHeroe.Manipulador);
            if (!ReglasHabilidadActiva.ValidarActivaBase(contexto, estado, out var error))
                return ResultadoActivarHabilidad.Error(error!);

            if (!ReglasHabilidadActiva.EsInicioDeMano(contexto.Mano))
                return ResultadoActivarHabilidad.Error(
                    "Solo podés cambiar cartas al inicio de la mano, antes de jugar.");

            if (!solicitud.NumeroCarta.HasValue || string.IsNullOrWhiteSpace(solicitud.PaloCarta))
                return ResultadoActivarHabilidad.Error("Indicá la carta a reemplazar (número y palo).");

            var mano = contexto.Mano;
            var humano = contexto.Jugador(IdJugador.Humano);
            var cartaDescartada = humano.Mano.FirstOrDefault(c =>
                c.Numero == solicitud.NumeroCarta.Value &&
                c.Palo.Equals(solicitud.PaloCarta, StringComparison.OrdinalIgnoreCase));

            if (cartaDescartada == null)
                return ResultadoActivarHabilidad.Error("La carta no está en tu mano.");

            var candidatas = mano.CartasRestantesMazo
                .Where(c => c.ValorTruco >= cartaDescartada.ValorTruco)
                .ToList();

            if (candidatas.Count == 0)
                return ResultadoActivarHabilidad.Error(
                    "No hay cartas en el mazo con valor igual o mayor al descartado.");

            var random = mano.RepartoContext?.Random ?? Random.Shared;
            var cartaNueva = candidatas[random.Next(candidatas.Count)];

            humano.Mano.Remove(cartaDescartada);
            mano.CartasRestantesMazo.Remove(cartaNueva);
            mano.CartasRestantesMazo.Add(cartaDescartada);
            humano.Mano.Add(cartaNueva);

            estado.ActivaUsadaEnEstaMano = true;
            estado.ActivaDisponible = false;

            var msg =
                $"Manipulador: cambiaste {cartaDescartada.Numero} de {cartaDescartada.Palo} " +
                $"por {cartaNueva.Numero} de {cartaNueva.Palo} (Truco {cartaNueva.ValorTruco}).";
            mano.UltimoMensajeHabilidad = msg;

            return ResultadoActivarHabilidad.Ok(msg);
        }
    }
}
