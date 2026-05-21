using System;
using TrucoRPG.Entidades;


namespace TrucoRPG.Logica.DTOs
{
    /// <summary>
    /// DTO inicial para exponer información de héroes (sin lógica de juego).
    /// </summary>
    public record HeroeDto(
        Guid Id,
        string Nombre,
        string DescripcionHabilidadPasiva,
        string DescripcionHabilidadActiva,
        ClaseHeroe TipoHeroe
    );
}
