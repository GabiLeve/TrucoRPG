namespace TrucoRPG.Dominio.Servicios
{
    public static class AzarServicio
    {
        public const double TimberoProbCara = 0.20;

        public static Func<bool>? MonedaCaraOverride { get; set; }

        public static bool MonedaCara(double probabilidadCara = 0.5) =>
            MonedaCaraOverride?.Invoke() ?? Random.Shared.NextDouble() < probabilidadCara;
    }
}
