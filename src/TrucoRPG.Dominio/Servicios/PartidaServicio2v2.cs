using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Crea y configura una nueva mano 2v2, reparte 12 cartas (3 por jugador).
    /// </summary>
    public static class PartidaServicio2v2
    {
        /// <summary>
        /// Crea una nueva mano 2v2.
        /// Los jugadores deben ser suministrados con sus ids/nombres ya configurados.
        /// Si no se pasan, se crean jugadores por defecto (1 humano + 3 máquinas).
        /// </summary>
        public static ManoTruco2v2 CrearManoNueva(
            int numeroDeMano = 1,
            int puntosEquipoA = 0,
            int puntosEquipoB = 0,
            Jugador? pos1 = null,
            Jugador? pos2 = null,
            Jugador? pos3 = null,
            Jugador? pos4 = null)
        {
            // Jugador mano rota: mano impar → pos1 es mano; par → pos2; etc.
            // En la implementación básica: pos1 siempre empieza en mano 1,
            // luego rota horario (pos2, pos3, pos4, pos1...)
            int posicionMano = ((numeroDeMano - 1) % 4) + 1;

            var j1 = pos1 ?? new Jugador { Id = "J1", Nombre = "Jugador 1", EsMaquina = false };
            var j2 = pos2 ?? new Jugador { Id = "J2", Nombre = "Maquina 2",  EsMaquina = true  };
            var j3 = pos3 ?? new Jugador { Id = "J3", Nombre = "Compañero",  EsMaquina = true  };
            var j4 = pos4 ?? new Jugador { Id = "J4", Nombre = "Maquina 4",  EsMaquina = true  };

            var jugadores = new[] { j1, j2, j3, j4 };
            string jugadorMano = jugadores[posicionMano - 1].Id;

            var mano = new ManoTruco2v2
            {
                NumeroDeMano  = numeroDeMano,
                Posicion1     = j1,
                Posicion2     = j2,
                Posicion3     = j3,
                Posicion4     = j4,
                PuntosEquipoA = puntosEquipoA,
                PuntosEquipoB = puntosEquipoB,
                JugadorMano   = jugadorMano,
            };

            // Configurar equipos
            mano.EquipoA.Jugador1 = j1;
            mano.EquipoA.Jugador2 = j3;
            mano.EquipoB.Jugador1 = j2;
            mano.EquipoB.Jugador2 = j4;

            mano.EquipoMano  = mano.ObtenerEquipoDeJugador(jugadorMano);
            mano.TurnoActual = jugadorMano;

            // Repartir cartas
            Repartir(mano);

            return mano;
        }

        private static void Repartir(ManoTruco2v2 mano)
        {
            var random = new Random();
            var mazo   = MazoServicio.CrearMazo().OrderBy(_ => random.Next()).ToList();

            foreach (var jugador in mano.OrdenJugadores)
            {
                jugador.Mano = new List<Carta>();
                for (int i = 0; i < 3 && mazo.Count > 0; i++)
                {
                    jugador.Mano.Add(mazo[0]);
                    mazo.RemoveAt(0);
                }
            }
        }
    }
}
