namespace TrucoRPG.Dominio.Servicios
{
    /// <summary>
    /// Azar del dominio (moneda, etc.). En tests se puede fijar el resultado.
    /// </summary>
    public static class AzarServicio
    {
        public const double TimberoProbCara = 0.20;

        /// <summary>Si está definido, reemplaza el lanzamiento de moneda (true = cara).</summary>
        public static Func<bool>? MonedaCaraOverride { get; set; }

        public static bool MonedaCara(double probabilidadCara = 0.5) =>
            MonedaCaraOverride?.Invoke() ?? Random.Shared.NextDouble() < probabilidadCara;
    }
}
