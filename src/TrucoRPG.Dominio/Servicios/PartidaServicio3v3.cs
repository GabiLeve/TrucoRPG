using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Crea y configura una nueva mano 3v3, reparte 18 cartas (3 por jugador).
    /// Espejo de <see cref="PartidaServicio2v2"/> generalizado a 6 jugadores.
    /// </summary>
    public static class PartidaServicio3v3
    {
        /// <summary>
        /// Crea una nueva mano 3v3. Los jugadores deben suministrarse con ids/nombres
        /// ya configurados; si faltan, se crean por defecto (J1..J6).
        /// </summary>
        public static ManoTruco3v3 CrearManoNueva(
            int numeroDeMano = 1,
            int puntosEquipoA = 0,
            int puntosEquipoB = 0,
            Jugador? pos1 = null,
            Jugador? pos2 = null,
            Jugador? pos3 = null,
            Jugador? pos4 = null,
            Jugador? pos5 = null,
            Jugador? pos6 = null)
        {
            // El jugador mano rota horario según el número de mano.
            int posicionMano = ((numeroDeMano - 1) % 6) + 1;

            var j1 = pos1 ?? new Jugador { Id = "J1", Nombre = "Jugador 1", EsMaquina = false };
            var j2 = pos2 ?? new Jugador { Id = "J2", Nombre = "Jugador 2", EsMaquina = false };
            var j3 = pos3 ?? new Jugador { Id = "J3", Nombre = "Jugador 3", EsMaquina = false };
            var j4 = pos4 ?? new Jugador { Id = "J4", Nombre = "Jugador 4", EsMaquina = false };
            var j5 = pos5 ?? new Jugador { Id = "J5", Nombre = "Jugador 5", EsMaquina = false };
            var j6 = pos6 ?? new Jugador { Id = "J6", Nombre = "Jugador 6", EsMaquina = false };

            var jugadores = new[] { j1, j2, j3, j4, j5, j6 };
            string jugadorMano = jugadores[posicionMano - 1].Id;

            var mano = new ManoTruco3v3
            {
                NumeroDeMano  = numeroDeMano,
                Posicion1     = j1,
                Posicion2     = j2,
                Posicion3     = j3,
                Posicion4     = j4,
                Posicion5     = j5,
                Posicion6     = j6,
                PuntosEquipoA = puntosEquipoA,
                PuntosEquipoB = puntosEquipoB,
                JugadorMano   = jugadorMano,
            };

            // Configurar equipos: A = J1/J3/J5, B = J2/J4/J6
            mano.EquipoA.Jugador1 = j1;
            mano.EquipoA.Jugador2 = j3;
            mano.EquipoA.Jugador3 = j5;
            mano.EquipoB.Jugador1 = j2;
            mano.EquipoB.Jugador2 = j4;
            mano.EquipoB.Jugador3 = j6;

            mano.EquipoMano  = mano.ObtenerEquipoDeJugador(jugadorMano);
            mano.TurnoActual = jugadorMano;
            mano.JugadoresActivos = new() { "J1", "J2", "J3", "J4", "J5", "J6" };

            Repartir(mano, mano.JugadoresActivos);

            return mano;
        }

        /// <summary>Umbral de puntos a partir del cual el 3v3 pasa a Pica-Pica (1 vs 1).</summary>
        public const int PuntajePicaPica = 5;

        /// <summary>
        /// Umbral a partir del cual el Pica-Pica se desactiva definitivamente.
        /// Si algún equipo alcanza este valor, se completan los duelos pendientes
        /// del ciclo actual y luego solo se juegan manos redondas hasta llegar a 30.
        /// </summary>
        public const int PuntajeFinalPicaPica = 25;

        /// <summary>
        /// Determina el tipo y crea la próxima mano según el ciclo Pica-Pica.
        ///
        /// Ciclo activo (5 ≤ pts &lt; 25):
        ///   slots 0,1,2 → duelo 1v1 | slot 3 → mano redonda | reinicia en 0.
        ///
        /// Cuando algún equipo supera PuntajeFinalPicaPica (25):
        ///   - Se completan los duelos que faltan en el ciclo (slots &lt; 3).
        ///   - Al cerrar el ciclo (slot llegaría a ≥3 o veníamos del slot 3) se bloquea
        ///     el Pica-Pica (slot = -2) y solo se juegan redondas hasta los 30.
        ///
        /// Valores de PicaPicaSlot:
        ///   -2 → bloqueado definitivamente (solo redondas)
        ///   -1 → no activado aún (puntaje &lt; 5)
        ///    0,1,2 → duelo Pica-Pica
        ///    3 → mano redonda del ciclo
        /// </summary>
        public static ManoTruco3v3 CrearProximaMano(
            int numeroDeMano, int ptsA, int ptsB, int prevSlot,
            Jugador j1, Jugador j2, Jugador j3, Jugador j4, Jugador j5, Jugador j6)
        {
            bool picaActivo       = ptsA >= PuntajePicaPica    || ptsB >= PuntajePicaPica;
            bool sobreUmbralFinal = ptsA >= PuntajeFinalPicaPica || ptsB >= PuntajeFinalPicaPica;

            int nextSlot;
            if (!picaActivo)
                nextSlot = -1;
            else if (prevSlot == -2)
                nextSlot = -2;
            else if (prevSlot < 0)
                nextSlot = 0;
            else
                nextSlot = (prevSlot + 1) % 4;

            if (picaActivo && sobreUmbralFinal && (nextSlot >= 3 || prevSlot == 3))
                nextSlot = -2;

            bool pica = nextSlot >= 0 && nextSlot < 3;
            var mano = pica
                ? CrearManoPicaPica(numeroDeMano, ptsA, ptsB, j1, j2, j3, j4, j5, j6)
                : CrearManoNueva   (numeroDeMano, ptsA, ptsB, j1, j2, j3, j4, j5, j6);
            mano.PicaPicaSlot = nextSlot;
            return mano;
        }

        /// <summary>
        /// Crea una mano en modo Pica-Pica: 1 vs 1 entre el par que contiene al jugador con "la mano".
        /// La mano rota igual que en 3v3 normal (J1→J2→…→J6→J1…).
        /// Pares fijos por posición opuesta: (J1,J4), (J2,J5), (J3,J6).
        /// Cuando J1 no está en el par activo, ambos duelistas son máquinas.
        /// </summary>
        public static ManoTruco3v3 CrearManoPicaPica(
            int numeroDeMano, int puntosEquipoA, int puntosEquipoB,
            Jugador j1, Jugador j2, Jugador j3, Jugador j4, Jugador j5, Jugador j6)
        {
            // Rotación continua igual que el modo normal: J1→J2→J3→J4→J5→J6→J1…
            int posicionMano = ((numeroDeMano - 1) % 6) + 1;
            var todos = new[] { j1, j2, j3, j4, j5, j6 };
            string jugadorMano = todos[posicionMano - 1].Id;

            // Par activo: los dos jugadores opuestos que contienen al mano.
            // basePar ∈ {0,1,2}  →  pares (J1,J4), (J2,J5), (J3,J6)
            int basePar = (posicionMano - 1) % 3;
            string idA = todos[basePar].Id;         // J1, J2 o J3
            string idB = todos[basePar + 3].Id;     // J4, J5 o J6

            var mano = new ManoTruco3v3
            {
                NumeroDeMano     = numeroDeMano,
                Posicion1 = j1, Posicion2 = j2, Posicion3 = j3,
                Posicion4 = j4, Posicion5 = j5, Posicion6 = j6,
                PuntosEquipoA    = puntosEquipoA,
                PuntosEquipoB    = puntosEquipoB,
                JugadorMano      = jugadorMano,
                PicaPica         = true,
                JugadoresActivos = new() { idA, idB },
            };

            mano.EquipoA.Jugador1 = j1; mano.EquipoA.Jugador2 = j3; mano.EquipoA.Jugador3 = j5;
            mano.EquipoB.Jugador1 = j2; mano.EquipoB.Jugador2 = j4; mano.EquipoB.Jugador3 = j6;

            mano.EquipoMano  = mano.ObtenerEquipoDeJugador(jugadorMano);
            mano.TurnoActual = jugadorMano;

            // Solo reparte a los duelistas; el resto queda sin cartas.
            Repartir(mano, mano.JugadoresActivos);

            return mano;
        }

        private static void Repartir(ManoTruco3v3 mano, List<string> activos)
        {
            var random = new Random();
            var mazo   = MazoServicio.CrearMazo().OrderBy(_ => random.Next()).ToList();

            foreach (var jugador in mano.OrdenJugadores)
            {
                jugador.Mano = new List<Carta>();
                jugador.Jugadas = new List<Carta>();
                if (!activos.Contains(jugador.Id)) continue;
                for (int i = 0; i < 3 && mazo.Count > 0; i++)
                {
                    jugador.Mano.Add(mazo[0]);
                    mazo.RemoveAt(0);
                }
            }
        }
    }
}
