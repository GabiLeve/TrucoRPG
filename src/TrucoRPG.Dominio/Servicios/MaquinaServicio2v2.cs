using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// IA para los jugadores máquina en modo 2v2.
    /// Gestiona decisiones de carta, envido y truco para máquinas.
    /// </summary>
    public static class MaquinaServicio2v2
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Elige la mejor carta para jugar según el contexto de la vuelta.
        /// Si ya hay cartas jugadas, intenta ganarlas con la mínima carta posible.
        /// </summary>
        public static Carta? ElegirCarta(List<Carta> manoMaquina, List<Carta> cartasYaJugadas)
        {
            if (!manoMaquina.Any()) return null;

            if (!cartasYaJugadas.Any())
            {
                // Juega primera: carta media
                return manoMaquina
                    .OrderBy(c => c.ValorTruco)
                    .ElementAt(manoMaquina.Count / 2);
            }

            int maxRival = cartasYaJugadas.Max(c => c.ValorTruco);

            var cartasQueGanan = manoMaquina
                .Where(c => c.ValorTruco > maxRival)
                .OrderBy(c => c.ValorTruco)
                .ToList();

            return cartasQueGanan.Any()
                ? cartasQueGanan.First()
                : manoMaquina.OrderBy(c => c.ValorTruco).First();
        }

        /// <summary>
        /// Decide si la máquina debe cantar envido.
        /// </summary>
        public static bool DebeCantarEnvido(List<Carta> mano)
        {
            int tanto = EnvidoServicio.CalcularTanto(mano);
            return tanto >= 27;
        }

        /// <summary>
        /// Decide si la máquina acepta el envido.
        /// </summary>
        public static bool AceptarEnvido(List<Carta> mano)
        {
            int tanto = EnvidoServicio.CalcularTanto(mano);
            if (tanto >= 30) return true;
            if (tanto <= 20) return false;

            int probabilidad = tanto switch
            {
                >= 29 => 90,
                28 => 80,
                27 => 70,
                26 => 60,
                25 => 50,
                24 => 40,
                23 => 30,
                22 => 20,
                21 => 15,
                _  => 10
            };

            return _random.Next(1, 101) <= probabilidad;
        }

        /// <summary>
        /// Decide si la máquina debe decir "son buenas" durante la declaración de tantos.
        /// Lo hace si su equipo claramente va perdiendo.
        /// </summary>
        public static bool DebeDeclararSonBuenas(
            int tantoPropio,
            int? mejorTantoRivalDeclarado)
        {
            if (mejorTantoRivalDeclarado == null) return false;
            // Solo dice son buenas si la diferencia es clara (más de 4 puntos)
            return mejorTantoRivalDeclarado > tantoPropio + 4;
        }

        /// <summary>
        /// Decide si la máquina debe cantar truco.
        /// </summary>
        public static bool DebeCantarTruco(List<Carta> mano)
        {
            int cartaMasFuerte = mano.Any() ? mano.Max(c => c.ValorTruco) : 0;
            return cartaMasFuerte >= 12; // 7 Espada (12), As de Basto (13), As de Espada (14) únicamente
        }

        /// <summary>
        /// Decide si la máquina acepta el truco.
        /// </summary>
        public static bool AceptarTruco(List<Carta> mano)
        {
            int cartaMasFuerte = mano.Any() ? mano.Max(c => c.ValorTruco) : 0;

            int probabilidad = cartaMasFuerte switch
            {
                >= 11 => 85,
                10 => 75,
                9  => 65,
                8  => 55,
                7  => 40,
                6  => 30,
                _  => 20
            };

            return _random.Next(1, 101) <= probabilidad;
        }

        /// <summary>
        /// Procesa el turno de la máquina en la mano actual.
        /// Incluye decisiones de envido, truco y jugar carta.
        /// </summary>
        public static void ProcesarTurnoMaquina(ManoTruco2v2 mano, string jugadorId)
        {
            if (mano.GanadorMano != null || mano.ManoTerminada || mano.PartidaTerminada) return;
            if (mano.TurnoActual != jugadorId) return;
            if (mano.TrucoPendienteRespuestaDe != null || mano.EnvidoPendienteRespuestaDe != null) return;

            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null || !jugador.EsMaquina) return;
            // Sin cartas en mano → esta máquina ya jugó en esta mano (no debería pasar, pero guard defensivo)
            if (jugador.Mano.Count == 0) return;

            // ── Cantar envido si corresponde ──────────────────────────────
            // El compañero (J3) no canta solo (le pregunta al humano). Solo el "pie" (último
            // del equipo en el orden) puede cantar el envido. Ventana: primera vuelta y sin
            // truco resuelto (el envido va primero si el truco está pendiente).
            bool esPieDeSuEquipo = jugadorId == TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, mano.ObtenerEquipoDeJugador(jugadorId));
            bool ventanaEnvido = !mano.EnvidoCantado && !mano.EnvidoResuelto && mano.Vueltas.Count == 0
                                 && (!mano.TrucoCantado || mano.TrucoPendienteRespuestaDe != null);
            if (jugadorId != "J3" && esPieDeSuEquipo && ventanaEnvido)
            {
                if (DebeCantarEnvido(jugador.Mano))
                {
                    mano.EnvidoCantado = true;
                    mano.CantorEnvido  = jugadorId;
                    mano.TipoEnvidoCantado = "Envido";
                    mano.PuntosEnvido  = 2;
                    mano.FaseEnvido    = "pendiente_respuesta";

                    // El primer jugador del equipo contrario responde
                    string equipoCantor  = mano.ObtenerEquipoDeJugador(jugadorId);
                    mano.EnvidoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, equipoCantor);
                    mano.EstadoEnvido = $"{jugadorId} cantó Envido.";
                    return;
                }
            }

            // ── Cantar truco si corresponde ───────────────────────────────
            // La máquina NO canta truco antes de que se juegue la primera carta,
            // así el envido conserva su ventana ("el envido va primero").
            bool primeraVueltaIniciada = mano.Vueltas.Count > 0 || mano.VueltaActual != null;
            // El compañero (J3) no canta truco solo: le pregunta al humano (¿voy o pongo?).
            if (jugadorId != "J3" && !mano.TrucoCantado && !mano.TrucoResuelto && primeraVueltaIniciada)
            {
                if (DebeCantarTruco(jugador.Mano))
                {
                    mano.TrucoCantado    = true;
                    mano.CantorTruco     = jugadorId;
                    mano.EquipoCantorTruco = mano.ObtenerEquipoDeJugador(jugadorId);
                    mano.NivelTruco      = 1;
                    mano.PuntosTrucoMano = 2;

                    string equipoCantor = mano.EquipoCantorTruco!;
                    mano.TrucoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, equipoCantor);
                    mano.EstadoTruco = $"{jugadorId} cantó Truco.";
                    return;
                }
            }

            // ── Jugar carta ───────────────────────────────────────────────
            var cartasEnVuelta = mano.VueltaActual?.CartasJugadas.Values.ToList() ?? new List<Carta>();
            var carta = ElegirCarta(jugador.Mano, cartasEnVuelta);
            if (carta == null) return; // mano vacía, guard defensivo
            JuegoServicio2v2.JugarCarta(mano, jugadorId, carta);
        }

        /// <summary>
        /// Procesa la respuesta de la máquina al envido cantado.
        /// </summary>
        public static void ResponderEnvido(ManoTruco2v2 mano, string jugadorId)
        {
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return;
            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null || !jugador.EsMaquina) return;
            if (jugador.Mano.Count == 0) { EnvidoServicio2v2.ResolverNoQuiero(mano); return; }

            bool acepta = AceptarEnvido(jugador.Mano);
            if (!acepta)
            {
                EnvidoServicio2v2.ResolverNoQuiero(mano);
            }
            else
            {
                mano.EnvidoPendienteRespuestaDe = null;
                mano.FaseEnvido = "aceptado";
                EnvidoServicio2v2.IniciarDeclaracionTantos(mano);
            }
        }

        /// <summary>
        /// Procesa la declaración de tanto de la máquina (o dice "son buenas").
        /// </summary>
        public static void DeclararTanto(ManoTruco2v2 mano, string jugadorId)
        {
            if (mano.FaseEnvido != "declarando_tantos") return;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return;

            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null || !jugador.EsMaquina) return;
            if (jugador.Mano.Count == 0)
            {
                EnvidoServicio2v2.ProcesarDeclaracion(mano, jugadorId, 0, sonBuenas: true);
                return;
            }

            int tantoPropio = EnvidoServicio.CalcularTanto(jugador.Mano);

            // Verificar si hay tantos del rival ya declarados
            var orden = TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano);
            int idxActual = orden.IndexOf(jugadorId);
            int? mejorRival = null;

            for (int i = 0; i < idxActual; i++)
            {
                var idPrevio = orden[i];
                string equipoPrevio = mano.ObtenerEquipoDeJugador(idPrevio);
                string equipoActual = mano.ObtenerEquipoDeJugador(jugadorId);

                if (equipoPrevio != equipoActual && mano.TantosDeclarados.TryGetValue(idPrevio, out var t) && t.HasValue)
                {
                    if (mejorRival == null || t.Value > mejorRival.Value)
                        mejorRival = t.Value;
                }
            }

            bool sonBuenas = DebeDeclararSonBuenas(tantoPropio, mejorRival);
            EnvidoServicio2v2.ProcesarDeclaracion(mano, jugadorId, tantoPropio, sonBuenas);
        }

        /// <summary>
        /// Procesa la respuesta de la máquina al truco cantado.
        /// </summary>
        public static void ResponderTruco(ManoTruco2v2 mano, string jugadorId)
        {
            if (mano.TrucoPendienteRespuestaDe != jugadorId) return;
            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null || !jugador.EsMaquina) return;

            bool acepta = jugador.Mano.Count > 0 && AceptarTruco(jugador.Mano);
            mano.TrucoPendienteRespuestaDe = null;

            if (!acepta)
            {
                int pts = mano.NivelTruco;
                mano.TrucoResuelto   = true;
                mano.GanadorMano     = mano.EquipoCantorTruco;
                mano.ManoTerminada   = true;
                mano.PuntosTrucoMano = pts;
                mano.EstadoTruco     = $"{jugadorId} no quiso truco. {mano.EquipoCantorTruco} gana {pts} pt.";
                JuegoServicio2v2.SumarPuntos(mano, mano.EquipoCantorTruco!, pts);
            }
            else
            {
                mano.TrucoResuelto = false;
                mano.EstadoTruco   = $"{jugadorId} quiso truco. Vale {mano.PuntosTrucoMano} pt.";
            }
        }
    }
}
