using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class DestelloServicio
    {
        public static void EvaluarTurnoHumano(ManoTruco mano)
        {
            if (!mano.Configuracion.HabilidadesRivalActivas)
                return;
            if (mano.Configuracion.RivalDeLaMaquina != ClaseRival.LuzMala)
                return;
            if (mano.EspejismoUsadoEnMano)
                return;
            if (mano.DestelloBloqueando)
                return;
            if (!EsMomentoDeJugarHumano(mano))
                return;

            int bazaActual = mano.Bazas.Count + 1;
            if (bazaActual > 2)
                return;

            ActivarCicloDestelloSiCorresponde(mano);

            if (!mano.DestelloPendiente)
                return;
            if (bazaActual != mano.DestelloBazaObjetivo)
                return;

            mano.DestelloBloqueando = true;
            mano.UltimoMensajeHabilidadRival =
                "¡Destello! La Luz Mala te confunde y vas a jugar una carta al azar...";
            HabilidadesRivalOrquestador.ActualizarVista(mano);
        }

        /// <summary>
        /// Cada 2 jugadas del humano en bazas 1–2 activa un ciclo de Destello.
        /// La baza objetivo (1 o 2) se elige al azar con 50% cada una.
        /// </summary>
        private static void ActivarCicloDestelloSiCorresponde(ManoTruco mano)
        {
            if (mano.DestelloPendiente)
                return;
            if (mano.ContadorTurnosHumanoPartida % 2 != 0)
                return;

            mano.DestelloPendiente = true;
            mano.DestelloBazaObjetivo = Random.Shared.Next(1, 3);
        }

        public static void RegistrarJugadaHumano(ManoTruco mano, int bazaAlJugar)
        {
            if (bazaAlJugar <= 2)
                mano.ContadorTurnosHumanoPartida++;
        }

        public static void CompletarDestello(ManoTruco mano)
        {
            mano.DestelloPendiente = false;
            mano.DestelloBazaObjetivo = 0;
        }

        public static void JugarCartaAleatoria(ManoTruco mano)
        {
            if (mano.Humano.Mano.Count == 0)
                throw new InvalidOperationException("No tenés cartas para jugar.");

            int bazaAlJugar = mano.Bazas.Count + 1;
            var carta = mano.Humano.Mano[Random.Shared.Next(mano.Humano.Mano.Count)];
            mano.Humano.Mano.Remove(carta);

            if (mano.CartaMaquinaEnMesa != null)
            {
                mano.Humano.Jugadas.Add(carta);
                var cartaMaquina = mano.CartaMaquinaEnMesa;
                mano.CartaMaquinaEnMesa = null;
                MaquinaServicio.ResolverBazaJugada(mano, carta, cartaMaquina);
            }
            else if (MaquinaServicio.EsModoHistoriaPasoAPaso(mano))
            {
                mano.CartaHumanoEnMesa = carta;
                mano.TurnoActual = IdJugador.Maquina;
                RemolinoServicio.IntentarEnPrimeraBaza(mano, carta);
                HabilidadesRivalOrquestador.ActualizarVista(mano);
                HabilidadesTurnoMaquinaServicio.Notificar(mano);
            }
            else
            {
                mano.Humano.Jugadas.Add(carta);
                var cartaMaquina = MaquinaServicio.ElegirCarta(mano.Maquina.Mano, carta);
                mano.Maquina.Mano.Remove(cartaMaquina);
                mano.Maquina.Jugadas.Add(cartaMaquina);
                MaquinaServicio.ResolverBazaJugada(mano, carta, cartaMaquina);
            }

            RegistrarJugadaHumano(mano, bazaAlJugar);
            CompletarDestello(mano);
            mano.UltimoMensajeHabilidadRival =
                $"¡Destello! La Luz Mala te confundió: jugaste {carta.Numero} de {carta.Palo} al azar.";
        }

        private static bool EsMomentoDeJugarHumano(ManoTruco mano)
        {
            if (mano.GanadorMano != null || mano.PartidaTerminada)
                return false;
            if (mano.EnvidoPendienteRespuestaHumano || mano.TrucoPendienteRespuestaHumano)
                return false;
            if (mano.SalpicaduraBloqueando || mano.TravesuraBloqueando
                || mano.RasgunoBloqueando || mano.AullidoBloqueando)
                return false;
            if (mano.EspejismoBloqueando)
                return false;
            if (mano.CartaHumanoEnMesa != null)
                return false;
            if (mano.Humano.Mano.Count == 0)
                return false;

            return mano.TurnoActual == IdJugador.Humano || mano.CartaMaquinaEnMesa != null;
        }
    }
}
