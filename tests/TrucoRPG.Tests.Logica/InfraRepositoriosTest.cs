using Microsoft.EntityFrameworkCore;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Infraestructura.Data;
using TrucoRPG.Infraestructura.Repositorios;
using Xunit;

namespace TrucoRPG.Tests.Logica;

/// <summary>
/// Tests de los repositorios EF con base InMemory. Cada test usa una DB con
/// nombre único para aislarse. Instanciar el contexto también ejecuta las
/// Configurations (HeroeConfiguration, RivalConfiguration, etc.).
/// </summary>
public class InfraRepositoriosTest
{
    private static AppDbContext CrearDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"test-{Guid.NewGuid()}")
            .Options);

    // ── RivalRepositorio ──────────────────────────────────────────

    [Fact]
    public async Task Rival_ObtenerTodos_DevuelveOrdenadoPorNivel()
    {
        using var db = CrearDb();
        db.Rivales.AddRange(
            new Rival { Nivel = 2, Nombre = "Dos" },
            new Rival { Nivel = 1, Nombre = "Uno" });
        await db.SaveChangesAsync();

        var repo = new RivalRepositorio(db);
        var rivales = await repo.ObtenerTodosAsync();

        Assert.Equal(2, rivales.Count);
        Assert.Equal("Uno", rivales[0].Nombre);
    }

    [Fact]
    public async Task Rival_ObtenerPorNivelYPorTipo_EncuentranAlRival()
    {
        using var db = CrearDb();
        db.Rivales.Add(new Rival { Nivel = 3, Nombre = "Mandinga", TipoRival = ClaseRival.Mandinga });
        await db.SaveChangesAsync();

        var repo = new RivalRepositorio(db);

        Assert.NotNull(await repo.ObtenerPorNivelAsync(3));
        Assert.Null(await repo.ObtenerPorNivelAsync(99));
        Assert.NotNull(await repo.ObtenerPorTipoAsync(ClaseRival.Mandinga));
    }

    // ── ItemTiendaRepositorio ─────────────────────────────────────

    [Fact]
    public async Task ItemTienda_ObtenerPorIdYTodos_Funcionan()
    {
        using var db = CrearDb();
        db.Items.AddRange(
            new ItemTienda { Id = 1, Nombre = "Mate", Categoria = "General" },
            new ItemTienda { Id = 2, Nombre = "Facón", Categoria = "General" });
        await db.SaveChangesAsync();

        var repo = new ItemTiendaRepositorio(db);

        Assert.Equal("Mate", (await repo.ObtenerItemPorIdAsync(1))?.Nombre);
        Assert.Null(await repo.ObtenerItemPorIdAsync(99));
        Assert.Equal(2, (await repo.ObtenerTodosLosItemsAsync()).Count);
    }

    // ── ProgresoPartidaRepositorio ────────────────────────────────

    [Fact]
    public async Task Progreso_ObtenerOCrear_CreaSiNoExisteYReusaSiExiste()
    {
        using var db = CrearDb();
        var repo = new ProgresoPartidaRepositorio(db);

        var creado = await repo.ObtenerOCrearAsync("user-1");
        var reusado = await repo.ObtenerOCrearAsync("user-1");

        Assert.Equal(0, creado.UltimoRivalDerrotadoNivel);
        Assert.Equal(creado.Id, reusado.Id);
        Assert.NotNull(await repo.ObtenerPorUsuarioIdAsync("user-1"));
        Assert.Null(await repo.ObtenerPorUsuarioIdAsync("otro"));
    }

    [Fact]
    public async Task Progreso_RegistrarVictoria_AvanzaNivelYSumaPuntos()
    {
        using var db = CrearDb();
        var repo = new ProgresoPartidaRepositorio(db);

        await repo.RegistrarVictoriaAsync("user-1", rivalNivelDerrotado: 1, diferenciaPuntos: 10);
        var progreso = await repo.ObtenerPorUsuarioIdAsync("user-1");

        Assert.Equal(1, progreso!.UltimoRivalDerrotadoNivel);
        Assert.Equal(10, progreso.PuntosAcumulados);
    }

    [Fact]
    public async Task Progreso_RegistrarVictoria_ReVencerRivalAnterior_NoBajaElNivel()
    {
        using var db = CrearDb();
        var repo = new ProgresoPartidaRepositorio(db);
        await repo.RegistrarVictoriaAsync("user-1", 1, 5);
        await repo.RegistrarVictoriaAsync("user-1", 2, 5);

        await repo.RegistrarVictoriaAsync("user-1", 1, 3); // vuelve a ganarle al 1

        var progreso = await repo.ObtenerPorUsuarioIdAsync("user-1");
        Assert.Equal(2, progreso!.UltimoRivalDerrotadoNivel);
        Assert.Equal(13, progreso.PuntosAcumulados);
    }

    [Fact]
    public async Task Progreso_RegistrarVictoria_SalteandoNiveles_Lanza()
    {
        using var db = CrearDb();
        var repo = new ProgresoPartidaRepositorio(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => repo.RegistrarVictoriaAsync("user-1", rivalNivelDerrotado: 3, diferenciaPuntos: 5));
    }

    // ── InventarioRepositorio ─────────────────────────────────────

    [Fact]
    public async Task Inventario_Agregar_NuevoItem_CreaEntrada()
    {
        using var db = CrearDb();
        var repo = new InventarioRepositorio(db);

        var ok = await repo.Agregar("user-1", 1, 1);

        Assert.True(ok);
        Assert.True(await repo.ItemExistente("user-1", 1));
        Assert.False(await repo.ItemExistente("user-1", 2));
    }

    [Fact]
    public async Task Inventario_Agregar_ItemExistente_AcumulaCantidad()
    {
        using var db = CrearDb();
        // ObtenerInventarioDeUsuario hace Include(ItemTienda): el item debe existir
        db.Items.Add(new ItemTienda { Id = 1, Nombre = "Mate", Categoria = "General" });
        await db.SaveChangesAsync();

        var repo = new InventarioRepositorio(db);
        await repo.Agregar("user-1", 1, 1);

        await repo.Agregar("user-1", 1, 2);

        var inventario = await repo.ObtenerInventarioDeUsuario("user-1");
        Assert.Single(inventario);
        Assert.Equal(3, inventario[0].Cantidad);
    }

    [Fact]
    public async Task Inventario_Eliminar_ItemExistente_LoQuita()
    {
        using var db = CrearDb();
        var repo = new InventarioRepositorio(db);
        await repo.Agregar("user-1", 1, 1);

        var ok = await repo.Eliminar("user-1", 1);

        Assert.True(ok);
        Assert.False(await repo.ItemExistente("user-1", 1));
    }

    [Fact]
    public async Task Inventario_Eliminar_ItemInexistente_DevuelveFalse()
    {
        using var db = CrearDb();
        var repo = new InventarioRepositorio(db);

        Assert.False(await repo.Eliminar("user-1", 99));
    }

    [Fact]
    public async Task Inventario_ObtenerDeUsuario_SoloDevuelveLoSuyo()
    {
        using var db = CrearDb();
        db.Items.AddRange(
            new ItemTienda { Id = 1, Nombre = "Mate", Categoria = "General" },
            new ItemTienda { Id = 2, Nombre = "Facón", Categoria = "General" });
        await db.SaveChangesAsync();

        var repo = new InventarioRepositorio(db);
        await repo.Agregar("user-1", 1, 1);
        await repo.Agregar("user-2", 2, 1);

        var inventario = await repo.ObtenerInventarioDeUsuario("user-1");

        Assert.Single(inventario);
        Assert.Equal(1, inventario[0].ItemTiendaId);
    }
}
