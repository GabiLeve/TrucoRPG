using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica;

public class PartidaMemoriaServicioTests
{
    // Helper: crea una ManoTruco mínima con Id nuevo
    private static ManoTruco ManoNueva() => PartidaServicio.CrearManoNueva();

    // ─── Guardar y Obtener ───────────────────────────────────────────

    [Fact]
    public void Guardar_LuegoObtener_RetornaLaMismaPartida()
    {
        var mano = ManoNueva();
        PartidaMemoriaServicio.Guardar(mano);

        var recuperada = PartidaMemoriaServicio.Obtener(mano.Id);
        Assert.NotNull(recuperada);
        Assert.Equal(mano.Id, recuperada!.Id);
    }

    [Fact]
    public void Obtener_IdInexistente_RetornaNull()
    {
        var idFalso = Guid.NewGuid();
        var resultado = PartidaMemoriaServicio.Obtener(idFalso);
        Assert.Null(resultado);
    }

    [Fact]
    public void Guardar_VariasPartidas_CadaUnaRecuperableConSuId()
    {
        var mano1 = ManoNueva();
        var mano2 = ManoNueva();

        PartidaMemoriaServicio.Guardar(mano1);
        PartidaMemoriaServicio.Guardar(mano2);

        Assert.Equal(mano1.Id, PartidaMemoriaServicio.Obtener(mano1.Id)!.Id);
        Assert.Equal(mano2.Id, PartidaMemoriaServicio.Obtener(mano2.Id)!.Id);
    }

    // ─── Actualizar ──────────────────────────────────────────────────

    [Fact]
    public void Actualizar_ModificaElEstadoPersistido()
    {
        var mano = ManoNueva();
        PartidaMemoriaServicio.Guardar(mano);

        mano.PuntosHumano = 7;
        PartidaMemoriaServicio.Actualizar(mano);

        var recuperada = PartidaMemoriaServicio.Obtener(mano.Id);
        Assert.Equal(7, recuperada!.PuntosHumano);
    }

    [Fact]
    public void Guardar_MismoId_SobreescribePartidaAnterior()
    {
        var mano = ManoNueva();
        PartidaMemoriaServicio.Guardar(mano);

        mano.PuntosMaquina = 10;
        PartidaMemoriaServicio.Guardar(mano); // segundo Guardar con mismo Id

        var recuperada = PartidaMemoriaServicio.Obtener(mano.Id);
        Assert.Equal(10, recuperada!.PuntosMaquina);
    }
}
