using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;

namespace TrucoRPG.Dominio.Servicios
{
    public static class PartidaServicio
    {
        public static ManoTruco CrearManoNueva(
            int numeroDeMano  = 1,
            int puntosHumano  = 0,
            int puntosMaquina = 0,
            ConfiguracionPartida? configuracion = null)
        {
            var config = configuracion ?? new ConfiguracionPartida();

            var mano = new ManoTruco
            {
                Configuracion = config,
                Humano = new Jugador
                {
                    Id = IdJugador.Humano,
                    Nombre = "Usuario",
                    EsMaquina = false
                },
                Maquina = new Jugador
                {
                    Id = IdJugador.Maquina,
                    Nombre = "Maquina",
                    EsMaquina = true
                },
                NumeroDeMano   = numeroDeMano,
                PuntosHumano   = puntosHumano,
                PuntosMaquina  = puntosMaquina,
                ManoIniciadaPor = numeroDeMano % 2 == 1 ? IdJugador.Humano : IdJugador.Maquina,
            };

            mano.TurnoActual = mano.ManoIniciadaPor;

            if (config.HabilidadesActivas)
                mano.EstadoHabilidades.ObtenerOCrear(IdJugador.Humano, config.HeroeDelHumano);

            mano.RepartoContext = new RepartoContext();

            if (config.HabilidadesActivas)
                HabilidadesOrquestador.Disparar(mano, EventoPartida.AntesDeRepartir);

            RepartoServicio.Repartir(mano);

            if (config.HabilidadesActivas)
                HabilidadesOrquestador.Disparar(mano, EventoPartida.DespuesDeRepartir);

            return mano;
        }
    }
}
