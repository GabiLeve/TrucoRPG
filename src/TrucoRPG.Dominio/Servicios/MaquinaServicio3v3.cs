using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// IA para los jugadores máquina en modo 3v3 solitario.
    /// El humano es J1 (EquipoA, junto a J3 y J5). Los 5 restantes son bots.
    /// Los compañeros bot del humano (J3, J5) NO cantan envido/truco por su cuenta
    /// (el humano decide por su equipo); solo juegan cartas y responden cuando les toca.
    /// Heurísticas espejo de <see cref="MaquinaServicio2v2"/>.
    /// </summary>
    public static class MaquinaServicio3v3
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Convierte un jugadorId al equipoId correcto antes de llamar a ObtenerResponsableTruco.
        /// Necesario porque ObtenerResponsableTruco espera un equipoId, no un jugadorId.
        /// </summary>
        private static readonly Func<ManoTruco3v3, string, string> Responsable =
            (m, jugadorId) => TurnoServicio3v3.ObtenerResponsableTruco(m, m.ObtenerEquipoDeJugador(jugadorId));

        /// <summary>
        /// True si el jugador es compañero bot del humano (mismo equipo que J1, distinto de J1)
        /// Y J1 está efectivamente jugando esta mano.
        /// En Pica-Pica donde J1 no es el duelista activo, los bots de EquipoA actúan libremente.
        /// </summary>
        private static bool EsCompaneroDelHumano(ManoTruco3v3 mano, string jugadorId) =>
            jugadorId != "J1"
            && mano.ObtenerEquipoDeJugador(jugadorId) == mano.ObtenerEquipoDeJugador("J1")
            && mano.JugadoresActivos.Contains("J1");

        /// <summary>
        /// Elige carta teniendo en cuenta al equipo: si un compañero ya tiene ganada la baza,
        /// juega la más baja; si no, gana con la mínima que supere al rival.
        /// </summary>
        private static Carta? ElegirCartaEnEquipo(ManoTruco3v3 mano, string jugadorId, List<Carta> manoMaquina)
        {
            if (!manoMaquina.Any()) return null;

            var vuelta = mano.VueltaActual;
            if (vuelta == null || vuelta.CartasJugadas.Count == 0)
                return manoMaquina.OrderBy(c => c.ValorTruco).ElementAt(manoMaquina.Count / 2);

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

            if (mejorEquipo > mejorRival)
                return manoMaquina.OrderBy(c => c.ValorTruco).First();

            var ganadoras = manoMaquina.Where(c => c.ValorTruco > mejorRival).OrderBy(c => c.ValorTruco).ToList();
            return ganadoras.Any() ? ganadoras.First() : manoMaquina.OrderBy(c => c.ValorTruco).First();
        }

        public static bool DebeCantarEnvido(List<Carta> mano) => EnvidoServicio.CalcularTanto(mano) >= 27;

        public static bool AceptarEnvido(List<Carta> mano)
        {
            int tanto = EnvidoServicio.CalcularTanto(mano);
            if (tanto >= 30) return true;
            if (tanto <= 20) return false;

            int probabilidad = tanto switch
            {
                >= 29 => 90, 28 => 80, 27 => 70, 26 => 60, 25 => 50,
                24 => 40, 23 => 30, 22 => 20, 21 => 15, _ => 10
            };
            return _random.Next(1, 101) <= probabilidad;
        }

        public static bool DebeCantarTruco(List<Carta> mano)
        {
            if (!mano.Any()) return false;
            int fuerte = mano.Max(c => c.ValorTruco);
            int suma   = mano.Sum(c => c.ValorTruco);
            if (fuerte >= 12) return true;
            if (fuerte >= 10) return _random.Next(100) < 60;
            if (suma   >= 22) return _random.Next(100) < 35;
            return _random.Next(100) < 12;
        }

        public static bool AceptarTruco(List<Carta> mano)
        {
            int cartaMasFuerte = mano.Any() ? mano.Max(c => c.ValorTruco) : 0;
            int probabilidad = cartaMasFuerte switch
            {
                >= 11 => 85, 10 => 75, 9 => 65, 8 => 55, 7 => 40, 6 => 30, _ => 20
            };
            return _random.Next(1, 101) <= probabilidad;
        }

        private static int FuerzaEquipoEnMano(ManoTruco3v3 mano, string equipoId)
        {
            int mejor = 0;
            foreach (var j in mano.ObtenerEquipo(equipoId).Jugadores)
            {
                foreach (var c in j.Jugadas) mejor = Math.Max(mejor, c.ValorTruco);
                foreach (var c in j.Mano)    mejor = Math.Max(mejor, c.ValorTruco);
            }
            return mejor;
        }

        private static bool AceptarTrucoEnContexto(ManoTruco3v3 mano, string jugadorId)
        {
            string equipo  = mano.ObtenerEquipoDeJugador(jugadorId);
            var jugador     = mano.ObtenerJugador(jugadorId);
            int mejorEquipo = FuerzaEquipoEnMano(mano, equipo);

            int ganadas  = mano.Vueltas.Count(v => v.GanadorVuelta == equipo);
            int perdidas = mano.Vueltas.Count(v => v.GanadorVuelta is not null and not "Parda" && v.GanadorVuelta != equipo);

            if (ganadas > perdidas) return _random.Next(100) < 92;
            if (mejorEquipo >= 12)  return _random.Next(100) < 88;
            if (mejorEquipo >= 10)  return _random.Next(100) < 65;
            if (perdidas > ganadas && mejorEquipo < 9) return _random.Next(100) < 15;
            return (jugador?.Mano.Count ?? 0) > 0 ? AceptarTruco(jugador!.Mano) : _random.Next(100) < 30;
        }

        /// <summary>Procesa el turno de la máquina: decide envido, truco o jugar carta.</summary>
        public static void ProcesarTurnoMaquina(ManoTruco3v3 mano, string jugadorId)
        {
            if (mano.GanadorMano != null || mano.ManoTerminada || mano.PartidaTerminada) return;
            if (mano.TurnoActual != jugadorId) return;
            if (mano.TrucoPendienteRespuestaDe != null || mano.EnvidoPendienteRespuestaDe != null) return;

            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null || !jugador.EsMaquina) return;
            if (jugador.Mano.Count == 0) return;

            bool esCompaHumano = EsCompaneroDelHumano(mano, jugadorId);

            // ── Cantar envido (solo el "pie" de un equipo rival, no los compañeros del humano) ──
            bool esPieDeSuEquipo = jugadorId == TurnoServicio3v3.ObtenerUltimoDelEquipoEnTurno(mano, mano.ObtenerEquipoDeJugador(jugadorId));
            bool ventanaEnvido = !mano.EnvidoCantado && !mano.EnvidoResuelto && mano.Vueltas.Count == 0
                                 && (!mano.TrucoCantado || mano.TrucoPendienteRespuestaDe != null);
            if (!esCompaHumano && esPieDeSuEquipo && ventanaEnvido && DebeCantarEnvido(jugador.Mano))
            {
                EnvidoServicio3v3.Cantar(mano, jugadorId, "Envido", Responsable);
                return;
            }

            // ── Cantar truco ──
            // Primera vuelta: solo el "pie" del equipo rival (último en jugar) puede cantar truco.
            // Segunda vuelta en adelante: cualquier rival puede cantarlo.
            // Los compañeros del humano (esCompaHumano) nunca cantan truco por su cuenta.
            bool puedeGritarTruco = mano.Vueltas.Count == 0 ? esPieDeSuEquipo : true;
            if (!esCompaHumano && !mano.TrucoCantado && !mano.TrucoResuelto && puedeGritarTruco
                && DebeCantarTruco(jugador.Mano))
            {
                TrucoServicio3v3.Cantar(mano, jugadorId, Responsable);
                return;
            }

            // ── Jugar carta ──
            Carta? carta;
            if (mano.OrdenJugarMayor == jugadorId)
            {
                // El humano ordenó jugar la más alta → respetamos la orden y limpiamos el flag.
                mano.OrdenJugarMayor = null;
                carta = jugador.Mano.OrderByDescending(c => c.ValorTruco).FirstOrDefault();
            }
            else
            {
                carta = ElegirCartaEnEquipo(mano, jugadorId, jugador.Mano);
            }
            if (carta == null) return;
            JuegoServicio3v3.JugarCarta(mano, jugadorId, carta);
        }

        /// <summary>Respuesta de la máquina al envido cantado.</summary>
        public static void ResponderEnvido(ManoTruco3v3 mano, string jugadorId)
        {
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return;
            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null || !jugador.EsMaquina) return;

            // El tanto se evalúa SIEMPRE con las 3 cartas originales (mano + jugadas):
            // si la máquina ya tiró una carta en la primera vuelta, igual cuenta.
            var cartasOriginales = jugador.Mano.Concat(jugador.Jugadas).ToList();
            if (cartasOriginales.Count == 0) { EnvidoServicio3v3.ResolverNoQuiero(mano); return; }

            if (!AceptarEnvido(cartasOriginales))
            {
                EnvidoServicio3v3.ResolverNoQuiero(mano);
                return;
            }

            int tanto = EnvidoServicio.CalcularTanto(cartasOriginales);
            string? escala = ElegirEscaladaEnvido(mano.TipoEnvidoCantado, tanto);
            if (escala != null)
            {
                EnvidoServicio3v3.Escalar(mano, jugadorId, escala, Responsable);
                return;
            }

            mano.EnvidoPendienteRespuestaDe = null;
            mano.FaseEnvido = "aceptado";
            EnvidoServicio3v3.IniciarDeclaracionTantos(mano);
        }

        private static string? ElegirEscaladaEnvido(string? tipoActual, int tanto)
        {
            if (tanto < 30) return null;
            string t = (tipoActual ?? "Envido").ToLowerInvariant().Replace(" ", "");
            int r = _random.Next(100);
            return t switch
            {
                "envido"       => tanto >= 32 && r < 40 ? "Real Envido" : r < 50 ? "Envido Envido" : null,
                "envidoenvido" => r < 40 ? "Real Envido" : null,
                "realenvido"   => tanto >= 33 && r < 30 ? "Falta Envido" : null,
                _              => null,
            };
        }

        /// <summary>Declaración de tanto de la máquina (o "son buenas").</summary>
        public static void DeclararTanto(ManoTruco3v3 mano, string jugadorId)
        {
            if (mano.FaseEnvido != "declarando_tantos") return;
            if (mano.EnvidoPendienteRespuestaDe != jugadorId) return;

            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null || !jugador.EsMaquina) return;

            // Declara el tanto REAL (calculado con sus 3 cartas originales al repartir,
            // ya precalculado en TantosReales): si ya jugó una carta, igual cuenta.
            int tantoPropio = mano.TantosReales.TryGetValue(jugadorId, out var tantoReal)
                ? tantoReal
                : EnvidoServicio3v3.TantoOriginal(jugador);

            var orden = TurnoServicio3v3.ObtenerOrdenDeclaracionEnvido(mano);
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

            bool sonBuenas = mejorRival.HasValue && tantoPropio <= mejorRival.Value;
            EnvidoServicio3v3.ProcesarDeclaracion(mano, jugadorId, tantoPropio, sonBuenas);
        }

        /// <summary>Respuesta de la máquina al truco cantado.</summary>
        public static void ResponderTruco(ManoTruco3v3 mano, string jugadorId)
        {
            if (mano.TrucoPendienteRespuestaDe != jugadorId) return;
            var jugador = mano.ObtenerJugador(jugadorId);
            if (jugador == null || !jugador.EsMaquina) return;

            bool acepta = AceptarTrucoEnContexto(mano, jugadorId);

            if (!acepta)
            {
                TrucoServicio3v3.Responder(mano, jugadorId, aceptar: false, escalarA: null, Responsable);
                return;
            }

            // Con el equipo fuerte, a veces sube la apuesta (retruco / vale cuatro).
            int fuerte = FuerzaEquipoEnMano(mano, mano.ObtenerEquipoDeJugador(jugadorId));
            string? escalarA = null;
            if (mano.NivelTruco < 3 && fuerte >= 12 && _random.Next(100) < 55
                && TurnoServicio3v3.PuedeEscalarTruco(mano, jugadorId))
            {
                escalarA = mano.NivelTruco == 1 ? "retruco" : "valecuatro";
            }
            TrucoServicio3v3.Responder(mano, jugadorId, aceptar: true, escalarA, Responsable);
        }

        // ─────────────────────────────────────────────────────────────
        //  Orquestación: avanzar UNA sola acción de la máquina
        //  (lógica de negocio antes en el controller; ahora vive en el dominio)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta exactamente UNA acción del próximo jugador máquina y devuelve un evento
        /// describiendo qué hizo (para mostrar el diálogo). Devuelve null si no hay máquina
        /// por actuar (turno del humano o mano/partida terminada).
        /// </summary>
        public static EventoMaquina3v3? AvanzarUnPaso(ManoTruco3v3 mano)
        {
            if (mano.PartidaTerminada || mano.ManoTerminada || mano.GanadorMano != null) return null;

            string? actor = ProximoActor(mano);
            if (actor == null || actor == "J1") return null;

            var jugador = mano.ObtenerJugador(actor);
            if (jugador == null || !jugador.EsMaquina) return null;

            // ── Responder truco ──
            if (mano.TrucoPendienteRespuestaDe == actor)
            {
                int nivelAntes = mano.NivelTruco;
                ResponderTruco(mano, actor);

                if (mano.NivelTruco > nivelAntes && mano.TrucoPendienteRespuestaDe != null)
                {
                    string nombre = mano.NivelTruco == 2 ? "Retruco" : "Vale Cuatro";
                    return new EventoMaquina3v3(actor, "truco", "¡" + nombre + "!");
                }
                bool noQuiso = (mano.EstadoTruco ?? "").Contains("no quiso");
                return new EventoMaquina3v3(actor, "truco-resp", noQuiso ? "¡No quiero!" : "¡Quiero!");
            }

            // ── Responder envido ──
            if (mano.FaseEnvido == "pendiente_respuesta" && mano.EnvidoPendienteRespuestaDe == actor)
            {
                int ordinalAntes = EnvidoServicio.OrdinalTipo(mano.TipoEnvidoCantado);
                ResponderEnvido(mano, actor);
                if (mano.FaseEnvido == "pendiente_respuesta"
                    && EnvidoServicio.OrdinalTipo(mano.TipoEnvidoCantado) > ordinalAntes)
                    return new EventoMaquina3v3(actor, "envido", "¡" + (mano.TipoEnvidoCantado ?? "Envido") + "!");
                bool quiso = mano.FaseEnvido == "declarando_tantos" || mano.FaseEnvido == "aceptado";
                string textoEnvido = quiso ? "¡Quiero!" : "¡No quiero!";
                // Si el envido se rechazó y el truco sigue pendiente, recordar al humano.
                if (!quiso && mano.TrucoPendienteRespuestaDe != null && mano.TrucoPendienteRespuestaDe != actor)
                    textoEnvido = "¡No quiero! ¿Y el truco?";
                return new EventoMaquina3v3(actor, "envido-resp", textoEnvido);
            }

            // ── Declarar tanto ──
            if (mano.FaseEnvido == "declarando_tantos" && mano.EnvidoPendienteRespuestaDe == actor)
            {
                DeclararTanto(mano, actor);
                if (mano.JugadorQueDijoSonBuenas == actor)
                    return new EventoMaquina3v3(actor, "tanto", "¡Son buenas!");
                string texto = mano.TantosDeclarados.TryGetValue(actor, out var t) && t.HasValue
                    ? t.Value.ToString()
                    : "¡Son buenas!";
                return new EventoMaquina3v3(actor, "tanto", texto);
            }

            // ── Turno normal: cantar o jugar carta ──
            if (mano.TurnoActual == actor)
            {
                bool esCompaHumano = EsCompaneroDelHumano(mano, actor);

                // Un compañero bot te consulta antes de jugar (solo en 3v3 normal).
                if (esCompaHumano && !mano.PicaPica)
                {
                    // ¿Canto los tantos? — lo pregunta el "pie" del equipo del humano.
                    if (!mano.CompaEnvidoConsultado
                        && actor == TurnoServicio3v3.ObtenerUltimoDelEquipoEnTurno(mano, "EquipoA")
                        && !mano.EnvidoCantado && !mano.EnvidoResuelto
                        && mano.Vueltas.Count == 0
                        && (!mano.TrucoCantado || mano.TrucoPendienteRespuestaDe != null))
                    {
                        mano.CompaPista = PistaTanto(EnvidoServicio3v3.TantoOriginal(jugador));
                        mano.CompaConsultaEnvido = true;
                        mano.CompaConsultor = actor;
                        return new EventoMaquina3v3(actor, "consulta-envido", "¿Canto los tantos?");
                    }

                    // ¿Voy o pongo? — solo si tiene una carta ALTA para el truco y todavía
                    // no jugaste tu carta.
                    if (!mano.CompaTrucoConsultado
                        && (mano.ObtenerJugador("J1")?.Jugadas.Count ?? 1) == 0
                        && !mano.TrucoCantado && !mano.TrucoResuelto
                        && mano.TrucoPendienteRespuestaDe == null
                        && jugador.Mano.Count > 0 && jugador.Mano.Max(c => c.ValorTruco) >= 12)
                    {
                        mano.CompaConsultaTruco = true;
                        mano.CompaConsultor = actor;
                        return new EventoMaquina3v3(actor, "consulta-truco", "¿Voy o pongo?");
                    }
                }

                bool envidoAntes = mano.EnvidoCantado;
                bool trucoAntes  = mano.TrucoCantado;

                ProcesarTurnoMaquina(mano, actor);

                if (!envidoAntes && mano.EnvidoCantado)
                    return new EventoMaquina3v3(actor, "envido", "¡" + (mano.TipoEnvidoCantado ?? "Envido") + "!");
                if (!trucoAntes && mano.TrucoCantado)
                    return new EventoMaquina3v3(actor, "truco", "¡Truco!");

                // Si un compañero juega y el envido sigue vivo, deja una pista de su tanto.
                if (esCompaHumano && !mano.PicaPica
                    && !mano.EnvidoCantado && !mano.EnvidoResuelto && mano.Vueltas.Count == 0
                    && string.IsNullOrEmpty(mano.CompaPista))
                {
                    mano.CompaPista = PistaTanto(EnvidoServicio3v3.TantoOriginal(jugador));
                    mano.CompaConsultor = actor;
                }

                return new EventoMaquina3v3(actor, "carta", "");
            }

            return null;
        }

        private static string PistaTanto(int tanto) =>
            tanto >= 28 ? "Tengo mucho" : tanto >= 23 ? "Tengo algo" : "Tengo poco";

        /// <summary>
        /// Resuelve la consulta "¿canto los tantos?": si el humano acepta, el compañero que
        /// preguntó canta el Envido.
        /// </summary>
        public static void ResolverConsultaEnvido(ManoTruco3v3 mano, bool aceptar)
        {
            if (!mano.CompaConsultaEnvido)
                throw new InvalidOperationException("Tu compañero no está preguntando por el envido.");
            mano.CompaConsultaEnvido   = false;
            mano.CompaEnvidoConsultado = true;
            var consultor = mano.CompaConsultor ?? "J3";
            mano.CompaConsultor = null;

            if (aceptar)
                EnvidoServicio3v3.Cantar(mano, consultor, "Envido", TurnoServicio3v3.ObtenerResponsableCanto);
        }

        /// <summary>
        /// Resuelve la consulta "¿voy o pongo?": el compañero juega su carta más baja (voy)
        /// o la más alta (pongo).
        /// </summary>
        public static void ResolverConsultaTruco(ManoTruco3v3 mano, bool voy)
        {
            if (!mano.CompaConsultaTruco)
                throw new InvalidOperationException("Tu compañero no está preguntando por el truco.");
            mano.CompaConsultaTruco   = false;
            mano.CompaTrucoConsultado = true;
            var consultor = mano.CompaConsultor ?? "J3";
            mano.CompaConsultor = null;

            var compa = mano.ObtenerJugador(consultor);
            if (compa == null || compa.Mano.Count == 0 || mano.TurnoActual != consultor) return;

            var carta = voy
                ? compa.Mano.OrderBy(c => c.ValorTruco).First()
                : compa.Mano.OrderByDescending(c => c.ValorTruco).First();
            JuegoServicio3v3.JugarCarta(mano, consultor, carta);
        }

        /// <summary>Próximo jugador que debe actuar (el envido va primero).</summary>
        private static string? ProximoActor(ManoTruco3v3 mano)
        {
            if (mano.EnvidoPendienteRespuestaDe != null &&
                (mano.FaseEnvido == "pendiente_respuesta" || mano.FaseEnvido == "declarando_tantos"))
                return mano.EnvidoPendienteRespuestaDe;
            if (mano.TrucoPendienteRespuestaDe != null)  return mano.TrucoPendienteRespuestaDe;
            if (mano.EnvidoPendienteRespuestaDe != null) return mano.EnvidoPendienteRespuestaDe;
            if (!mano.ManoTerminada && mano.GanadorMano == null) return mano.TurnoActual;
            return null;
        }
    }

    /// <summary>Evento de una acción de máquina, para mostrar diálogos en el front.</summary>
    public record EventoMaquina3v3(string Jugador, string Tipo, string Texto);
}
