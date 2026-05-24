namespace TrucoRPG.Dominio.Habilidades
{
    public class ResultadoActivarHabilidad
    {
        public bool Exito { get; init; }
        public string Mensaje { get; init; } = "";

        public static ResultadoActivarHabilidad Ok(string mensaje) =>
            new() { Exito = true, Mensaje = mensaje };

        public static ResultadoActivarHabilidad Error(string mensaje) =>
            new() { Exito = false, Mensaje = mensaje };
    }
}
