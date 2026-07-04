namespace TrucoRPG.Dominio.Habilidades
{
    public class VistaHabilidadesRival
    {
        public bool HabilidadesActivasEnPartida { get; set; }
        public bool SalpicaduraActiva { get; set; }
        public bool SalpicaduraBloqueando { get; set; }
        public bool TravesuraActiva { get; set; }
        public bool TravesuraBloqueando { get; set; }
        public bool RasgunoActivo { get; set; }
        public bool RasgunoBloqueando { get; set; }
        public bool AullidoBloqueando { get; set; }
        public bool DestelloBloqueando { get; set; }
        public bool EspejismoActivo { get; set; }
        public bool EspejismoBloqueando { get; set; }
        public bool EspejismoAlternando { get; set; }
        public bool EspejismoMostrarFakePrimero { get; set; }
        public CartaReferencia? EspejismoCartaFalsa { get; set; }
        public List<CartaReferencia> CartasOcultasTravesura { get; set; } = [];
        public string? UltimoMensajeHabilidad { get; set; }
    }

    public class CartaReferencia
    {
        public int Numero { get; set; }
        public string Palo { get; set; } = "";
    }
}
