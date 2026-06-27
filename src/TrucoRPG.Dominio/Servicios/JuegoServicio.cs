using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class JuegoServicio
    {
        public static string ResolverBaza(Carta cartaHumano, Carta cartaMaquina)
        {
            if (cartaHumano.ValorTruco > cartaMaquina.ValorTruco)
                return "Humano";

            if (cartaMaquina.ValorTruco > cartaHumano.ValorTruco)
                return "Maquina";

            return "Parda";
        }

        public static string? ResolverGanadorMano(List<Baza> bazas, string jugadorMano)
        {
            if (bazas.Count == 0)
                return null;

            string? ganadorPrimera = ObtenerGanadorBaza(bazas, 0);
            string? ganadorSegunda = ObtenerGanadorBaza(bazas, 1);
            string? ganadorTercera = ObtenerGanadorBaza(bazas, 2);

            if (ganadorPrimera is "Humano" or "Maquina")
            {
                if (ganadorSegunda == ganadorPrimera)
                    return ganadorPrimera;

                if (ganadorSegunda == "Parda")
                    return ganadorPrimera;
            }

            if (ganadorPrimera == "Parda")
            {
                if (ganadorSegunda is "Humano" or "Maquina")
                {
                    // Parda en primera → gana quien gana la segunda (la carta más alta decide)
                    return ganadorSegunda;
                }

                if (ganadorSegunda == "Parda")
                {
                    // Parda en primera y segunda → va a la tercera
                    if (ganadorTercera == null)
                        return null;

                    if (ganadorTercera == "Parda")
                        return jugadorMano; // Todas pardas → gana el jugador "mano"

                    return ganadorTercera;
                }
            }

            if (ganadorPrimera is "Humano" or "Maquina")
            {
                if (ganadorSegunda is "Humano" or "Maquina" && ganadorSegunda != ganadorPrimera)
                {
                    if (ganadorTercera == null)
                        return null;

                    if (ganadorTercera == "Parda")
                        return ganadorPrimera;

                    return ganadorTercera;
                }
            }

            return null;
        }

        public static void SumarPuntos(
            ManoTruco mano,
            string? ganador,
            int puntos,
            string? origen = null,
            string? cantorId = null)
        {
            if (ganador is not (IdJugador.Humano or IdJugador.Maquina))
                return;

            var modificador = new ModificadorPuntos();
            modificador.AplicarBase(puntos, ganador, origen, cantorId);

            if (mano.Configuracion.HabilidadesActivas)
                HabilidadesOrquestador.Disparar(mano, EventoPartida.AntesDeSumarPuntos, modificador);

            AplicarPuntos(mano, ganador, modificador.PuntosParaGanador());

            if (modificador.BonusAlRival > 0)
            {
                var rival = modificador.RivalDe(ganador);
                if (rival != null)
                    AplicarPuntos(mano, rival, modificador.BonusAlRival);
            }

            if (mano.GanadorMano != null
                && origen is OrigenPuntos.TrucoMano or OrigenPuntos.TrucoRechazo or OrigenPuntos.AullidoLobizon)
            {
                PomberitoPasivaServicio.AplicarSiManoSilenciosa(mano);
            }
        }

        private static void AplicarPuntos(ManoTruco mano, string ganador, int puntos)
        {
            if (puntos <= 0)
                return;

            if (ganador == IdJugador.Humano)
                mano.PuntosHumano += puntos;
            else if (ganador == IdJugador.Maquina)
                mano.PuntosMaquina += puntos;

            EvaluarFinPartida(mano);
        }

        private static void EvaluarFinPartida(ManoTruco mano)
        {
            if (mano.PuntosHumano >= 30)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida   = IdJugador.Humano;
            }
            else if (mano.PuntosMaquina >= 30)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida   = IdJugador.Maquina;
            }
        }

        private static string? ObtenerGanadorBaza(List<Baza> bazas, int index)
        {
            return bazas.Count > index ? bazas[index].Ganador : null;
        }
    }
}
