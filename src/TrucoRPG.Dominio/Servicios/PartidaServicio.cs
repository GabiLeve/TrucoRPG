using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Lógica de negocio para iniciar y gestionar partidas (single-player y multi).
    /// </summary>
    public static class PartidaServicio
    {
        /// <summary>
        /// Crea una mano nueva con mazo repartido.
        /// Acepta parámetros opcionales para continuidad de partida.
        /// </summary>
        public static ManoTruco CrearManoNueva(
            int numeroDeMano  = 1,
            int puntosHumano  = 0,
            int puntosMaquina = 0)
        {
            var mano = new ManoTruco
            {
                Humano = new Jugador { Nombre = "Usuario",  EsMaquina = false },
                Maquina = new Jugador { Nombre = "Maquina", EsMaquina = true  },
                NumeroDeMano   = numeroDeMano,
                PuntosHumano   = puntosHumano,
                PuntosMaquina  = puntosMaquina,
                ManoIniciadaPor = numeroDeMano % 2 == 1 ? "Humano" : "Maquina",
            };

            mano.TurnoActual = mano.ManoIniciadaPor;

            RepartoServicio.Repartir(mano);
            return mano;
        }
    }
}
