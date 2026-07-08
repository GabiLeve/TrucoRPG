using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;
using Xunit;

namespace TrucoRPG.Tests.Logica;

public class DebugUseCasesTest
{
    private static ManoTruco ManoHistoria()
    {
        var mano = PartidaServicio.CrearManoNueva(
            configuracion: new ConfiguracionPartida { Modo = ModoJuego.Historia });
        PartidaMemoriaServicio.Guardar(mano);
        return mano;
    }

    private static ManoTruco ManoTradicional()
    {
        var mano = PartidaServicio.CrearManoNueva();
        PartidaMemoriaServicio.Guardar(mano);
        return mano;
    }

    // ── GanarAutomaticoDebugUseCase ───────────────────────────────

    [Fact]
    public void GanarAutomatico_EnHistoria_ForzaVictoriaDelHumano()
    {
        var mano = ManoHistoria();

        var resultado = new GanarAutomaticoDebugUseCase().Ejecutar(mano.Id);

        Assert.Equal(30, resultado.PuntosHumano);
        Assert.True(resultado.PartidaTerminada);
        Assert.Equal(IdJugador.Humano, resultado.GanadorPartida);
        Assert.False(resultado.SalpicaduraBloqueando);
        Assert.False(resultado.EspejismoActivo);
    }

    [Fact]
    public void GanarAutomatico_ConPartidaYaTerminada_DevuelveSinModificar()
    {
        var mano = ManoHistoria();
        mano.PartidaTerminada = true;
        mano.PuntosHumano = 12;
        PartidaMemoriaServicio.Actualizar(mano);

        var resultado = new GanarAutomaticoDebugUseCase().Ejecutar(mano.Id);

        Assert.Equal(12, resultado.PuntosHumano); // no la pisó con 30
    }

    [Fact]
    public void GanarAutomatico_EnModoTradicional_LanzaInvalidOperation()
    {
        var mano = ManoTradicional();

        Assert.Throws<InvalidOperationException>(
            () => new GanarAutomaticoDebugUseCase().Ejecutar(mano.Id));
    }

    [Fact]
    public void GanarAutomatico_ManoInexistente_LanzaKeyNotFound()
    {
        Assert.Throws<KeyNotFoundException>(
            () => new GanarAutomaticoDebugUseCase().Ejecutar(Guid.NewGuid()));
    }

    // ── SumarPuntosHumanoDebugUseCase ─────────────────────────────

    [Fact]
    public void SumarPuntos_EnModoTradicional_LanzaInvalidOperation()
    {
        var mano = ManoTradicional();

        Assert.Throws<InvalidOperationException>(
            () => new SumarPuntosHumanoDebugUseCase().Ejecutar(mano.Id));
    }

    [Fact]
    public void SumarPuntos_EnHistoriaSinMandinga_LanzaInvalidOperation()
    {
        var mano = ManoHistoria(); // historia, pero sin rival Mandinga

        Assert.Throws<InvalidOperationException>(
            () => new SumarPuntosHumanoDebugUseCase().Ejecutar(mano.Id));
    }

    [Fact]
    public void SumarPuntos_ManoInexistente_LanzaKeyNotFound()
    {
        Assert.Throws<KeyNotFoundException>(
            () => new SumarPuntosHumanoDebugUseCase().Ejecutar(Guid.NewGuid()));
    }

    // ── AvanzarMaquinaHistoriaUseCase ─────────────────────────────

    [Fact]
    public void AvanzarMaquina_ManoInexistente_LanzaKeyNotFound()
    {
        Assert.Throws<KeyNotFoundException>(
            () => new AvanzarMaquinaHistoriaUseCase().Ejecutar(Guid.NewGuid()));
    }

    [Fact]
    public void AvanzarMaquina_ConManoValida_DevuelveLaManoActualizada()
    {
        var mano = ManoHistoria();

        var (resultado, _) = new AvanzarMaquinaHistoriaUseCase().Ejecutar(mano.Id);

        Assert.Equal(mano.Id, resultado.Id);
    }
}
