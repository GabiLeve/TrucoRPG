namespace TrucoRPG.Dominio.Habilidades
{
    public class ModificadorPuntos
    {
        public string? GanadorId { get; set; }
        public int PuntosBase { get; set; }
        public int PuntosFinales { get; set; }
        public int BonusAlRival { get; set; }
        public bool DuplicarPuntosGanador { get; set; }
        public string? Origen { get; set; }
        public string? CantorId { get; set; }

        public void AplicarBase(int puntos, string? ganadorId, string? origen = null, string? cantorId = null)
        {
            GanadorId = ganadorId;
            PuntosBase = puntos;
            PuntosFinales = puntos;
            Origen = origen;
            CantorId = cantorId;
        }

        public int PuntosParaGanador()
        {
            int total = PuntosFinales;
            if (DuplicarPuntosGanador)
                total *= 2;
            return total;
        }

        public string? RivalDe(string ganadorId) =>
            ganadorId == Entities.IdJugador.Humano
                ? Entities.IdJugador.Maquina
                : ganadorId == Entities.IdJugador.Maquina
                    ? Entities.IdJugador.Humano
                    : null;
    }
}
