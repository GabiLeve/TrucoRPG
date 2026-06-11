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
        /// Mejor carta del equipo en toda la mano: las que YA jugó + las que le quedan.
        /// Sirve para no achicarse cuando ya tiró una carta brava (p. ej. el ancho de basto).
        /// </summary>
        private static int FuerzaEquipoEnMano(ManoTruco2v2 mano, string equipoId)
        {
            int mejor = 0;
            foreach (var j in mano.ObtenerEquipo(equipoId).Jugadores)
            {
                foreach (var c in j.Jugadas) mejor = Math.Max(mejor, c.ValorTruco);
                foreach (var c in j.Mano)    mejor = Math.Max(mejor, c.ValorTruco);
            }
            return mejor;
        }

        /// <summary>
        /// Decide si la máquina acepta el truco/retruco considerando TODA la situación del
        /// equipo (vueltas ganadas y cartas ya jugadas), no solo lo que le queda en la mano.
        /// </summary>
        private static bool AceptarTrucoEnContexto(ManoTruco2v2 mano, string jugadorId)
        {
            string equipo  = mano.ObtenerEquipoDeJugador(jugadorId);
            var jugador     = mano.ObtenerJugador(jugadorId);
            int mejorEquipo = FuerzaEquipoEnMano(mano, equipo);

            int ganadas   = mano.Vueltas.Count(v => v.GanadorVuelta == equipo);
            int perdidas  = mano.Vueltas.Count(v => v.GanadorVuelta is not null and not "Parda" && v.GanadorVuelta != equipo);

            // Va ganando la mano → casi siempre quiere.
            if (ganadas > perdidas) return _random.Next(100) < 92;
            // Tiene (o jugó) una carta brava → quiere casi siempre.
            if (mejorEquipo >= 12) return _random.Next(100) < 88;
            if (mejorEquipo >= 10) return _random.Next(100) < 65;
            // Va claramente perdiendo y sin cartas → normalmente no quiere.
            if (perdidas > ganadas && mejorEquipo < 9) return _random.Next(100) < 15;
            // Caso intermedio: según lo que le queda en la mano.
            return (jugador?.Mano.Count ?? 0) > 0 ? AceptarTruco(jugador!.Mano) : _random.Next(100) < 30;
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
                    mano.PuntosEnvidoNoQuiero = 1;
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

            // El tanto se evalúa SIEMPRE con las 3 cartas originales (mano + jugadas):
            // si la máquina ya tiró una carta en la primera vuelta, igual cuenta.
            var cartasOriginales = jugador.Mano.Concat(jugador.Jugadas).ToList();
            if (cartasOriginales.Count == 0) { EnvidoServicio2v2.ResolverNoQuiero(mano); return; }

            bool acepta = AceptarEnvido(cartasOriginales);
            if (!acepta)
            {
                EnvidoServicio2v2.ResolverNoQuiero(mano);
                return;
            }

            // Si tiene mucho tanto, a veces sube la apuesta en vez de solo querer.
            // Delega en EnvidoServicio2v2.Escalar para que los puntos se acumulen bien.
            int tanto = EnvidoServicio.CalcularTanto(cartasOriginales);
            string? escala = ElegirEscaladaEnvido(mano.TipoEnvidoCantado, tanto);
            if (escala != null && EnvidoServicio2v2.Escalar(
                    mano, jugadorId, escala,
                    (m, j) => TurnoServicio2v2.ObtenerResponsableTruco(m, m.ObtenerEquipoDeJugador(j))))
            {
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

            // Declara el tanto REAL (calculado con sus 3 cartas originales al repartir,
            // ya precalculado en TantosReales): si ya jugó una carta, igual cuenta.
            int tantoPropio = mano.TantosReales.TryGetValue(jugadorId, out var tantoReal)
                ? tantoReal
                : EnvidoServicio2v2.TantoOriginal(jugador);

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

            bool acepta = AceptarTrucoEnContexto(mano, jugadorId);
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
                // Con el equipo fuerte (carta brava jugada o en mano), a veces sube la apuesta.
                int fuerte = FuerzaEquipoEnMano(mano, mano.ObtenerEquipoDeJugador(jugadorId));
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
                    var equipoContrario = mano.ObtenerEquipoContrario(equipoMaquina);
                    mano.PuedeEscalarTruco = TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, equipoContrario.Id);
                }
                else
                {
                    mano.TrucoResuelto = false;
                    mano.EstadoTruco   = $"{jugadorId} quiso truco. Vale {mano.PuntosTrucoMano} pt.";
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  Orquestación: avanzar UNA sola acción de la máquina
        //  (lógica de negocio antes en Truco2v2Controller; movida al dominio)
        // ─────────────────────────────────────────────────────────────
        private const string J1 = "J1";
        private const string J3 = "J3";

        /// <summary>
        /// Ejecuta exactamente UNA acción del próximo jugador máquina y devuelve un evento
        /// describiendo qué hizo (para mostrar el diálogo). Devuelve null si no hay máquina
        /// por actuar (turno del humano o mano/partida terminada).
        /// </summary>
        public static EventoMaquina2v2? AvanzarUnPaso(ManoTruco2v2 mano)
        {
            if (mano.PartidaTerminada || mano.ManoTerminada || mano.GanadorMano != null) return null;

            string? actor = ProximoActor(mano);
            if (actor == null || actor == J1) return null;

            var jugador = mano.ObtenerJugador(actor);
            if (jugador == null || !jugador.EsMaquina) return null;

            // ── Responder truco ──
            if (mano.TrucoPendienteRespuestaDe == actor)
            {
                int nivelAntes = mano.NivelTruco;
                ResponderTruco(mano, actor);

                if (mano.NivelTruco > nivelAntes && mano.TrucoPendienteRespuestaDe == J1)
                {
                    string nombre = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
                    return new EventoMaquina2v2(actor, "truco", "¡" + nombre + "!");
                }

                bool noQuiso = (mano.EstadoTruco ?? "").Contains("no quiso");
                return new EventoMaquina2v2(actor, "truco-resp", noQuiso ? "¡No quiero!" : "¡Quiero!");
            }

            // ── Responder envido ──
            if (mano.FaseEnvido == "pendiente_respuesta" && mano.EnvidoPendienteRespuestaDe == actor)
            {
                ResponderEnvido(mano, actor);
                bool quiso = mano.FaseEnvido == "declarando_tantos" || mano.FaseEnvido == "aceptado";
                return new EventoMaquina2v2(actor, "envido-resp", quiso ? "¡Quiero!" : "¡No quiero!");
            }

            // ── Declarar tanto ──
            if (mano.FaseEnvido == "declarando_tantos" && mano.EnvidoPendienteRespuestaDe == actor)
            {
                DeclararTanto(mano, actor);
                if (mano.JugadorQueDijoSonBuenas == actor)
                    return new EventoMaquina2v2(actor, "tanto", "¡Son buenas!");
                string texto = mano.TantosDeclarados.TryGetValue(actor, out var t) && t.HasValue
                    ? t.Value.ToString()
                    : "¡Son buenas!";
                return new EventoMaquina2v2(actor, "tanto", texto);
            }

            // ── Turno normal: cantar o jugar carta ──
            if (mano.TurnoActual == actor)
            {
                // El compañero (J3), cuando es el PIE del equipo, le pregunta al humano si
                // quiere que cante los tantos (le da una pista de su propio tanto).
                if (actor == J3
                    && J3 == TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoA")
                    && !mano.CompaEnvidoConsultado
                    && !mano.EnvidoCantado && !mano.EnvidoResuelto
                    && mano.Vueltas.Count == 0
                    && (!mano.TrucoCantado || mano.TrucoPendienteRespuestaDe != null))
                {
                    int tantoCompa = EnvidoServicio2v2.TantoOriginal(jugador);
                    mano.CompaPista = tantoCompa >= 28 ? "Tengo mucho"
                                    : tantoCompa >= 23 ? "Tengo algo"
                                    : "Tengo poco";
                    mano.CompaConsultaEnvido = true;
                    return new EventoMaquina2v2(actor, "consulta-envido", "¿Canto los tantos?");
                }

                // El compañero (J3) pregunta antes de cantar truco: ¿voy o pongo?
                if (actor == J3 && !mano.CompaTrucoConsultado
                    && (mano.ObtenerJugador(J1)?.Jugadas.Count ?? 1) == 0
                    && !mano.TrucoCantado && !mano.TrucoResuelto
                    && mano.TrucoPendienteRespuestaDe == null
                    && jugador.Mano.Count > 0 && jugador.Mano.Max(c => c.ValorTruco) >= 10)
                {
                    mano.CompaConsultaTruco = true;
                    return new EventoMaquina2v2(actor, "consulta-truco", "¿Voy o pongo?");
                }

                bool envidoAntes = mano.EnvidoCantado;
                bool trucoAntes  = mano.TrucoCantado;

                ProcesarTurnoMaquina(mano, actor);

                if (!envidoAntes && mano.EnvidoCantado)
                {
                    if (mano.EnvidoPendienteRespuestaDe == J1 && _random.Next(100) < 50)
                    {
                        var compa = mano.ObtenerJugador(J3);
                        int tantoCompa = compa != null ? EnvidoServicio2v2.TantoOriginal(compa) : 0;
                        mano.CompaPista = tantoCompa >= 28 ? "Tengo mucho"
                                        : tantoCompa >= 23 ? "Tengo algo"
                                        : "Tengo poco";
                    }
                    return new EventoMaquina2v2(actor, "envido", "¡" + (mano.TipoEnvidoCantado ?? "Envido") + "!");
                }
                if (!trucoAntes && mano.TrucoCantado)
                {
                    OfrecerEnvidoVaPrimeroSiCorresponde(mano);
                    return new EventoMaquina2v2(actor, "truco", "¡Truco!");
                }

                if (actor == J3
                    && !mano.EnvidoCantado && !mano.EnvidoResuelto
                    && mano.Vueltas.Count == 0
                    && string.IsNullOrEmpty(mano.CompaPista)
                    && J1 == TurnoServicio2v2.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoA"))
                {
                    int tantoCompa = EnvidoServicio2v2.TantoOriginal(jugador);
                    mano.CompaPista = tantoCompa >= 28 ? "Tengo mucho"
                                    : tantoCompa >= 23 ? "Tengo algo"
                                    : "Tengo poco";
                }

                return new EventoMaquina2v2(actor, "carta", "");
            }

            return null;
        }

        /// <summary>
        /// Tras un truco cantado por un rival, si el humano (J1) ya jugó su carta pero el
        /// compañero (J3) todavía no, le ofrece cantar el envido "va primero" antes de responder.
        /// </summary>
        private static void OfrecerEnvidoVaPrimeroSiCorresponde(ManoTruco2v2 mano)
        {
            if (mano.CompaConsultaEnvido || mano.CompaEnvidoConsultado) return;
            if (mano.EnvidoCantado || mano.EnvidoResuelto)              return;
            if (mano.Vueltas.Count != 0)                                return;
            if (mano.TrucoPendienteRespuestaDe != J1)                   return;
            if ((mano.ObtenerJugador(J1)?.Jugadas.Count ?? 0) == 0)     return;
            if ((mano.ObtenerJugador(J3)?.Jugadas.Count ?? 1) != 0)     return;

            var compa = mano.ObtenerJugador(J3);
            int tantoCompa = compa != null ? EnvidoServicio2v2.TantoOriginal(compa) : 0;
            mano.CompaPista = tantoCompa >= 28 ? "Tengo mucho"
                            : tantoCompa >= 23 ? "Tengo algo"
                            : "Tengo poco";
            mano.CompaConsultaEnvido = true;
        }

        /// <summary>
        /// Resuelve la consulta "¿voy o pongo?" del compañero (J3) al humano:
        /// "voy" → el compañero juega su carta más baja (vos metés la alta);
        /// "pongo" → juega su carta más alta para intentar ganar la baza.
        /// </summary>
        public static void ResolverConsultaTruco(ManoTruco2v2 mano, bool voy)
        {
            mano.CompaConsultaTruco   = false;
            mano.CompaTrucoConsultado = true;

            var compa = mano.ObtenerJugador(J3);
            if (compa == null || compa.Mano.Count == 0) return;

            var carta = voy
                ? compa.Mano.OrderBy(c => c.ValorTruco).First()
                : compa.Mano.OrderByDescending(c => c.ValorTruco).First();

            // Si el envido sigue disponible, deja una pista de su tanto.
            if (!mano.EnvidoCantado && !mano.EnvidoResuelto && mano.Vueltas.Count == 0
                && string.IsNullOrEmpty(mano.CompaPista))
            {
                int tantoCompa = EnvidoServicio2v2.TantoOriginal(compa);
                mano.CompaPista = tantoCompa >= 28 ? "Tengo mucho"
                                : tantoCompa >= 23 ? "Tengo algo"
                                : "Tengo poco";
            }

            JuegoServicio2v2.JugarCarta(mano, J3, carta);
        }

        /// <summary>Próximo jugador que debe actuar (el envido va primero).</summary>
        private static string? ProximoActor(ManoTruco2v2 mano)
        {
            if (mano.EnvidoPendienteRespuestaDe != null &&
                (mano.FaseEnvido == "pendiente_respuesta" || mano.FaseEnvido == "declarando_tantos"))
                return mano.EnvidoPendienteRespuestaDe;
            if (mano.TrucoPendienteRespuestaDe != null)    return mano.TrucoPendienteRespuestaDe;
            if (mano.EnvidoPendienteRespuestaDe != null)   return mano.EnvidoPendienteRespuestaDe;
            if (!mano.ManoTerminada && mano.GanadorMano == null) return mano.TurnoActual;
            return null;
        }
    }

    /// <summary>Evento de una acción de máquina 2v2, para mostrar diálogos en el front.</summary>
    public record EventoMaquina2v2(string Jugador, string Tipo, string Texto);
}
