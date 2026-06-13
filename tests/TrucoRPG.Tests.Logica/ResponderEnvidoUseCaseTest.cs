using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica
{
    public class ResponderEnvidoUseCaseTest
    {
        private static ManoTruco CrearMano()
        {
            var mano = new ManoTruco();

            mano.Id = Guid.NewGuid();

            mano.EnvidoCantado = true;
            mano.EnvidoPendienteRespuestaHumano = true;
            mano.TipoEnvidoCantado = "Real Envido";

            mano.Maquina = new Jugador
            {
                Mano = new List<Carta>()
            };

            return mano;
        }

        [Fact]
        public void Ejecutar_EscalarInvalidoDeberiaLanzarExcepcion()
        {
            //Given
            var mano = CrearMano();

            PartidaMemoriaServicio.Guardar(mano);

            var sut = new ResponderEnvidoUseCase();

            //When
            var ex = Assert.Throws<InvalidOperationException>(() =>
                sut.Ejecutar(
                    mano.Id,
                    aceptar: true,
                    escalarA: "Envido"
                ));

            //Then
            Assert.Contains( "No podés escalar",ex.Message);
        }

        [Fact]
        public void Ejecutar_EscalarYMaquinaNoAcepta()
        {
            //Given
            var mano = CrearMano();

            mano.TipoEnvidoCantado = "Envido";

            mano.NivelMentiraEnvidoMaquina = 0;

            PartidaMemoriaServicio.Guardar(mano);

            var sut = new ResponderEnvidoUseCase();

            //When
            var resultado = sut.Ejecutar(
                mano.Id,
                aceptar: true,
                escalarA: "Real Envido"
            );

            //Then
            Assert.False(resultado.EnvidoPendienteRespuestaHumano);
            Assert.Equal("Humano", resultado.GanadorEnvido);
            Assert.True(resultado.EnvidoResuelto);
            Assert.Contains("no quiso", resultado.EstadoEnvido);
        }

        [Fact]
        public void Ejecutar_EscalarYMaquinaAcepta()
        {
            //Given
            var mano = CrearMano();

            mano.TipoEnvidoCantado = "Envido";

            mano.NivelMentiraEnvidoMaquina = 100;

            PartidaMemoriaServicio.Guardar(mano);

            var sut = new ResponderEnvidoUseCase();

            //When
            var resultado = sut.Ejecutar(
                mano.Id,
                aceptar: true,
                escalarA: "Real Envido"
            );

            //Then
            Assert.True(resultado.EnvidoResuelto);
            Assert.False(resultado.EnvidoPendienteRespuestaHumano);
            Assert.Contains("quiso",resultado.EstadoEnvido);
        }

        [Fact]
        public void Ejecutar_QuieroSinEscalarDeberiaResolver()
        {
            //Given
            var mano = CrearMano();

            mano.TipoEnvidoCantado = "Envido";

            PartidaMemoriaServicio.Guardar(mano);

            var sut = new ResponderEnvidoUseCase();

            //When
            var resultado = sut.Ejecutar(
                mano.Id,
                aceptar: true,
                escalarA: null
            );

            //Then
            Assert.True(resultado.EnvidoResuelto);
            Assert.False(resultado.EnvidoPendienteRespuestaHumano);
        }
    }
}
