using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica
{
    public class EquiparAvatarUseCaseTest
    {
        private const string UserId = "user-1";
        private readonly Mock<IInventarioRepositorio> _inventarioMock = new();
        private readonly Mock<IUsuarioRepositorio> _usuariosMock = new();

        private EquiparAvatarUseCase CrearUseCase() =>
            new(_inventarioMock.Object, _usuariosMock.Object);

        private void ConInventario(params Inventario[] lineas) =>
            _inventarioMock
                .Setup(r => r.ObtenerInventarioDeUsuario(UserId))
                .ReturnsAsync(new List<Inventario>(lineas));

        private void ActualizarSpriteDevuelve(bool resultado) =>
            _usuariosMock
                .Setup(r => r.ActualizarSpriteAsync(UserId, It.IsAny<string>()))
                .ReturnsAsync(resultado);

        [Fact]
        public async Task Ejecutar_ConSpriteDefault_NoConsultaElInventario()
        {
            ActualizarSpriteDevuelve(true);

            bool resultado = await CrearUseCase().Ejecutar(UserId, "personaje3");

            Assert.True(resultado);
            _inventarioMock.Verify(r => r.ObtenerInventarioDeUsuario(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Ejecutar_SiNoSePuedeActualizarElSprite_LanzaInvalidOperationException()
        {
            ActualizarSpriteDevuelve(false);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearUseCase().Ejecutar(UserId, "personaje1"));
            Assert.Contains("apariencia", ex.Message);
        }

        [Fact]
        public async Task Ejecutar_ConPrendaComprada_EquipaCorrectamente()
        {
            ConInventario(new Inventario { ItemTienda = new ItemTienda { SpriteKey = "gorra" } });
            ActualizarSpriteDevuelve(true);

            bool resultado = await CrearUseCase().Ejecutar(UserId, "personaje1_gorra");

            Assert.True(resultado);
            _usuariosMock.Verify(r => r.ActualizarSpriteAsync(UserId, "personaje1_gorra"), Times.Once);
        }

        [Fact]
        public async Task Ejecutar_ConPrendaNoComprada_LanzaInvalidOperationException()
        {
            ConInventario(new Inventario { ItemTienda = new ItemTienda { SpriteKey = "sombrero" } });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearUseCase().Ejecutar(UserId, "personaje1_gorra"));
            Assert.Contains("no compraste", ex.Message);
        }

        [Fact]
        public async Task Ejecutar_ConInventarioVacio_LanzaInvalidOperationException()
        {
            ConInventario();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CrearUseCase().Ejecutar(UserId, "personaje1_gorra"));
        }

        [Fact]
        public async Task Ejecutar_IgnoraLineasSinItemOSinSpriteKey()
        {
            ConInventario(
                new Inventario { ItemTienda = null },
                new Inventario { ItemTienda = new ItemTienda { SpriteKey = "" } },
                new Inventario { ItemTienda = new ItemTienda { SpriteKey = "gorra" } });
            ActualizarSpriteDevuelve(true);

            bool resultado = await CrearUseCase().Ejecutar(UserId, "personaje2_gorra");

            Assert.True(resultado);
        }
    }
}
