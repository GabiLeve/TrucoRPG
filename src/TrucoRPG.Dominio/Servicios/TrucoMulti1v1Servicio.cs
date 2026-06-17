using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Reglas de negocio del TRUCO 1v1 MULTIJUGADOR (dos humanos reales).
    /// Opera sobre <see cref="EstadoTrucoMulti1v1"/> y lo muta. Cada método devuelve
    /// true si la acción fue válida (el Hub solo traduce conexiones a roles y hace
    /// el broadcast; toda la regla vive acá, en el Dominio).
    /// Roles internos heredados de <see cref="ManoTruco"/>: J1 = "Humano", J2 = "Maquina".
    /// </summary>
    public static class TrucoMulti1v1Servicio
    {
        private static string Rol(bool esJ1) => esJ1 ? "Humano" : "Maquina";

        private static string Nombre(string? rol) => rol == "Humano" ? "Jugador 1" : "Jugador 2";

        // ─────────────────────────────────────────────────────────────
        //  Nueva mano / partida
        // ─────────────────────────────────────────────────────────────
        public static void IniciarNuevaMano(EstadoTrucoMulti1v1 estado, bool esPrimeraPartida)
        {
            int numMano = esPrimeraPartida ? 1 : estado.Mano.NumeroDeMano + 1;
            int ptsH    = esPrimeraPartida ? 0 : estado.Mano.PuntosHumano;
            int ptsM    = esPrimeraPartida ? 0 : estado.Mano.PuntosMaquina;

            var mano = PartidaServicio.CrearManoNueva(numMano, ptsH, ptsM);
            mano.Humano.Nombre   = "Jugador 1";
            mano.Maquina.Nombre  = "Jugador 2";
            mano.PuntosTrucoMano = 1;

            estado.Mano                       = mano;
            estado.CartaPendienteJ1           = null;
            estado.TrucoPendienteRespuestaJ2  = false;
            estado.EnvidoPendienteRespuestaJ2 = false;
            estado.PuntosEnvidoEnJuego        = 0;
            estado.PuntosEnvidoNoQuiero       = 1;
        }

        // ─────────────────────────────────────────────────────────────
        //  Jugar carta
        // ─────────────────────────────────────────────────────────────
        public static bool JugarCarta(EstadoTrucoMulti1v1 estado, bool esJ1, int numero, string palo)
        {
            var mano = estado.Mano;
            if (mano.GanadorMano != null || mano.PartidaTerminada) return false;
            if (mano.EnvidoPendienteRespuestaHumano || estado.EnvidoPendienteRespuestaJ2) return false;
            if (mano.TrucoPendienteRespuestaHumano || estado.TrucoPendienteRespuestaJ2) return false;

            string rol = Rol(esJ1);
            if (mano.TurnoActual != rol) return false;

            var manoJugador = esJ1 ? mano.Humano.Mano : mano.Maquina.Mano;
            var carta = manoJugador.FirstOrDefault(c =>
                c.Numero == numero && c.Palo.Equals(palo, StringComparison.OrdinalIgnoreCase));
            if (carta == null) return false;

            manoJugador.Remove(carta);
            (esJ1 ? mano.Humano.Jugadas : mano.Maquina.Jugadas).Add(carta);

            if (esJ1)
            {
                if (mano.CartaMaquinaEnMesa != null)
                {
                    // J2 abrió la baza (su carta ya estaba en la mesa).
                    var cartaJ2 = mano.CartaMaquinaEnMesa;
                    mano.CartaMaquinaEnMesa = null;
                    ResolverBaza(mano, carta, cartaJ2, abridorBaza: "Maquina");
                }
                else
                {
                    estado.CartaPendienteJ1 = carta;
                    mano.TurnoActual = "Maquina";
                }
            }
            else
            {
                if (estado.CartaPendienteJ1 != null)
                {
                    // J1 abrió la baza (su carta ya estaba en la mesa).
                    var cartaJ1 = estado.CartaPendienteJ1;
                    estado.CartaPendienteJ1 = null;
                    ResolverBaza(mano, cartaJ1, carta, abridorBaza: "Humano");
                }
                else
                {
                    mano.CartaMaquinaEnMesa = carta;
                    mano.TurnoActual = "Humano";
                }
            }

            return true;
        }

        private static void ResolverBaza(ManoTruco mano, Carta cartaJ1, Carta cartaJ2, string abridorBaza)
        {
            var ganador = JuegoServicio.ResolverBaza(cartaJ1, cartaJ2);
            mano.Bazas.Add(new Baza { CartaJugador = cartaJ1, CartaMaquina = cartaJ2, Ganador = ganador });
            // Si la baza fue parda, vuelve a salir quien ABRIÓ esa baza (no siempre el mano
            // de la mano: en la 2da/3ra baza puede haber salido el que ganó la anterior).
            mano.TurnoActual = ganador == "Parda" ? abridorBaza : ganador;

            var ganadorMano = JuegoServicio.ResolverGanadorMano(mano.Bazas, mano.ManoIniciadaPor);
            if (ganadorMano != null)
            {
                mano.GanadorMano   = ganadorMano;
                int pts            = mano.PuntosTrucoMano > 0 ? mano.PuntosTrucoMano : 1;
                JuegoServicio.SumarPuntos(mano, ganadorMano, pts);
                mano.TrucoResuelto = true;
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Envido
        // ─────────────────────────────────────────────────────────────
        public static bool CantarEnvido(EstadoTrucoMulti1v1 estado, bool esJ1, string tipo)
        {
            var mano = estado.Mano;
            if (mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (mano.Bazas.Count > 0) return false;
            if (mano.PartidaTerminada || mano.GanadorMano != null) return false;

            mano.EnvidoCantado     = true;
            mano.CantorEnvido      = Rol(esJ1);
            mano.TipoEnvidoCantado = EnvidoServicio.NormalizarTipo(tipo);

            // Arranca la cadena de apuestas del envido.
            estado.PuntosEnvidoEnJuego  = EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado);
            estado.PuntosEnvidoNoQuiero = 1;

            mano.EnvidoPendienteRespuestaHumano = !esJ1;
            estado.EnvidoPendienteRespuestaJ2   = esJ1;
            mano.EstadoEnvido = $"{Nombre(mano.CantorEnvido)} cantó {tipo}.";
            return true;
        }

        public static bool ResponderEnvido(EstadoTrucoMulti1v1 estado, bool esJ1, bool aceptar)
        {
            var mano = estado.Mano;
            if (!mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (esJ1 && !mano.EnvidoPendienteRespuestaHumano) return false;
            if (!esJ1 && !estado.EnvidoPendienteRespuestaJ2) return false;

            mano.EnvidoPendienteRespuestaHumano = false;
            estado.EnvidoPendienteRespuestaJ2   = false;

            if (!aceptar)
            {
                // Rechazar paga lo apostado ANTES de la última suba (Envido→1, Envido+Real→2, etc.)
                int ptsNoQuiero     = Math.Max(1, estado.PuntosEnvidoNoQuiero);
                mano.EnvidoResuelto = true;
                mano.GanadorEnvido  = mano.CantorEnvido;
                mano.PuntosEnvido   = ptsNoQuiero;
                mano.EstadoEnvido   = $"No quiso. {Nombre(mano.CantorEnvido)} gana {ptsNoQuiero} punto(s) de envido.";
                JuegoServicio.SumarPuntos(mano, mano.CantorEnvido!, ptsNoQuiero);
            }
            else
            {
                // Los cantos acumulados de la cadena (Envido + Real Envido = 5, etc.)
                int pts = estado.PuntosEnvidoEnJuego > 0
                    ? estado.PuntosEnvidoEnJuego
                    : EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado);

                // El tanto se cuenta con las cartas ORIGINALES (mano + jugadas): si alguien ya
                // tiró una carta en la primera vuelta, igual cuenta para el envido.
                mano.TantoHumano  = EnvidoServicio.CalcularTantoOriginal(mano.Humano);
                mano.TantoMaquina = EnvidoServicio.CalcularTantoOriginal(mano.Maquina);

                if (mano.TantoHumano > mano.TantoMaquina)       mano.GanadorEnvido = "Humano";
                else if (mano.TantoMaquina > mano.TantoHumano)  mano.GanadorEnvido = "Maquina";
                else                                             mano.GanadorEnvido = mano.ManoIniciadaPor;

                // La Falta vale lo que le falta al que VA GANANDO la partida para llegar a 30.
                if (mano.TipoEnvidoCantado == "FaltaEnvido")
                    pts = EnvidoServicio.CalcularPuntosFalta(Math.Max(mano.PuntosHumano, mano.PuntosMaquina));

                mano.PuntosEnvido   = pts;
                mano.EnvidoResuelto = true;
                mano.EstadoEnvido   = $"Quiso. J1 tiene {mano.TantoHumano}, J2 tiene {mano.TantoMaquina}. " +
                                      $"Gana {Nombre(mano.GanadorEnvido)} ({pts} pt).";
                JuegoServicio.SumarPuntos(mano, mano.GanadorEnvido, pts);
            }

            return true;
        }

        /// <summary>"Son buenas": el que responde reconoce que pierde el envido (equivale a querer y perder).</summary>
        public static bool SonBuenas(EstadoTrucoMulti1v1 estado, bool esJ1)
        {
            var mano = estado.Mano;
            if (!mano.EnvidoCantado || mano.EnvidoResuelto) return false;
            if (esJ1 && !mano.EnvidoPendienteRespuestaHumano) return false;
            if (!esJ1 && !estado.EnvidoPendienteRespuestaJ2) return false;

            // El declarante pierde → el cantor gana lo acumulado de la cadena.
            mano.EnvidoPendienteRespuestaHumano = false;
            estado.EnvidoPendienteRespuestaJ2   = false;
            mano.SonBuenasDeclarado             = true;
            mano.EnvidoResuelto                 = true;
            mano.GanadorEnvido                  = mano.CantorEnvido;

            int pts = estado.PuntosEnvidoEnJuego > 0
                ? estado.PuntosEnvidoEnJuego
                : EnvidoServicio.ObtenerPuntosSegunTipo(mano.TipoEnvidoCantado);
            if (mano.TipoEnvidoCantado == "FaltaEnvido")
                pts = EnvidoServicio.CalcularPuntosFalta(Math.Max(mano.PuntosHumano, mano.PuntosMaquina));

            mano.PuntosEnvido = pts;
            mano.EstadoEnvido = $"Son buenas. {Nombre(mano.CantorEnvido)} gana {pts} punto(s) de envido.";
            JuegoServicio.SumarPuntos(mano, mano.CantorEnvido!, pts);
            return true;
        }

        public static bool EscalarEnvido(EstadoTrucoMulti1v1 estado, bool esJ1, string tipo)
        {
            var mano = estado.Mano;
            if (!mano.EnvidoCantado || mano.EnvidoResuelto) return false;

            string rolActual = Rol(esJ1);
            if (mano.CantorEnvido == rolActual) return false;
            string tipoNuevo = EnvidoServicio.NormalizarTipo(tipo);
            if (EnvidoServicio.OrdinalTipo(tipoNuevo) <= EnvidoServicio.OrdinalTipo(mano.TipoEnvidoCantado)) return false;

            mano.TipoEnvidoCantado = tipoNuevo;
            mano.CantorEnvido      = rolActual;

            // Acumular la apuesta: rechazar la suba paga lo anterior; aceptarla suma el
            // incremento del nuevo canto (la Falta se calcula al resolver).
            estado.PuntosEnvidoNoQuiero = Math.Max(1, estado.PuntosEnvidoEnJuego);
            estado.PuntosEnvidoEnJuego  = tipoNuevo == "FaltaEnvido"
                ? 0
                : estado.PuntosEnvidoEnJuego + EnvidoServicio.IncrementoPuntosTipo(tipoNuevo);

            mano.EnvidoPendienteRespuestaHumano = !esJ1;
            estado.EnvidoPendienteRespuestaJ2   = esJ1;
            mano.EstadoEnvido = $"{(esJ1 ? "J1" : "J2")} cantó {tipo}.";
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        //  Truco
        // ─────────────────────────────────────────────────────────────
        public static bool CantarTruco(EstadoTrucoMulti1v1 estado, bool esJ1)
        {
            var mano = estado.Mano;
            if (mano.TrucoCantado || mano.GanadorMano != null || mano.PartidaTerminada) return false;

            mano.TrucoCantado    = true;
            mano.NivelTruco      = 1;
            mano.PuntosTrucoMano = 2;
            mano.CantorTruco     = Rol(esJ1);

            mano.TrucoPendienteRespuestaHumano = !esJ1;
            estado.TrucoPendienteRespuestaJ2   = esJ1;
            mano.EstadoTruco = $"{Nombre(mano.CantorTruco)} cantó Truco.";
            return true;
        }

        public static bool ResponderTruco(EstadoTrucoMulti1v1 estado, bool esJ1, bool aceptar, string? escalarA)
        {
            var mano = estado.Mano;
            if (esJ1 && !mano.TrucoPendienteRespuestaHumano) return false;
            if (!esJ1 && !estado.TrucoPendienteRespuestaJ2) return false;

            mano.TrucoPendienteRespuestaHumano = false;
            estado.TrucoPendienteRespuestaJ2   = false;

            if (!aceptar)
            {
                int ptsRefusal       = mano.NivelTruco;
                mano.TrucoResuelto   = true;
                mano.GanadorMano     = mano.CantorTruco;
                mano.PuntosTrucoMano = ptsRefusal;
                mano.EstadoTruco     = $"No quiso. {Nombre(mano.CantorTruco)} gana {ptsRefusal} pt.";
                JuegoServicio.SumarPuntos(mano, mano.CantorTruco!, ptsRefusal);
                return true;
            }

            var escalar = escalarA?.Trim().ToLowerInvariant();
            string respondedor = Rol(esJ1);

            if (!string.IsNullOrEmpty(escalar) && mano.NivelTruco < 3)
            {
                mano.NivelTruco++;
                mano.PuntosTrucoMano = mano.NivelTruco == 2 ? 3 : 4;
                mano.CantorTruco     = respondedor;
                string nombreNivel   = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
                mano.EstadoTruco     = $"Quiso y cantó {nombreNivel}! Vale {mano.PuntosTrucoMano} pt.";
                if (esJ1) estado.TrucoPendienteRespuestaJ2    = true;
                else      mano.TrucoPendienteRespuestaHumano  = true;
            }
            else
            {
                mano.TrucoResuelto = true;
                mano.EstadoTruco   = $"Quiso. Vale {mano.PuntosTrucoMano} pt.";
            }

            return true;
        }

        public static bool EscalarTruco(EstadoTrucoMulti1v1 estado, bool esJ1)
        {
            var mano = estado.Mano;
            if (!mano.TrucoCantado || mano.NivelTruco >= 3) return false;
            if (mano.TrucoPendienteRespuestaHumano || estado.TrucoPendienteRespuestaJ2) return false;
            if (mano.GanadorMano != null || mano.PartidaTerminada) return false;

            string rolActual = Rol(esJ1);
            if (mano.CantorTruco == rolActual) return false;

            mano.NivelTruco++;
            mano.TrucoResuelto   = false;
            mano.CantorTruco     = rolActual;
            mano.PuntosTrucoMano = mano.NivelTruco == 2 ? 3 : 4;
            string nombre        = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
            mano.EstadoTruco     = $"{(esJ1 ? "J1" : "J2")} cantó {nombre}!";

            if (esJ1) estado.TrucoPendienteRespuestaJ2   = true;
            else      mano.TrucoPendienteRespuestaHumano = true;
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        //  Irse al mazo
        // ─────────────────────────────────────────────────────────────
        public static bool IrseAlMazo(EstadoTrucoMulti1v1 estado, bool esJ1)
        {
            var mano = estado.Mano;
            if (mano.GanadorMano != null || mano.PartidaTerminada) return false;

            string ganador = esJ1 ? "Maquina" : "Humano";

            // Puntos para el rival: sin truco → 1; truco cantado SIN responder → equivale a
            // "no quiero" (vale el nivel); truco ya querido → vale lo apostado (2/3/4).
            bool hayCantoSinResponder = mano.TrucoPendienteRespuestaHumano || estado.TrucoPendienteRespuestaJ2;
            int pts;
            if (!mano.TrucoCantado)            pts = 1;
            else if (hayCantoSinResponder)     pts = Math.Max(1, mano.NivelTruco);
            else                               pts = Math.Max(1, mano.PuntosTrucoMano);

            mano.GanadorMano   = ganador;
            mano.TrucoResuelto = true;
            // La mano terminó: no quedan cantos por responder.
            mano.TrucoPendienteRespuestaHumano  = false;
            estado.TrucoPendienteRespuestaJ2    = false;
            mano.EnvidoPendienteRespuestaHumano = false;
            estado.EnvidoPendienteRespuestaJ2   = false;
            mano.EstadoTruco   = $"{(esJ1 ? "J1" : "J2")} se fue al mazo. {(ganador == "Humano" ? "J1" : "J2")} gana {pts} pt.";
            JuegoServicio.SumarPuntos(mano, ganador, pts);
            return true;
        }
    }
}
