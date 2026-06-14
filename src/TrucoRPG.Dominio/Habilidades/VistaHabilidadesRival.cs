namespace TrucoRPG.Dominio.Habilidades
{
    public class VistaHabilidadesRival
    {
        public bool HabilidadesActivasEnPartida { get; set; }
        public bool SalpicaduraActiva { get; set; }
        public bool SalpicaduraBloqueando { get; set; }
        public bool TravesuraActiva { get; set; }
        public bool TravesuraBloqueando { get; set; }
        public List<CartaReferencia> CartasOcultasTravesura { get; set; } = [];
        public string? UltimoMensajeHabilidad { get; set; }
    }

    public class CartaReferencia
    {
        public int Numero { get; set; }
        public string Palo { get; set; } = "";
    }
}
