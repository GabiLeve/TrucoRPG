using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades.Heroes
{
    public sealed class ManipuladorHabilidad : HabilidadHeroeBase
    {
        public override ClaseHeroe Tipo => ClaseHeroe.Manipulador;

        public override void OnEvento(ContextoPartida contexto, EventoPartida evento, object? datos = null)
        {
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
                $"Pasiva activa: 10% de mejorar cada carta al repartir. " +
                $"Fuerza de tu mano: {suma} (suma ValorTruco de tus 3 cartas). " +
                $"Repartí de nuevo (nueva mano) para comparar.";
        }
    }
}
