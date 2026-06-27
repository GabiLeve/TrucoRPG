namespace TrucoRPG.API.Models
{
    public class CategoriaReglasDto
    {
        public string Categoria { get; set; } = string.Empty;
        public List<ReglasDetalleDto> Detalle { get; set; } = new List<ReglasDetalleDto>();

    }
}
