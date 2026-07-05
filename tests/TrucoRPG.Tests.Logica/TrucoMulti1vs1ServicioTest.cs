using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class TrucoMulti1vs1ServicioTest
    {
        //jugar carta
        [Fact]
        public void JugarCarta_CuandoHayEnvidoPendiente_DebeRetornarFalseYNoModificarNada()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Humano";
            estado.Mano.EnvidoPendienteRespuestaHumano = true; 

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 7, palo: "Espada");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoNoEsElTurnoDelJugador_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Maquina"; 

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 7, palo: "Espada"); // J1 intenta jugar

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoElJugadorNoTieneEsaCartaEnLaMano_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Humano";

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 1, palo: "Ancho Falso");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void JugarCarta_CuandoJ1AbreBaza_DeBeGuardarCartaPendienteYPasarTurnoAMaquina()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Humano";
            estado.Mano.CartaMaquinaEnMesa = null; 

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 7, palo: "Espada");

            // Then
            Assert.True(resultado);
            Assert.Empty(estado.Mano.Humano.Mano); 
            Assert.Equal("Maquina", estado.Mano.TurnoActual);
        }

        [Fact]
        public void JugarCarta_CuandoJ1CierraBaza_DebeLimpiarMesaYResolverBaza()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TurnoActual = "Humano";

            // J2 ya había jugado (Mesa con carta)
            var cartaJ2 = new Carta { Numero = 1, Palo = "Basto" };
            estado.Mano.CartaMaquinaEnMesa = cartaJ2;

            // When
            var resultado = TrucoMulti1v1Servicio.JugarCarta(estado, esJ1: true, numero: 7, palo: "Espada");

            // Then
            Assert.True(resultado);
            Assert.Null(estado.Mano.CartaMaquinaEnMesa); 
        }

        //cantar envido
        [Fact]
        public void CantarEnvido_DadoEnvidoYaCantado_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;

            // When
            var resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, true, "Envido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarEnvido_DadoEnvidoYaResuelto_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;
            estado.Mano.EnvidoResuelto = true;

            // When
            var resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, true, "Envido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarEnvido_DadoQueYaSeJugaronCartas_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.Bazas = new List<Baza> { new Baza() };

            // When
            var resultado = TrucoMulti1v1Servicio.CantarEnvido(estado,true, "Envido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarEnvido_DadoPartidaTerminada_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.Bazas = new List<Baza>();
            estado.Mano.PartidaTerminada = true;

            // When
            var resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, true, "Envido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarEnvido_DadoManoConGanadorYaDefinido_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.Bazas = new List<Baza>();
            estado.Mano.GanadorMano = "Humano";

            // When
            var resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, true, "Envido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarEnvido_DadoJ1CantaExitosamente_DebeInicializarApuestaYPasarTurnoAJ2()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.Bazas = new List<Baza>();
            estado.Mano.PartidaTerminada = false;
            estado.Mano.GanadorMano = null;

            // Asumiendo que EnvidoServicio.ObtenerPuntosSegunTipo("Envido") devuelve 2
            // When
            var resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, true, "Envido");

            // Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoCantado);
            Assert.Equal("Humano", estado.Mano.CantorEnvido);
            Assert.Equal(1, estado.PuntosEnvidoNoQuiero);
            Assert.Equal(2, estado.PuntosEnvidoEnJuego); 
        }

        [Fact]
        public void CantarEnvido_DadoJ2CantaExitosamente_DebeInicializarApuestaYPasarTurnoAJ1()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.Bazas = new List<Baza>();
            estado.Mano.PartidaTerminada = false;
            estado.Mano.GanadorMano = null;

            // When
            var resultado = TrucoMulti1v1Servicio.CantarEnvido(estado, false, "RealEnvido");

            // Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoCantado);
            Assert.Equal("Maquina", estado.Mano.CantorEnvido); 
            Assert.Equal(1, estado.PuntosEnvidoNoQuiero);
            Assert.Equal(3, estado.PuntosEnvidoEnJuego);
        }

        //responder envido
        [Fact]
        public void ResponderEnvido_SiEnvidoNoCantadoOResuelto_DevuelveFalse()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_SiEsJ1YNoTieneRespuestaPendiente_DevuelveFalse()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoPendienteRespuestaHumano = false; 

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_SiEsJ2YNoTieneRespuestaPendiente_DevuelveFalse()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.EnvidoPendienteRespuestaJ2 = false; 

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: false, aceptar: true);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderEnvido_SiNoSeAcepta_ResuelveEnvidoYAsignaPuntosAlCantor()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.EnvidoPendienteRespuestaHumano = true; 
            estado.Mano.CantorEnvido = "Maquina";
            estado.PuntosEnvidoNoQuiero = 2;

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: true, aceptar: false);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal(2, estado.Mano.PuntosEnvido);
            Assert.Contains("No quiso", estado.Mano.EstadoEnvido);
            Assert.False(estado.Mano.EnvidoPendienteRespuestaHumano);
            Assert.False(estado.EnvidoPendienteRespuestaJ2);
        }

        [Fact]
        public void ResponderEnvido_SiSeAceptaYHumanoTieneMayorTanto_GanaHumano()
        {
            //Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.EnvidoPendienteRespuestaJ2 = true; 
            estado.PuntosEnvidoEnJuego = 4;

            estado.Mano.Humano.Mano = new List<Carta> {
                new Carta { Numero = 7, Palo = "Espada" }, new Carta { Numero = 6, Palo = "Espada" }
            };
            estado.Mano.Maquina.Mano = new List<Carta> {
                new Carta { Numero = 4, Palo = "Copas" }
            }; 

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: false, aceptar: true);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal("Humano", estado.Mano.GanadorEnvido);
            Assert.Equal(4, estado.Mano.PuntosEnvido);
            Assert.Contains("Quiso", estado.Mano.EstadoEnvido);
        }

        [Fact]
        public void ResponderEnvido_SiEsFaltaEnvido_CalculaPuntosEspecialesDeFalta()
        {
            // Given
            var estado = CrearEstadoBase();

            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.EnvidoPendienteRespuestaJ2 = true;

            estado.Mano.TipoEnvidoCantado = "FaltaEnvido";
            estado.PuntosEnvidoEnJuego = 0;
            estado.Mano.PuntosHumano = 12;
            estado.Mano.PuntosMaquina = 10;
            estado.Mano.ManoIniciadaPor = "Maquina"; 
            estado.Mano.Humano.Mano = new List<Carta>();
            estado.Mano.Maquina.Mano = new List<Carta>();

            //When
            var resultado = TrucoMulti1v1Servicio.ResponderEnvido(estado, esJ1: false, aceptar: true);

            //Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.True(estado.Mano.PuntosEnvido > 0);
        }

        //son buenas 
        [Fact]
        public void SonBuenas_DadoEnvidoNoCantado_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;
            estado.Mano.EnvidoResuelto = false;

            // When
            var resultado = TrucoMulti1v1Servicio.SonBuenas(estado, true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void SonBuenas_DadoEnvidoYaResuelto_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = true;

            // When
            var resultado = TrucoMulti1v1Servicio.SonBuenas(estado,true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void SonBuenas_DadoJ1YNoTieneEnvidoPendiente_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.EnvidoPendienteRespuestaHumano = false;

            // When
            var resultado = TrucoMulti1v1Servicio.SonBuenas(estado, true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void SonBuenas_DadoJ2YNoTieneEnvidoPendiente_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.EnvidoPendienteRespuestaJ2 = false;

            // When
            var resultado = TrucoMulti1v1Servicio.SonBuenas(estado,false);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void SonBuenas_DadoJ1DeclaraSonBuenasConPuntosEnJuego_DebeAsignarPuntosYDarGanadorALaMaquina()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.EnvidoPendienteRespuestaHumano = true;
            estado.Mano.CantorEnvido = "Maquina"; 
            estado.Mano.TipoEnvidoCantado = "RealEnvido";
            estado.PuntosEnvidoEnJuego = 5; 

            // When
            var resultado = TrucoMulti1v1Servicio.SonBuenas(estado,true);

            // Then
            Assert.True(resultado);
            Assert.False(estado.Mano.EnvidoPendienteRespuestaHumano);
            Assert.True(estado.Mano.SonBuenasDeclarado);
            Assert.True(estado.Mano.EnvidoResuelto);
        }

        [Fact]
        public void SonBuenas_DadoJ2DeclaraSonBuenasSinPuntosEnJuego_DebeObtenerPuntosPorTipoYDarGanadorAlHumano()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.EnvidoPendienteRespuestaJ2 = true; 
            estado.Mano.CantorEnvido = "Humano";
            estado.Mano.TipoEnvidoCantado = "Envido";
            estado.PuntosEnvidoEnJuego = 0; 

            // When
            var resultado = TrucoMulti1v1Servicio.SonBuenas(estado,  false);

            // Then
            Assert.True(resultado);
            Assert.False(estado.EnvidoPendienteRespuestaJ2);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal("Humano", estado.Mano.GanadorEnvido);
        }

        [Fact]
        public void SonBuenas_DadoCantoFaltaEnvido_DebeCalcularPuntosFaltaUtilizandoElMaximoDePuntaje()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.EnvidoPendienteRespuestaHumano = true;
            estado.Mano.CantorEnvido = "Maquina";
            estado.Mano.TipoEnvidoCantado = "FaltaEnvido";
            estado.PuntosEnvidoEnJuego = 0;

            estado.Mano.PuntosHumano = 10;
            estado.Mano.PuntosMaquina = 14; 

            // When
            var resultado = TrucoMulti1v1Servicio.SonBuenas(estado,true);

            // Then
            Assert.True(resultado);
            Assert.True(estado.Mano.EnvidoResuelto);
            Assert.Equal("Maquina", estado.Mano.GanadorEnvido);
        }


        //escalar envido 
        [Fact]
        public void EscalarEnvido_DadoEnvidoNoCantado_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = false;
            estado.Mano.EnvidoResuelto = false;

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado,true,"RealEnvido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarEnvido_DadoEnvidoYaResuelto_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = true;

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado,true,"RealEnvido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarEnvido_DadoQueElMismoJugadorIntentaEscalar_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.CantorEnvido = "Humano";

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado, true, "RealEnvido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarEnvido_DadoCantoDeMenorOIgualJerarquia_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.CantorEnvido = "Maquina"; 
            estado.Mano.TipoEnvidoCantado = "RealEnvido";

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado,true,"Envido");

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarEnvido_DadoJ1EscalaExitosamenteCantoNormal_DebeActualizarPuntosYPasarTurnoAJ2()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.PuntosEnvidoEnJuego = 2; 
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.CantorEnvido = "Maquina";
            estado.Mano.TipoEnvidoCantado = "Envido";

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado,true,"RealEnvido");

            // Then
            Assert.True(resultado);
            Assert.Equal("RealEnvido", estado.Mano.TipoEnvidoCantado);
            Assert.Equal("Humano", estado.Mano.CantorEnvido);
            Assert.Equal(2, estado.PuntosEnvidoNoQuiero); 
        }

        [Fact]
        public void EscalarEnvido_DadoJ2EscalaAFaltaEnvido_DebeSetearPuntosEnJuegoEnCeroYPasarTurnoAJ1()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.PuntosEnvidoEnJuego = 4; 
            estado.Mano.EnvidoCantado = true;
            estado.Mano.EnvidoResuelto = false;
            estado.Mano.CantorEnvido = "Humano";
            estado.Mano.TipoEnvidoCantado = "EnvidoEnvido";

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarEnvido(estado,false, "FaltaEnvido");

            // Then
            Assert.True(resultado);
            Assert.Equal("FaltaEnvido", estado.Mano.TipoEnvidoCantado);
            Assert.Equal(4, estado.PuntosEnvidoNoQuiero); 
            Assert.Equal(0, estado.PuntosEnvidoEnJuego); 
        }

        //cantar truco
        [Fact]
        public void CantarTruco_DadoTrucoYaCantado_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;

            // When
            var resultado = TrucoMulti1v1Servicio.CantarTruco(estado, true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarTruco_DadoManoConGanadorYaDefinido_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.GanadorMano = "Maquina";

            // When
            var resultado = TrucoMulti1v1Servicio.CantarTruco(estado,true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarTruco_DadoPartidaTerminada_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.PartidaTerminada = true;

            // When
            var resultado = TrucoMulti1v1Servicio.CantarTruco(estado, true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void CantarTruco_DadoJ1CantaTrucoExitosamente_DebeInicializarTrucoYPasarTurnoAJ2()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = false;
            estado.Mano.GanadorMano = null;
            estado.Mano.PartidaTerminada = false;

            // When
            var resultado = TrucoMulti1v1Servicio.CantarTruco(estado, true);

            // Then
            Assert.True(resultado);
            Assert.True(estado.Mano.TrucoCantado);
            Assert.Equal(1, estado.Mano.NivelTruco);
            Assert.Equal(2, estado.Mano.PuntosTrucoMano);
            Assert.True(estado.TrucoPendienteRespuestaJ2); 
        }

        [Fact]
        public void CantarTruco_DadoJ2CantaTrucoExitosamente_DebeInicializarTrucoYPasarTurnoAJ1()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = false;
            estado.Mano.GanadorMano = null;
            estado.Mano.PartidaTerminada = false;

            // When
            var resultado = TrucoMulti1v1Servicio.CantarTruco(estado, false);

            // Then
            Assert.True(resultado);
            Assert.True(estado.Mano.TrucoCantado);
            Assert.Equal(1, estado.Mano.NivelTruco);
            Assert.Equal(2, estado.Mano.PuntosTrucoMano);
            Assert.True(estado.Mano.TrucoPendienteRespuestaHumano);
        }


        //responder truco
        [Fact]
        public void ResponderTruco_DadoJ1YNoHayTrucoPendiente_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoPendienteRespuestaHumano = false;

            // When
            var resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, esJ1: true, aceptar: true, escalarA: null);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderTruco_DadoJ2YNoHayTrucoPendiente_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.TrucoPendienteRespuestaJ2 = false;

            // When
            var resultado = TrucoMulti1v1Servicio.ResponderTruco(estado,false, true,null);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void ResponderTruco_DadoJ1RechazaTruco_DebeAsignarPuntosAlCantorYRetornarTrue()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoPendienteRespuestaHumano = true;
            estado.Mano.NivelTruco = 2; 
            estado.Mano.CantorTruco = "Maquina";

            // When
            var resultado = TrucoMulti1v1Servicio.ResponderTruco(estado,true, false, null);

            // Then
            Assert.True(resultado);
            Assert.False(estado.Mano.TrucoPendienteRespuestaHumano);
            Assert.True(estado.Mano.TrucoResuelto);
            Assert.Equal("Maquina", estado.Mano.GanadorMano);
            Assert.Equal(2, estado.Mano.PuntosTrucoMano);
            Assert.Contains("No quiso", estado.Mano.EstadoTruco);
        }

        [Fact]
        public void ResponderTruco_DadoJ1AceptaSinEscalar_DebeMarcarComoResueltoYRetornarTrue()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoPendienteRespuestaHumano = true;
            estado.Mano.PuntosTrucoMano = 2;

            // When
            var resultado = TrucoMulti1v1Servicio.ResponderTruco(estado,true,true, null);

            // Then
            Assert.True(resultado);
            Assert.False(estado.Mano.TrucoPendienteRespuestaHumano);
            Assert.True(estado.Mano.TrucoResuelto);
        }

        [Fact]
        public void ResponderTruco_DadoJ1AceptaYEscalaARetruco_DebeAumentarNivelYPasarElTurnoAJ2()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoPendienteRespuestaHumano = true;
            estado.Mano.NivelTruco = 1; // Truco normal

            // When
            var resultado = TrucoMulti1v1Servicio.ResponderTruco(estado,true, true,  "retruco");

            // Then
            Assert.True(resultado);
            Assert.False(estado.Mano.TrucoPendienteRespuestaHumano);
            Assert.Equal(2, estado.Mano.NivelTruco);
            Assert.Equal(3, estado.Mano.PuntosTrucoMano);
        }

        [Fact]
        public void ResponderTruco_DadoJ2AceptaYEscalaAValeCuatro_DebeAumentarNivelYPasarElTurnoAJ1()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.TrucoPendienteRespuestaJ2 = true;
            estado.Mano.NivelTruco = 2; // Ya estaban en Retruco

            // When
            var resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, false, true, "vale cuatro");

            // Then
            Assert.True(resultado);
            Assert.False(estado.TrucoPendienteRespuestaJ2);
            Assert.Equal(3, estado.Mano.NivelTruco);
            Assert.Equal(4, estado.Mano.PuntosTrucoMano);
        }

        [Fact]
        public void ResponderTruco_DadoNivelMaximoEIntentoDeEscalar_DebeIgnorarEscaladaYMarcarComoResuelto()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoPendienteRespuestaHumano = true;
            estado.Mano.NivelTruco = 3; // Ya es Vale Cuatro
            estado.Mano.PuntosTrucoMano = 4;

            // When
            var resultado = TrucoMulti1v1Servicio.ResponderTruco(estado, esJ1: true, aceptar: true, escalarA: "otra cosa");

            // Then
            Assert.True(resultado);
            Assert.True(estado.Mano.TrucoResuelto);
            Assert.Equal(3, estado.Mano.NivelTruco);
        }

        //escalar truco
        [Fact]
        public void EscalarTruco_DadoTrucoNoCantado_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = false;
            estado.Mano.NivelTruco = 1;

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_DadoNivelTrucoMaximoOResuelto_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.NivelTruco = 3; // Ya es Vale Cuatro

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_DadoTrucoPendienteHumano_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.NivelTruco = 1;
            estado.Mano.TrucoPendienteRespuestaHumano = true;

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_DadoTrucoPendienteJ2_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.NivelTruco = 1;
            estado.TrucoPendienteRespuestaJ2 = true;

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_DadoGanadorManoYaDefinido_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.NivelTruco = 1;
            estado.Mano.GanadorMano = "Humano";

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_DadoPartidaTerminada_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.NivelTruco = 1;
            estado.Mano.PartidaTerminada = true;

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_DadoQueElMismoJugadorCantoElUltimoNivel_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.NivelTruco = 1;
            estado.Mano.CantorTruco = "Humano";

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void EscalarTruco_DadoJ1EscalaARetrucoExitosamente_DebeActualizarEstadoYPasarTurnoAJ2()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.NivelTruco = 1; 
            estado.Mano.CantorTruco = "Maquina";

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: true);

            // Then
            Assert.True(resultado);
            Assert.Equal(2, estado.Mano.NivelTruco);
            Assert.False(estado.Mano.TrucoResuelto);
        }

        [Fact]
        public void EscalarTruco_DadoJ2EscalaAValeCuatroExitosamente_DebeActualizarEstadoYPasarTurnoAJ1()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.NivelTruco = 2; 
            estado.Mano.CantorTruco = "Humano";

            // When
            var resultado = TrucoMulti1v1Servicio.EscalarTruco(estado, esJ1: false);

            // Then
            Assert.True(resultado);
            Assert.Equal(3, estado.Mano.NivelTruco);
            Assert.False(estado.Mano.TrucoResuelto);
        }

        //irse a mazo
        [Fact]
        public void IrseAlMazo_DadoManoConGanadorYaDefinido_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.GanadorMano = "Humano";

            // When
            var resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IrseAlMazo_DadoPartidaYaTerminada_DebeRetornarFalse()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.PartidaTerminada = true;

            // When
            var resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.False(resultado);
        }

        [Fact]
        public void IrseAlMazo_DadoJ1SeVaSinTrucoCantado_DebeDar1PuntoAMaquinaYRetornarTrue()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = false;

            // Cuando J1 se va, gana J2 (Maquina)
            // When
            var resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.True(resultado);
            Assert.Equal("Maquina", estado.Mano.GanadorMano);
            Assert.True(estado.Mano.TrucoResuelto);
        }

        [Fact]
        public void IrseAlMazo_DadoJ2SeVaConTrucoPendienteHumano_DebeCalcularPuntosSegunNivelYRetornarTrue()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.TrucoPendienteRespuestaHumano = true; 
            estado.Mano.NivelTruco = 2;

            // When
            var resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: false);

            // Then
            Assert.True(resultado);
            Assert.Equal("Humano", estado.Mano.GanadorMano);
            Assert.False(estado.Mano.TrucoPendienteRespuestaHumano); 
        }

        [Fact]
        public void IrseAlMazo_DadoJ1SeVaConTrucoPendienteJ2_DebeCalcularPuntosSegunNivelYRetornarTrue()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.TrucoPendienteRespuestaJ2 = true; 
            estado.Mano.NivelTruco = 3; 

            // When
            var resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.True(resultado);
            Assert.Equal("Maquina", estado.Mano.GanadorMano);
            Assert.False(estado.TrucoPendienteRespuestaJ2); // Se resetea
            Assert.Contains("J1 se fue al mazo. J2 gana 3 pt.", estado.Mano.EstadoTruco);
        }

        [Fact]
        public void IrseAlMazo_DadoJ1SeVaConTrucoYaQuerido_DebeDarPuntosApostadosAMaquinaYRetornarTrue()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.TrucoCantado = true;
            estado.Mano.TrucoPendienteRespuestaHumano = false;
            estado.TrucoPendienteRespuestaJ2 = false;
            estado.Mano.PuntosTrucoMano = 3; // Ya habían querido el Retruco

            // When
            var resultado = TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.True(resultado);
            Assert.Equal("Maquina", estado.Mano.GanadorMano);
            Assert.True(estado.Mano.TrucoResuelto);
        }

        [Fact]
        public void IrseAlMazo_DebeLimpiarCualquierEnvidoPendiente()
        {
            // Given
            var estado = CrearEstadoBase();
            estado.Mano.EnvidoPendienteRespuestaHumano = true;
            estado.EnvidoPendienteRespuestaJ2 = true;

            // When
            TrucoMulti1v1Servicio.IrseAlMazo(estado, esJ1: true);

            // Then
            Assert.False(estado.Mano.EnvidoPendienteRespuestaHumano);
            Assert.False(estado.EnvidoPendienteRespuestaJ2);
        }


        private static EstadoTrucoMulti1v1 CrearEstadoBase()
        {
            return new EstadoTrucoMulti1v1
            {
                CartaPendienteJ1 = null,
                EnvidoPendienteRespuestaJ2 = false,
                TrucoPendienteRespuestaJ2 = false,
                Mano = new ManoTruco
                {
                    GanadorMano = null,
                    PartidaTerminada = false,
                    EnvidoPendienteRespuestaHumano = false,
                    TrucoPendienteRespuestaHumano = false,
                    CartaMaquinaEnMesa = null,
                    Humano = new()
                    {
                        Mano = new List<Carta> { new Carta { Numero = 7, Palo = "Espada" } },
                        Jugadas = new List<Carta>()
                    },
                    Maquina = new()
                    {
                        Mano = new List<Carta> { new Carta { Numero = 6, Palo = "Copas" } },
                        Jugadas = new List<Carta>()
                    }
                }
            };
        }

    }
}
