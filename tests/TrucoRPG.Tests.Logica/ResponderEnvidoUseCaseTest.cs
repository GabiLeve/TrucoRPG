using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica
{
    public class ResponderEnvidoUseCaseTest
    {
        [Fact]
        public void Ejecutar_SiManoNoExiste_DebeLanzarKeyNotFoundException()
        {
            //Given
            var useCase = new ResponderEnvidoUseCase();
            Guid idInexistente = Guid.NewGuid();

            //When
            Action act = () => useCase.Ejecutar(idInexistente, aceptar: true, escalarA: null);

            //Then
            Assert.Throws<KeyNotFoundException>(act);
        }

        [Fact]
        public void Ejecutar_SiPartidaYaTermino_DebeLanzarInvalidOperationException()
        {
            // Given
            var useCase = new ResponderEnvidoUseCase();
            var mano = CrearManoBase();
            mano.PartidaTerminada = true; 
            PartidaMemoriaServicio.Actualizar(mano);

            // When
            Action act = () => useCase.Ejecutar(mano.Id, aceptar: true, escalarA: null);

            // Then
            var ex = Assert.Throws<InvalidOperationException>(act );
            Assert.Contains("La partida ya terminó", ex.Message);
        }

        [Fact]
        public void Ejecutar_SiNoHayEnvidoCantado_DebeLanzarInvalidOperationException()
        {
            // Given
            var useCase = new ResponderEnvidoUseCase();
            var mano = CrearManoBase();
            mano.EnvidoCantado = false; // Incumple regla

            PartidaMemoriaServicio.Actualizar(mano);

            // When
            Action act = () => useCase.Ejecutar(mano.Id, aceptar: true, escalarA: null);
            // Then
            Assert.Throws<InvalidOperationException>(act );
        }

        [Fact]
        public void Ejecutar_CuandoHumanoNoAcepta_DebeDarPuntoAMaquinaYResolver()
        {
            // Given
            var useCase = new ResponderEnvidoUseCase();
            var mano = CrearManoBase();
            mano.EnvidoCantado = true;
            mano.EnvidoPendienteRespuestaHumano = true;

            PartidaMemoriaServicio.Actualizar(mano);

            // When
            var resultado = useCase.Ejecutar(mano.Id, aceptar: false, escalarA: null);

            // Then
            Assert.False(resultado.EnvidoPendienteRespuestaHumano);
            Assert.True(resultado.EnvidoResuelto);
            Assert.Equal("Maquina", resultado.GanadorEnvido);
            Assert.Equal(1, resultado.PuntosEnvido);
        }

        [Fact]
        public void Ejecutar_CuandoHumanoAceptaSinEscalar_DebeResolverElEnvido()
        {
            // Given
            var useCase = new ResponderEnvidoUseCase();
            var mano = CrearManoBase();
            mano.EnvidoCantado = true;
            mano.EnvidoPendienteRespuestaHumano = true;
            mano.TipoEnvidoCantado = "Envido";

            PartidaMemoriaServicio.Actualizar(mano);

            // When
            var resultado = useCase.Ejecutar(mano.Id, aceptar: true, escalarA: null);

            // Then
            Assert.False(resultado.EnvidoPendienteRespuestaHumano);
        }

        [Fact]
        public void Ejecutar_CuandoEscalacionInvalida_DebeLanzarInvalidOperationException()
        {
            // Given
            var useCase = new ResponderEnvidoUseCase();
            var mano = CrearManoBase();
            mano.EnvidoCantado = true;
            mano.EnvidoPendienteRespuestaHumano = true;
            mano.TipoEnvidoCantado = "Real Envido";

            PartidaMemoriaServicio.Actualizar(mano);

            // When
            Action act = () => useCase.Ejecutar(mano.Id, aceptar: true, escalarA: "Envido");
            // Then
            Assert.Throws<InvalidOperationException>(act );
        }

        [Fact]
        public void Ejecutar_CuandoHumanoEscalaValidoYMaquinaNoQuiere_DebeDarPuntosPreviosAHumano()
        {
            // Given
            var useCase = new ResponderEnvidoUseCase();
            var mano = CrearManoBase();
            mano.EnvidoCantado = true;
            mano.EnvidoPendienteRespuestaHumano = true;
            mano.TipoEnvidoCantado = "Envido";

            // Forzamos a la máquina a no querer dándole cartas malísimas (ej: un 4 y un 5 cruzados de distinto palo)
            mano.Maquina = new()
            {
                Mano = new List<Carta> { new Carta { Numero = 4, Palo = "Copa" }, new Carta { Numero = 5, Palo = "Espada" } }
            };
            mano.NivelMentiraEnvidoMaquina = 0;

            PartidaMemoriaServicio.Actualizar(mano);

            // When
            var resultado = useCase.Ejecutar(mano.Id, aceptar: true, escalarA: "Real Envido");

            // Then
            Assert.True(resultado.EnvidoResuelto);
            Assert.Equal("Humano", resultado.GanadorEnvido);
            Assert.Contains("La máquina no quiso", resultado.EstadoEnvido);
        }

        private static ManoTruco CrearManoBase()
        {
            return new ManoTruco
            {
                Id = Guid.NewGuid(),
                PartidaTerminada = false,
                EnvidoCantado = true,
                EnvidoPendienteRespuestaHumano = true,
                SalpicaduraBloqueando = false,
                TravesuraBloqueando = false,
                RasgunoBloqueando = false,
                AullidoBloqueando = false,
                Maquina = new() { Mano = new List<Carta>() },
                Humano = new() { Mano = new List<Carta>() },
                Bazas = new List<Baza>()
            };
        }
    }
}
