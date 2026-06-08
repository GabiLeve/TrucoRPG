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
        /// Elige carta teniendo en cuenta al equipo: si un compañero YA tiene ganada la baza,
        /// juega la carta más baja (guarda las altas para las próximas vueltas) en vez de
        /// "taparlo" con una carta alta. Si no, intenta ganarle al rival con lo mínimo.
        /// </summary>
        private static Carta? ElegirCartaEnEquipo(ManoTruco2v2 mano, string jugadorId, List<Carta> manoMaquina)
        {
            if (!manoMaquina.Any()) return null;

            var vuelta = mano.VueltaActual;
            if (vuelta == null || vuelta.CartasJugadas.Count == 0)
            {
                // Primero en jugar la vuelta: carta media.
                return manoMaquina.OrderBy(c => c.ValorTruco).ElementAt(manoMaquina.Count / 2);
            }

            string equipo = mano.ObtenerEquipoDeJugador(jugadorId);
            int mejorEquipo = -1, mejorRival = -1;
            foreach (var kv in vuelta.CartasJugadas)
            {
                int v = kv.Value.ValorTruco;
                if (mano.ObtenerEquipoDeJugador(kv.Key) == equipo)
                    mejorEquipo = Math.Max(mejorEquipo, v);
                else
                    mejorRival = Math.Max(mejorRival, v);
            }

            // Si mi equipo ya va ganando la baza, no gasto una carta alta: tiro la más baja.
            if (mejorEquipo > mejorRival)
                return manoMaquina.OrderBy(c => c.ValorTruco).First();

            // Si no, gano con la mínima carta que supere al rival; si no puedo, tiro la más baja.
            var ganadoras = manoMaquina.Where(c => c.ValorTruco > mejorRival).OrderBy(c => c.ValorTruco).ToList();
            return ganadoras.Any() ? ganadoras.First() : manoMaquina.OrderBy(c => c.ValorTruco).First();
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
            if (!mano.Any()) return false;
            int fuerte = mano.Max(c => c.ValorTruco);
            int suma   = mano.Sum(c => c.ValorTruco);

            if (fuerte >= 12) return true;                       // 7 espada, ancho de basto/espada
            if (fuerte >= 10) return _random.Next(100) < 60;     // tiene un 3 o mejor
            if (suma   >= 22) return _random.Next(100) < 35;     // mano pareja y fuerte
            return _random.Next(100) < 12;                       // farol ocasional
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
            var carta = ElegirCartaEnEquipo(mano, jugadorId, jugador.Mano);
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
                return;
            }

            // Si tiene mucho tanto, a veces sube la apuesta en vez de solo querer.
            int tanto = EnvidoServicio.CalcularTanto(jugador.Mano);
            string? escala = ElegirEscaladaEnvido(mano.TipoEnvidoCantado, tanto);
            if (escala != null)
            {
                mano.TipoEnvidoCantado = EnvidoServicio.NormalizarTipo(escala);
                mano.PuntosEnvido      = EnvidoServicio2v2.ObtenerPuntosEnJuego(mano.TipoEnvidoCantado);
                mano.CantorEnvido      = jugadorId;
                string equipoCantor    = mano.ObtenerEquipoDeJugador(jugadorId);
                mano.EnvidoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, equipoCantor);
                mano.EstadoEnvido      = $"{jugadorId} cantó {escala}.";
                return; // queda esperando la respuesta del rival
            }

            mano.EnvidoPendienteRespuestaDe = null;
            mano.FaseEnvido = "aceptado";
            EnvidoServicio2v2.IniciarDeclaracionTantos(mano);
        }

        /// <summary>
        /// Decide si la máquina sube la apuesta del envido (envido envido / real envido / falta)
        /// según lo que ya se cantó y su tanto. Devuelve null si no escala.
        /// </summary>
        private static string? ElegirEscaladaEnvido(string? tipoActual, int tanto)
        {
            if (tanto < 30) return null; // solo escala con mucho tanto
            string t = (tipoActual ?? "Envido").ToLowerInvariant().Replace(" ", "");
            int r = _random.Next(100);
            return t switch
            {
                "envido"        => tanto >= 32 && r < 40 ? "Real Envido"
                                 : r < 50 ? "Envido Envido" : null,
                "envidoenvido"  => r < 40 ? "Real Envido" : null,
                "realenvido"    => tanto >= 33 && r < 30 ? "Falta Envido" : null,
                _               => null,
            };
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

            // Mejor tanto que ya muestra el equipo rival. Como a los jugadores cuyo equipo
            // ya va ganando se los saltea (no llegan a cantar), si me toca cantar es porque
            // mi equipo NO va ganando: el rival tiene el mejor tanto en la mesa.
            var orden = TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano);
            int idxActual = orden.IndexOf(jugadorId);
            string equipoActual = mano.ObtenerEquipoDeJugador(jugadorId);
            int? mejorRival = null;

            for (int i = 0; i < idxActual; i++)
            {
                var idPrevio = orden[i];
                if (mano.ObtenerEquipoDeJugador(idPrevio) == equipoActual) continue;
                if (mano.TantosDeclarados.TryGetValue(idPrevio, out var t) && t.HasValue)
                    if (mejorRival == null || t.Value > mejorRival.Value) mejorRival = t.Value;
            }

            // Si mi tanto no supera lo que ya hay en la mesa, digo "son buenas".
            // (Cantar un número que no le gana al rival no tiene sentido.)
            bool sonBuenas = mejorRival.HasValue && tantoPropio <= mejorRival.Value;
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
                // Con mano fuerte, a veces en vez de solo querer, sube la apuesta (retruco / vale cuatro).
                int fuerte = jugador.Mano.Max(c => c.ValorTruco);
                if (mano.NivelTruco < 3 && fuerte >= 12 && _random.Next(100) < 55)
                {
                    string equipoMaquina = mano.ObtenerEquipoDeJugador(jugadorId);
                    mano.NivelTruco++;
                    mano.PuntosTrucoMano   = mano.NivelTruco == 2 ? 3 : 4;
                    mano.EquipoCantorTruco = equipoMaquina;
                    mano.CantorTruco       = jugadorId;
                    mano.TrucoResuelto     = false;
                    string nombre          = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
                    mano.EstadoTruco       = $"{jugadorId} cantó {nombre}.";
                    mano.TrucoPendienteRespuestaDe = TurnoServicio2v2.ObtenerResponsableTruco(mano, equipoMaquina);
                }
                else
                {
                    mano.TrucoResuelto = false;
                    mano.EstadoTruco   = $"{jugadorId} quiso truco. Vale {mano.PuntosTrucoMano} pt.";
                }
            }
        }
    }
}
