using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>Fachada de envido del modo 2v2: delega en EnvidoEquiposServicio aportando el orden de declaración y el marcador propios del modo.</summary>
    public static class EnvidoServicio2v2
    {
        public static int CalcularTantoEquipo(Equipo2v2 equipo) =>
            equipo.Jugadores.Select(TantoOriginal).DefaultIfEmpty(0).Max();

        public static int TantoOriginal(Jugador jugador) =>
            EnvidoEquiposServicio.TantoOriginal(jugador);

        public static Dictionary<string, int> CalcularTodosLosTantos(ManoTruco2v2 mano) =>
            EnvidoEquiposServicio.CalcularTodosLosTantos(mano);

        public static bool PuedeCantarEnvido(ManoTruco2v2 mano, string jugadorId) =>
            EnvidoEquiposServicio.PuedeCantarEnvido(mano, jugadorId);

        public static bool Cantar(ManoTruco2v2 mano, string jugadorId, string tipo,
                                  Func<ManoTruco2v2, string, string> responsable) =>
            EnvidoEquiposServicio.Cantar(mano, jugadorId, tipo, id => responsable(mano, id));

        public static bool Responder(ManoTruco2v2 mano, string jugadorId, bool aceptar) =>
            EnvidoEquiposServicio.Responder(mano, jugadorId, aceptar,
                () => TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano),
                (equipo, pts) => JuegoServicio2v2.SumarPuntos(mano, equipo, pts));

        public static bool Escalar(ManoTruco2v2 mano, string jugadorId, string tipo,
                                   Func<ManoTruco2v2, string, string> responsable) =>
            EnvidoEquiposServicio.Escalar(mano, jugadorId, tipo, id => responsable(mano, id));

        public static void IniciarDeclaracionTantos(ManoTruco2v2 mano) =>
            EnvidoEquiposServicio.IniciarDeclaracionTantos(mano,
                () => TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano));

        public static bool ProcesarDeclaracion(ManoTruco2v2 mano, string jugadorId, int? tanto, bool sonBuenas) =>
            EnvidoEquiposServicio.ProcesarDeclaracion(mano, jugadorId, tanto, sonBuenas,
                () => TurnoServicio2v2.ObtenerOrdenDeclaracionEnvido(mano),
                (equipo, pts) => JuegoServicio2v2.SumarPuntos(mano, equipo, pts));

        public static void ResolverNoQuiero(ManoTruco2v2 mano) =>
            EnvidoEquiposServicio.ResolverNoQuiero(mano,
                (equipo, pts) => JuegoServicio2v2.SumarPuntos(mano, equipo, pts));

        public static int ObtenerPuntosEnJuego(string? tipo) =>
            EnvidoServicio.ObtenerPuntosSegunTipo(tipo);
    }
}
