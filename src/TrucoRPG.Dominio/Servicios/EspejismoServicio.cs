using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class EspejismoServicio
    {
        public const double ProbabilidadActivacion = 1.0;

        /// <summary>
        /// Pasiva Luz Mala: al mostrar la primera carta de baza 1 con iniciativa máquina,
        /// Siempre activa carta falsa visual (solo UI) cuando aplica.
        /// </summary>
        public static bool IntentarAlJugarPrimeraCarta(ManoTruco mano)
        {
            if (!EsLuzMalaHistoria(mano))
                return false;
            if (mano.ManoIniciadaPor != IdJugador.Maquina)
                return false;
            if (mano.Bazas.Count != 0 || mano.GanadorMano != null)
                return false;
            if (mano.CartaMaquinaEnMesa is null)
                return false;
            if (mano.EspejismoActivo)
                return false;

            if (!AzarServicio.TirarProbabilidad(ProbabilidadActivacion))
                return false;

            var real = mano.CartaMaquinaEnMesa;
            mano.EspejismoCartaFalsa = GenerarCartaFalsaVisual(mano, real);
            mano.EspejismoMostrarFakePrimero = AzarServicio.MonedaCara();
            mano.EspejismoActivo = true;
            mano.EspejismoBloqueando = true;
            mano.EspejismoAlternando = false;
            mano.EspejismoUsadoEnMano = true;
            mano.DestelloPendiente = false;
            mano.DestelloBazaObjetivo = 0;
            mano.DestelloBloqueando = false;
            mano.UltimoMensajeHabilidadRival =
                "¡Espejismo! La Luz Mala te muestra una carta que no es la que jugó...";
            HabilidadesRivalOrquestador.ActualizarVista(mano);
            return true;
        }

        public static void ConfirmarOverlay(ManoTruco mano)
        {
            if (!mano.EspejismoBloqueando)
                return;

            mano.EspejismoBloqueando = false;
            mano.EspejismoAlternando = true;
            mano.UltimoMensajeHabilidadRival =
                "¡Espejismo! No confíes en lo que ves en la mesa.";
            HabilidadesRivalOrquestador.ActualizarVista(mano);
        }

        public static void Finalizar(ManoTruco mano)
        {
            if (!mano.EspejismoActivo)
                return;

            mano.EspejismoActivo = false;
            mano.EspejismoBloqueando = false;
            mano.EspejismoAlternando = false;
            mano.EspejismoMostrarFakePrimero = false;
            mano.EspejismoCartaFalsa = null;
            HabilidadesRivalOrquestador.ActualizarVista(mano);
        }

        private static readonly Lazy<List<Carta>> MazoCompleto = new(MazoServicio.CrearMazo);

        private static Carta GenerarCartaFalsaVisual(ManoTruco mano, Carta real)
        {
            var ocupadas = CartasEnJuegoServicio.Obtener(mano, null);
            var revelada = mano.EstadoHabilidades.Obtener(IdJugador.Humano)?.CartaReveladaRival;
            if (revelada is not null)
                ocupadas.Add(CartasEnJuegoServicio.Clave(revelada));

            var candidatas = MazoCompleto.Value
                .Where(c => !ocupadas.Contains(CartasEnJuegoServicio.Clave(c)))
                .Where(c => !CartasEnJuegoServicio.Clave(c).Equals(CartasEnJuegoServicio.Clave(real)))
                .ToList();

            if (candidatas.Count == 0)
                throw new InvalidOperationException("No hay cartas válidas para el Espejismo.");

            var elegida = candidatas[Random.Shared.Next(candidatas.Count)];
            return new Carta
            {
                Numero = elegida.Numero,
                Palo = elegida.Palo,
                ValorTruco = elegida.ValorTruco
            };
        }

        private static bool EsLuzMalaHistoria(ManoTruco mano) =>
            MaquinaServicio.EsModoHistoriaPasoAPaso(mano)
            && mano.Configuracion.HabilidadesRivalActivas
            && mano.Configuracion.RivalDeLaMaquina == ClaseRival.LuzMala;
    }
}
