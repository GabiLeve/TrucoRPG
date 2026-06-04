using System;
using TrucoRPG.Dominio.Entities;


namespace TrucoRPG.Dominio.DTOs
{
    public record HeroeDto(
        Guid Id,
        string Nombre,
        string DescripcionHabilidadPasiva,
        string DescripcionHabilidadActiva,
        ClaseHeroe TipoHeroe
    );
}
