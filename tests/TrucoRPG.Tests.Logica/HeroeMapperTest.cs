using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Mapeos;
using Xunit;

namespace TrucoRPG.Tests.Logica;

public class HeroeMapperTest
{
    private static Heroe HeroeDemo(string nombre = "El Gaucho") => new()
    {
        Nombre = nombre,
        DescripcionHabilidadPasiva = "Pasiva",
        DescripcionHabilidadActiva = "Activa",
    };

    [Fact]
    public void ToDto_MapeaTodosLosCampos()
    {
        var heroe = HeroeDemo();

        var dto = heroe.ToDto();

        Assert.Equal(heroe.Id, dto.Id);
        Assert.Equal("El Gaucho", dto.Nombre);
        Assert.Equal("Pasiva", dto.DescripcionHabilidadPasiva);
        Assert.Equal("Activa", dto.DescripcionHabilidadActiva);
        Assert.Equal(heroe.TipoHeroe, dto.TipoHeroe);
    }

    [Fact]
    public void ToDto_SobreLista_MapeaCadaElemento()
    {
        var heroes = new List<Heroe> { HeroeDemo("Uno"), HeroeDemo("Dos") };

        var dtos = heroes.ToDto();

        Assert.Equal(2, dtos.Count);
        Assert.Equal("Uno", dtos[0].Nombre);
        Assert.Equal("Dos", dtos[1].Nombre);
    }
}
