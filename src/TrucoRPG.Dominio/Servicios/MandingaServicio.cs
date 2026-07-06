using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class MandingaServicio
    {
        public const int UmbralFase2 = 10;
        public const int UmbralFase3 = 20;

        private static readonly Lazy<List<Carta>> MazoCompleto = new(MazoServicio.CrearMazo);

        public static bool EsMandingaHistoria(ManoTruco mano) =>
            MaquinaServicio.EsModoHistoriaPasoAPaso(mano)
            && mano.Configuracion.HabilidadesRivalActivas
            && mano.Configuracion.RivalDeLaMaquina == ClaseRival.Mandinga;

        public static void OnManoIniciada(ManoTruco mano)
        {
            if (!EsMandingaHistoria(mano))
                return;

            ActualizarDesbloqueosFases(mano);
            ReiniciarAcumuladoresMano(mano);

            mano.MandingaEnganoProgramadoEstaMano = DebeActivarEngano(mano);
            mano.MandingaMaldicionProgramadaEstaMano = mano.NumeroDeMano % 2 == 1;

            IniciarColaOverlays(mano);
        }

        public static void ConfirmarEspejo(ManoTruco mano)
        {
            if (!mano.MandingaEspejoBloqueando)
                return;

            mano.MandingaEspejoBloqueando = false;
            mano.UltimoMensajeHabilidadRival =
                "¡El Espejo! El Mandinga robó tu mejor carta de la mano anterior.";
            AvanzarColaOverlays(mano);
        }

        public static void ConfirmarEngano(ManoTruco mano)
        {
            if (!mano.MandingaEnganoBloqueando)
                return;

            MezclarYOcultarManoHumano(mano);
            mano.MandingaEnganoBloqueando = false;
            mano.MandingaEnganoManoOculta = true;
            mano.UltimoMensajeHabilidadRival =
                "¡El Engaño! El Mandinga te dio vuelta las cartas y las mezcló.";
            AvanzarColaOverlays(mano);
        }

        public static void ConfirmarMaldicion(ManoTruco mano)
        {
            if (!mano.MandingaMaldicionBloqueando)
                return;

            mano.MandingaMaldicionBloqueando = false;
            mano.MandingaMaldicionActivaEnMano = true;
            mano.UltimoMensajeHabilidadRival =
                "¡El Pacto! La mesa está maldita: el Diablo duplica si gana; vos perdés 1 si ganás.";
        }

        public static bool DebeAcumularPuntos(ManoTruco mano) =>
            EsMandingaHistoria(mano) && mano.MandingaMaldicionActivaEnMano;

        public static void AcumularPuntos(ManoTruco mano, string ganador, int puntos)
        {
            if (puntos <= 0)
                return;

            if (ganador == IdJugador.Humano)
                mano.PuntosHumanoAcumuladosMano += puntos;
            else if (ganador == IdJugador.Maquina)
                mano.PuntosMaquinaAcumuladosMano += puntos;
        }

        public static void LiquidarPuntosMaldicion(ManoTruco mano)
        {
            if (!DebeAcumularPuntos(mano))
                return;

            int hum = mano.PuntosHumanoAcumuladosMano;
            int maq = mano.PuntosMaquinaAcumuladosMano;

            if (mano.GanadorMano == IdJugador.Humano && hum > 0)
                AplicarPuntosDirecto(mano, IdJugador.Humano, Math.Max(0, hum - 1));

            if (mano.GanadorMano == IdJugador.Maquina && maq > 0)
                AplicarPuntosDirecto(mano, IdJugador.Maquina, maq * 2);

            if (mano.GanadorMano == IdJugador.Humano && maq > 0)
                AplicarPuntosDirecto(mano, IdJugador.Maquina, maq);

            if (mano.GanadorMano == IdJugador.Maquina && hum > 0)
                AplicarPuntosDirecto(mano, IdJugador.Humano, hum);

            ReiniciarAcumuladoresMano(mano);
            mano.MandingaMaldicionActivaEnMano = false;
        }

        public static void RegistrarFinMano(ManoTruco mano)
        {
            if (!EsMandingaHistoria(mano))
                return;

            mano.MandingaJugadasHumanoManoAnterior = mano.Bazas
                .Where(b => b.CartaJugador is not null)
                .Select(b => ClonarCarta(b.CartaJugador!))
                .ToList();
        }

        public static void TrasladarEstadoPartida(ManoTruco? anterior, ManoTruco nueva)
        {
            if (anterior is null)
                return;

            nueva.MandingaFase2Desbloqueada = anterior.MandingaFase2Desbloqueada;
            nueva.MandingaFase3Desbloqueada = anterior.MandingaFase3Desbloqueada;
            nueva.MandingaPrimeraManoEngano = anterior.MandingaPrimeraManoEngano;
            nueva.MandingaJugadasHumanoManoAnterior = anterior.MandingaJugadasHumanoManoAnterior
                .Select(ClonarCarta)
                .ToList();
        }

        public static void SincronizarDesbloqueosFases(ManoTruco mano)
        {
            if (mano.PuntosHumano >= UmbralFase2)
                mano.MandingaFase2Desbloqueada = true;
            if (mano.PuntosHumano >= UmbralFase3)
                mano.MandingaFase3Desbloqueada = true;
        }

        private static void ActualizarDesbloqueosFases(ManoTruco mano) =>
            SincronizarDesbloqueosFases(mano);

        private static void ReiniciarAcumuladoresMano(ManoTruco mano)
        {
            mano.PuntosHumanoAcumuladosMano = 0;
            mano.PuntosMaquinaAcumuladosMano = 0;
            mano.MandingaMaldicionActivaEnMano = false;
            mano.MandingaEnganoManoOculta = false;
            mano.MandingaEspejoBloqueando = false;
            mano.MandingaEnganoBloqueando = false;
            mano.MandingaMaldicionBloqueando = false;
        }

        private static void IniciarColaOverlays(ManoTruco mano)
        {
            if (DebeActivarEspejo(mano) && IntentarAplicarEspejo(mano))
            {
                mano.MandingaEspejoBloqueando = true;
                mano.UltimoMensajeHabilidadRival =
                    "¡El Espejo! El Mandinga va a copiar tu última carta más alta jugada...";
                HabilidadesRivalOrquestador.ActualizarVista(mano);
                return;
            }

            AvanzarColaOverlays(mano);
        }

        private static void AvanzarColaOverlays(ManoTruco mano)
        {
            if (mano.MandingaEnganoProgramadoEstaMano && !mano.MandingaEnganoManoOculta)
            {
                mano.MandingaEnganoBloqueando = true;
                mano.UltimoMensajeHabilidadRival =
                    "¡El Engaño! El Mandinga te deja ver tus cartas un instante...";
                HabilidadesRivalOrquestador.ActualizarVista(mano);
                return;
            }

            if (mano.MandingaMaldicionProgramadaEstaMano)
            {
                mano.MandingaMaldicionBloqueando = true;
                mano.UltimoMensajeHabilidadRival =
                    "¡El Pacto! El Mandinga maldice la mesa...";
                HabilidadesRivalOrquestador.ActualizarVista(mano);
            }
        }

        private static bool DebeActivarEngano(ManoTruco mano)
        {
            if (!mano.MandingaFase2Desbloqueada)
                return false;

            if (mano.MandingaPrimeraManoEngano == 0)
            {
                if (mano.PuntosHumano >= UmbralFase2)
                {
                    mano.MandingaPrimeraManoEngano = mano.NumeroDeMano;
                    return true;
                }

                return false;
            }

            return (mano.NumeroDeMano - mano.MandingaPrimeraManoEngano) % 3 == 0;
        }

        private static bool DebeActivarEspejo(ManoTruco mano) =>
            mano.MandingaFase3Desbloqueada
            && mano.MandingaJugadasHumanoManoAnterior.Count > 0;

        private static bool IntentarAplicarEspejo(ManoTruco mano)
        {
            var candidatas = ObtenerCartasMasAltas(mano.MandingaJugadasHumanoManoAnterior);
            if (candidatas.Count == 0)
                return false;

            var copia = ElegirCartaCopiable(candidatas, mano);
            if (copia is null)
                return false;

            ReemplazarCartaMasBajaMaquina(mano, copia);
            return true;
        }

        private static List<Carta> ObtenerCartasMasAltas(List<Carta> jugadas)
        {
            if (jugadas.Count == 0)
                return [];

            int max = jugadas.Max(c => c.ValorTruco);
            return jugadas.Where(c => c.ValorTruco == max).ToList();
        }

        private static Carta? ElegirCartaCopiable(List<Carta> candidatas, ManoTruco mano)
        {
            var barajadas = candidatas.OrderBy(_ => Random.Shared.Next()).ToList();
            foreach (var candidata in barajadas)
            {
                if (!EstaEnJuego(mano, candidata))
                    return ClonarCarta(candidata);
            }

            var primera = barajadas[0];
            return ClonarCarta(primera);
        }

        private static void ReemplazarCartaMasBajaMaquina(ManoTruco mano, Carta copia)
        {
            var manoMaquina = mano.Maquina.Mano;
            if (manoMaquina.Count == 0)
                return;

            int idx = 0;
            int minValor = manoMaquina[0].ValorTruco;
            for (int i = 1; i < manoMaquina.Count; i++)
            {
                if (manoMaquina[i].ValorTruco < minValor)
                {
                    minValor = manoMaquina[i].ValorTruco;
                    idx = i;
                }
            }

            manoMaquina[idx].Numero = copia.Numero;
            manoMaquina[idx].Palo = copia.Palo;
            manoMaquina[idx].ValorTruco = copia.ValorTruco;
        }

        private static void MezclarYOcultarManoHumano(ManoTruco mano)
        {
            var manoHumano = mano.Humano.Mano;
            if (manoHumano.Count <= 1)
                return;

            var mezclada = manoHumano.OrderBy(_ => Random.Shared.Next()).ToList();
            manoHumano.Clear();
            manoHumano.AddRange(mezclada);
        }

        private static bool EstaEnJuego(ManoTruco mano, Carta carta)
        {
            if (mano.Humano.Mano.Any(c => EsMismaCarta(c, carta)))
                return true;
            if (mano.Maquina.Mano.Any(c => EsMismaCarta(c, carta)))
                return true;
            if (mano.CartaHumanoEnMesa is not null && EsMismaCarta(mano.CartaHumanoEnMesa, carta))
                return true;
            if (mano.CartaMaquinaEnMesa is not null && EsMismaCarta(mano.CartaMaquinaEnMesa, carta))
                return true;

            return mano.Bazas.Any(b =>
                (b.CartaJugador is not null && EsMismaCarta(b.CartaJugador, carta))
                || (b.CartaMaquina is not null && EsMismaCarta(b.CartaMaquina, carta)));
        }

        private static bool EsMismaCarta(Carta a, Carta b) =>
            a.Numero == b.Numero
            && a.Palo.Equals(b.Palo, StringComparison.OrdinalIgnoreCase);

        private static Carta ClonarCarta(Carta c) => new()
        {
            Numero = c.Numero,
            Palo = c.Palo,
            ValorTruco = c.ValorTruco
        };

        private static void AplicarPuntosDirecto(ManoTruco mano, string ganador, int puntos)
        {
            if (puntos <= 0)
                return;

            if (ganador == IdJugador.Humano)
                mano.PuntosHumano += puntos;
            else
                mano.PuntosMaquina += puntos;

            if (mano.PuntosHumano >= 30)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida = IdJugador.Humano;
            }
            else if (mano.PuntosMaquina >= 30)
            {
                mano.PartidaTerminada = true;
                mano.GanadorPartida = IdJugador.Maquina;
            }
        }
    }
}
