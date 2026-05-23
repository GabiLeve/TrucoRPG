using TrucoRPG.Dominio.Entities;

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

        // ── Turno e iniciativa de la máquina (extraídos del Controller) ───────────
        public static void ProcesarIniciativa(ManoTruco mano)
        {
            if (!mano.EnvidoCantado && !mano.EnvidoResuelto && mano.Bazas.Count == 0)
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
        }
    }
}
