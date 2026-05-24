namespace TrucoRPG.Dominio.Habilidades
{
    /// <summary>
    /// Momentos del juego en los que una habilidad puede intervenir.
    /// Los use cases disparan eventos; cada héroe decide si reacciona.
    /// </summary>
    public enum EventoPartida
    {
        PartidaIniciada,
        ManoIniciada,
        AntesDeRepartir,
        DespuesDeRepartir,
        EmpateEnvido,
        AntesDeSumarPuntos,
        TrucoAceptado,
        EnvidoAceptado,
        ManoFinalizada
    }
}
