using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class MaquinaServicio
    {
        public static Carta ElegirCarta(List<Carta> manoMaquina, Carta? cartaHumano)
        {
            if (cartaHumano == null)
            {
                return manoMaquina
                    .OrderBy(c => c.ValorTruco)
                    .ElementAt(manoMaquina.Count / 2);
            }

            var cartasQueGanan = manoMaquina
                .Where(c => c.ValorTruco > cartaHumano.ValorTruco)
                .OrderBy(c => c.ValorTruco)
                .ToList();

            if (cartasQueGanan.Any())
                return cartasQueGanan.First();

            return manoMaquina.OrderBy(c => c.ValorTruco).First();
        }

        public static bool EsModoHistoriaPasoAPaso(ManoTruco mano) =>
            mano.Configuracion.Modo == ModoJuego.Historia;

        public static Truco1v1EventoMaquina? AvanzarUnPaso(ManoTruco mano)
        {
            if (!EsModoHistoriaPasoAPaso(mano)) return null;
            if (mano.SalpicaduraBloqueando || mano.TravesuraBloqueando
                || mano.RasgunoBloqueando || mano.AullidoBloqueando
                || mano.DestelloBloqueando || mano.EspejismoBloqueando
                || mano.MandingaEspejoBloqueando || mano.MandingaEnganoBloqueando
                || mano.MandingaMaldicionBloqueando) return null;
            if (mano.GanadorMano != null || mano.PartidaTerminada) return null;
            if (mano.EnvidoPendienteRespuestaHumano || mano.TrucoPendienteRespuestaHumano) return null;
            if (mano.CartaMaquinaEnMesa != null) return null;

            if (mano.CartaHumanoEnMesa != null)
                return CompletarBazaConCartaHumanoEnMesa(mano);

            if (IntentarCantarEnvidoIniciativa(mano))
                return new Truco1v1EventoMaquina("envido", "¡Envido!");

            if (mano.TurnoActual != "Maquina") return null;
            if (mano.Maquina.Mano.Count == 0) return null;

            bool teniaTrucoPendiente = mano.TrucoPendienteRespuestaHumano;
            AvanzarTurno(mano);

            if (mano.TrucoPendienteRespuestaHumano && !teniaTrucoPendiente)
                return new Truco1v1EventoMaquina("truco", "¡Truco!");
            if (mano.CartaMaquinaEnMesa != null)
                return new Truco1v1EventoMaquina("carta", "");

            return null;
        }

        private static bool IntentarCantarEnvidoIniciativa(ManoTruco mano)
        {
            if (!EnvidoServicio.PuedeCantarEnvido(mano))
                return false;

            bool cantaEnvido = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(
                mano.Maquina.Mano,
                mano.NivelMentiraEnvidoMaquina);

            if (!cantaEnvido) return false;

            mano.EnvidoCantado                = true;
            mano.CantorEnvido                 = "Maquina";
            mano.TipoEnvidoCantado            = "Envido";
            mano.EnvidoPendienteRespuestaHumano = true;
            mano.EstadoEnvido                 = "La máquina cantó Envido.";
            return true;
        }

        private static Truco1v1EventoMaquina CompletarBazaConCartaHumanoEnMesa(ManoTruco mano)
        {
            var cartaHumano = mano.CartaHumanoEnMesa!;
            mano.CartaHumanoEnMesa = null;

            var cartaMaquina = ElegirCarta(mano.Maquina.Mano, cartaHumano);
            mano.Maquina.Mano.Remove(cartaMaquina);
            mano.Maquina.Jugadas.Add(cartaMaquina);

            ResolverBazaJugada(mano, cartaHumano, cartaMaquina);
            return new Truco1v1EventoMaquina("carta", "");
        }

        public static void ResolverBazaJugada(ManoTruco mano, Carta cartaHumano, Carta cartaMaquina)
        {
            var ganadorBaza = JuegoServicio.ResolverBaza(cartaHumano, cartaMaquina);
            mano.Bazas.Add(new Baza
            {
                CartaJugador = cartaHumano,
                CartaMaquina = cartaMaquina,
                Ganador      = ganadorBaza
            });

            if (AullidoServicio.IntentarTrasPrimeraBaza(mano, ganadorBaza))
            {
                HabilidadesRivalOrquestador.ActualizarVista(mano);
                return;
            }

            mano.TurnoActual = ganadorBaza == "Parda" ? mano.ManoIniciadaPor : ganadorBaza;
            mano.GanadorMano = JuegoServicio.ResolverGanadorMano(mano.Bazas, mano.ManoIniciadaPor);

            if (mano.GanadorMano is null && mano.TurnoActual == IdJugador.Humano)
                DestelloServicio.EvaluarTurnoHumano(mano);

            if (mano.GanadorMano is "Humano" or "Maquina")
            {
                if (!mano.TrucoCantado)
                    mano.EstadoTruco = "No se cantó truco. La mano vale 1 punto.";

                int puntosMano = mano.PuntosTrucoMano > 0 ? mano.PuntosTrucoMano : 1;
                JuegoServicio.SumarPuntos(
                    mano, mano.GanadorMano, puntosMano, OrigenPuntos.TrucoMano, mano.CantorTruco);
                mano.TrucoResuelto = true;
                MandingaServicio.RegistrarFinMano(mano);
                HabilidadesRivalOrquestador.ActualizarVista(mano);
            }
            else if (!EsModoHistoriaPasoAPaso(mano))
            {
                AvanzarTurno(mano);
            }
            else if (mano.TurnoActual == IdJugador.Maquina)
            {
                HabilidadesTurnoMaquinaServicio.Notificar(mano);
            }
        }

        // ── Turno e iniciativa de la máquina (extraídos del Controller) ───────────
        public static void AvanzarTurnoSiNoEsHistoria(ManoTruco mano)
        {
            if (!EsModoHistoriaPasoAPaso(mano))
                AvanzarTurno(mano);
        }

        public static void ProcesarIniciativa(ManoTruco mano)
        {
            if (EnvidoServicio.PuedeCantarEnvido(mano))
            {
                bool cantaEnvido = IniciativaMaquinaEnvidoServicio.DebeCantarEnvido(
                    mano.Maquina.Mano,
                    mano.NivelMentiraEnvidoMaquina);

                if (cantaEnvido)
                {
                    mano.EnvidoCantado                = true;
                    mano.CantorEnvido                  = "Maquina";
                    mano.TipoEnvidoCantado             = "Envido";
                    mano.EnvidoPendienteRespuestaHumano = true;
                    mano.EstadoEnvido                  = "La máquina cantó Envido.";
                }
            }

            AvanzarTurno(mano);
        }

        public static void AvanzarTurno(ManoTruco mano)
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
                    mano.NivelMentiraTrucoMaquina);

                if (cantaTruco)
                {
                    mano.TrucoCantado                  = true;
                    mano.NivelTruco                    = 1;
                    mano.PuntosTrucoMano               = 2;
                    mano.TrucoPendienteRespuestaHumano = true;
                    mano.CantorTruco                   = "Maquina";
                    mano.EstadoTruco                   = "La máquina cantó Truco.";
                    return;
                }
            }

            var carta = ElegirCarta(mano.Maquina.Mano, null);
            mano.Maquina.Mano.Remove(carta);
            mano.Maquina.Jugadas.Add(carta);
            mano.CartaMaquinaEnMesa = carta;
            EspejismoServicio.IntentarAlJugarPrimeraCarta(mano);
            if (!mano.EspejismoBloqueando)
                DestelloServicio.EvaluarTurnoHumano(mano);
        }
    }
}
