using System;
using TrucoRPG.Dominio.Entities;
using System.Diagnostics.CodeAnalysis;

namespace TrucoRPG.Dominio.DTOs
{
    [ExcludeFromCodeCoverage]
    public record HeroeDto(
        Guid Id,
        string Nombre,
        string DescripcionHabilidadPasiva,
        string DescripcionHabilidadActiva,
        ClaseHeroe TipoHeroe
    );
}
