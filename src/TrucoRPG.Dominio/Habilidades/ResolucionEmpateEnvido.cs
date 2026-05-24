namespace TrucoRPG.Dominio.Habilidades
{
  /// <summary>
  /// Payload mutable para resolver empate de envido (Fanfarrón u otros héroes futuros).
  /// </summary>
  public class ResolucionEmpateEnvido
  {
      public string GanadorPorMano { get; set; } = "";
      public string GanadorFinal { get; set; } = "";
  }
}
