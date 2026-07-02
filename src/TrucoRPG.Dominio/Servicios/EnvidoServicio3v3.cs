using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>Fachada de envido del modo 3v3: delega en EnvidoEquiposServicio aportando el orden de declaración y el marcador propios del modo.</summary>
    public static class EnvidoServicio3v3
    {
        public static int CalcularTantoEquipo(Equipo3v3 equipo) =>
            equipo.Jugadores.Select(TantoOriginal).DefaultIfEmpty(0).Max();

        public static int TantoOriginal(Jugador jugador) =>
            EnvidoEquiposServicio.TantoOriginal(jugador);

        public static Dictionary<string, int> CalcularTodosLosTantos(ManoTruco3v3 mano) =>
            EnvidoEquiposServicio.CalcularTodosLosTantos(mano);

        public static bool PuedeCantarEnvido(ManoTruco3v3 mano, string jugadorId) =>
            EnvidoEquiposServicio.PuedeCantarEnvido(mano, jugadorId);

        public static bool Cantar(ManoTruco3v3 mano, string jugadorId, string tipo,
                                  Func<ManoTruco3v3, string, string> responsable) =>
            EnvidoEquiposServicio.Cantar(mano, jugadorId, tipo, id => responsable(mano, id));

        public static bool Responder(ManoTruco3v3 mano, string jugadorId, bool aceptar) =>
            EnvidoEquiposServicio.Responder(mano, jugadorId, aceptar,
                () => TurnoServicio3v3.ObtenerOrdenDeclaracionEnvido(mano),
                (equipo, pts) => JuegoServicio3v3.SumarPuntos(mano, equipo, pts));

        public static bool Escalar(ManoTruco3v3 mano, string jugadorId, string tipo,
                                   Func<ManoTruco3v3, string, string> responsable) =>
            EnvidoEquiposServicio.Escalar(mano, jugadorId, tipo, id => responsable(mano, id));

        public static void IniciarDeclaracionTantos(ManoTruco3v3 mano) =>
            EnvidoEquiposServicio.IniciarDeclaracionTantos(mano,
                () => TurnoServicio3v3.ObtenerOrdenDeclaracionEnvido(mano));

        public static bool ProcesarDeclaracion(ManoTruco3v3 mano, string jugadorId, int? tanto, bool sonBuenas) =>
            EnvidoEquiposServicio.ProcesarDeclaracion(mano, jugadorId, tanto, sonBuenas,
                () => TurnoServicio3v3.ObtenerOrdenDeclaracionEnvido(mano),
                (equipo, pts) => JuegoServicio3v3.SumarPuntos(mano, equipo, pts));

        public static void ResolverNoQuiero(ManoTruco3v3 mano) =>
            EnvidoEquiposServicio.ResolverNoQuiero(mano,
                (equipo, pts) => JuegoServicio3v3.SumarPuntos(mano, equipo, pts));

        public static int ObtenerPuntosEnJuego(string? tipo) =>
            EnvidoServicio.ObtenerPuntosSegunTipo(tipo);
    }
}
