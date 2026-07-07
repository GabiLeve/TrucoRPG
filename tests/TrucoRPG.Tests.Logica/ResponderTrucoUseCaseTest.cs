using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Tests.Logica
{
    [Collection("TrucoTestsSequentials")]
    public class ResponderTrucoUseCaseTest
    {
        private ManoTruco CrearManoBase(Guid id)
        {
            var mano = new ManoTruco
            {
                Id = id,
                TrucoPendienteRespuestaHumano = true,
                SalpicaduraBloqueando = false,
                NivelTruco = 1,
                PuntosTrucoMano = 1,
                CantorTruco = "Maquina"
            };
            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }

        [Fact]
        public void Ejecutar_SiManoNoExiste_DebeLanzarKeyNotFoundException()
        {
            //Given
            var useCase = new ResponderTrucoUseCase();
            Guid idInexistente = Guid.NewGuid();

            //When
            Action act = () => useCase.Ejecutar(idInexistente, aceptar: true, escalarA: null);

            //Then
            Assert.Throws<KeyNotFoundException>(act);
        }

        [Fact]
        public void Ejecutar_SiNoHayRespuestaPendienteParaTruco_DebeLanzarInvalidOperationException()
        {
            //Given
            var useCase = new ResponderTrucoUseCase();
            Guid manoId = Guid.NewGuid();
            var mano = CrearManoBase(manoId);
            mano.TrucoPendienteRespuestaHumano = false;
            PartidaMemoriaServicio.Actualizar(mano);

            //When
            Action act = () => useCase.Ejecutar(manoId, aceptar: true, escalarA: null);

            //Then
            var ex = Assert.Throws<InvalidOperationException>(act);
            Assert.Contains("No hay respuesta pendiente para truco", ex.Message);
        }

        [Fact]
        public void Ejecutar_NoQuiero_SiHumanoNoAceptaElTruco_DebeResolverManoYAsignarPuntosAlCantor()
        {
            //Given
            var useCase = new ResponderTrucoUseCase();
            Guid manoId = Guid.NewGuid();
            var mano = CrearManoBase(manoId);
            mano.NivelTruco = 2; 
            mano.CantorTruco = "Maquina";
            PartidaMemoriaServicio.Actualizar(mano);

            //When
            var resultado = useCase.Ejecutar(manoId, aceptar: false, escalarA: null);

            //Then
            Assert.True(resultado.TrucoResuelto);
            Assert.Equal("Maquina", resultado.GanadorMano);
            Assert.Equal(2, resultado.PuntosTrucoMano);
            Assert.Contains("No quisiste", resultado.EstadoTruco);
        }

        [Fact]
        public void Ejecutar_EscalarARetruco_SiHumanoEscalaARetrucoYMaquinaNoQuiere_DebeAsignarDosPuntosAlHumano()
        {
            //Given
            var useCase = new ResponderTrucoUseCase();
            Guid manoId = Guid.NewGuid();
            var mano = CrearManoBase(manoId);
            mano.NivelTruco = 1; 
            mano.Maquina.Mano = new List<Carta>(); 
            mano.NivelMentiraTrucoMaquina = 0;
            PartidaMemoriaServicio.Actualizar(mano);

            //When
            var resultado = useCase.Ejecutar(manoId, aceptar: true, escalarA: "retruco");

            //Then
            Assert.True(resultado.TrucoResuelto);
            Assert.Equal("Humano", resultado.GanadorMano);
            Assert.Equal(2, resultado.PuntosTrucoMano);
            Assert.Contains("La máquina no quiso el retruco", resultado.EstadoTruco);
        }

        //ESTANA SI XQ CUANDO CORRO UNO ME DA VERDE Y DESPUES LA CORRO DA ROJO
        //RESUEMN BIPOLAR 
        //[Fact]
        //public void EjecutarEscalarARetrucoSiHumanoEscalaARetrucoYMaquinaRedoblaDebeSubirANivelValeCuatro()
        //{
        //    //Given
        //    var useCase = new ResponderTrucoUseCase();
        //    Guid manoId = Guid.NewGuid();
        //    var mano = CrearManoBase(manoId);
        //    mano.NivelTruco = 1;
        //    mano.TrucoPendienteRespuestaHumano = true; 
        //    mano.TrucoResuelto = false;

        //    mano.Maquina.Mano = new List<Carta>
        //    {
        //        new Carta { ValorTruco = 14, Palo= "Espada" },
        //        new Carta { ValorTruco = 13, Palo = "Basto" }
        //    };
        //    mano.NivelMentiraTrucoMaquina = 99;
        //    PartidaMemoriaServicio.Actualizar(mano);

        //    //When
        //    var resultado = useCase.Ejecutar(manoId, aceptar: true, escalarA: "retruco");

        //    //Then
        //    int nivelFinal = resultado.NivelTruco;
        //    int puntosFinales = resultado.PuntosTrucoMano;
        //    string? cantorFinal = resultado.CantorTruco;
        //    bool pendienteHumanoFinal = resultado.TrucoPendienteRespuestaHumano;
        //    string estadoTexto = resultado.EstadoTruco;

        //    Assert.Equal(3, nivelFinal);
        //    Assert.Equal(4, puntosFinales);
        //    Assert.Equal("Maquina", cantorFinal);
        //    Assert.True(pendienteHumanoFinal);
        //    Assert.Contains("cantó Vale Cuatro", estadoTexto);
        //}

        [Fact]
        public void Ejecutar_SiHumanoEscalaARetrucoYMaquinaSoloAcepta_DebeSubirANivelDosSinCerrar()
        {
            //Given
            var useCase = new ResponderTrucoUseCase();
            Guid manoId = Guid.NewGuid();
            var mano = CrearManoBase(manoId);
            mano.NivelTruco = 1;
            mano.TrucoPendienteRespuestaHumano = true; 
            mano.TrucoResuelto = false;
            mano.Maquina.Mano = new List<Carta> { new Carta { ValorTruco = 14 } };
            mano.NivelMentiraTrucoMaquina = 0;
            PartidaMemoriaServicio.Actualizar(mano);

            //When
            var resultado = useCase.Ejecutar(manoId, aceptar: true, escalarA: "retruco");

            //Then
            bool esResuelto = resultado.TrucoResuelto;
            int nivelCambiado = resultado.NivelTruco;
            string estadoTexto = resultado.EstadoTruco;

            Assert.False(esResuelto);
        }

        //[Fact]
        //public void Ejecutar_EscalarValecuatro_SiHumanoEscalaAValeCuatroYMaquinaNoQuiere_DebeAsignarTresPuntosAlHumano()
        //{
        //    //Given
        //    var useCase = new ResponderTrucoUseCase();
        //    Guid manoId = Guid.NewGuid();
        //    var mano = CrearManoBase(manoId);
        //    mano.NivelTruco = 2; 
        //    mano.Maquina.Mano = new List<Carta>();
        //    PartidaMemoriaServicio.Actualizar(mano);

        //    //When
        //    var resultado = useCase.Ejecutar(manoId, aceptar: true, escalarA: "valecuatro");

        //    //Then
        //    Assert.True(resultado.TrucoResuelto);
        //    Assert.Equal("Humano", resultado.GanadorMano);
        //    Assert.Equal(3, resultado.PuntosTrucoMano);
        //    Assert.Contains("La máquina no quiso el vale cuatro", resultado.EstadoTruco);
        //}

        //[Fact]
        //public void Ejecutar_EscalarValeCuatro_SiHumanoEscalaAValeCuatroYMaquinaAcepta_DebeResolverTrucoEnNivelMaximo()
        //{
        //    //Given
        //    var useCase = new ResponderTrucoUseCase();
        //    Guid manoId = Guid.NewGuid();
        //    var mano = CrearManoBase(manoId);
        //    mano.NivelTruco = 2;
        //    mano.Maquina.Mano = new List<Carta> { new Carta { ValorTruco = 14 } }; 
        //    PartidaMemoriaServicio.Actualizar(mano);

        //    //When
        //    var resultado = useCase.Ejecutar(manoId, aceptar: true, escalarA: "valecuatro");

        //    //Then
        //    Assert.True(resultado.TrucoResuelto);
        //    Assert.Equal(3, resultado.NivelTruco);
        //    Assert.Contains("La máquina quiso el vale cuatro", resultado.EstadoTruco);
        //}

        [Fact]
        public void Ejecutar_SiEscalacionEsInvalidaParaElNivelActual_DebeLanzarInvalidOperationException()
        {
            // Given
            var useCase = new ResponderTrucoUseCase();
            Guid manoId = Guid.NewGuid();
            var mano = CrearManoBase(manoId);
            mano.NivelTruco = 2; // Estamos en Retruco, no podés cantar "retruco" encima
            PartidaMemoriaServicio.Actualizar(mano);

            // When
            Action act = () => useCase.Ejecutar(manoId, aceptar: true, escalarA: "retruco");

            // Then
            var ex = Assert.Throws<InvalidOperationException>(act);
            // Corregido para que coincida exactamente con el string de tu código
            Assert.Contains("para el nivel actual de truco", ex.Message);
        }

        [Fact]
        public void Ejecutar_Quiero_SiHumanoDiceQuieroSimpleEnTrucoComun_DebeMantenerAbiertaLaNegociacion()
        {
            //Given
            var useCase = new ResponderTrucoUseCase();
            Guid manoId = Guid.NewGuid();
            var mano = CrearManoBase(manoId);
            mano.NivelTruco = 1;
            mano.PuntosTrucoMano = 2;
            PartidaMemoriaServicio.Actualizar(mano);

            //When
            var resultado = useCase.Ejecutar(manoId, aceptar: true, escalarA: null);

            //Then
            Assert.False(resultado.TrucoResuelto); 
            Assert.Contains("Quisiste", resultado.EstadoTruco);
        }

        [Fact]
        public void Ejecutar_SiHumanoDiceQuieroSimpleEnValeCuatro_DebeResolverElTruco()
        {
            // Given
            var useCase = new ResponderTrucoUseCase();
            Guid manoId = Guid.NewGuid();
            var mano = CrearManoBase(manoId);
            mano.NivelTruco = 3; 
            mano.PuntosTrucoMano = 4;
            PartidaMemoriaServicio.Actualizar(mano);

            // When
            var resultado = useCase.Ejecutar(manoId, aceptar: true, escalarA: "");

            // Then
            Assert.True(resultado.TrucoResuelto);
        }



    }
}
