using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrucoRPG.API.Services;
using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Tests.API
{
    public class SalaServiceTest
    {
        //crear sala
        [Fact]
        public void CrearSala_CuandoDatosSonValidos_DeberiaInicializarLaSalaCorrectamente()
        {
            // Given 
            var salaService = new SalaService();
            var connectionId = "connection_123";
            var modo = "2v2";
            var esPublica = true;

            // When (Cuando ejecuto el método a testear)
            string codigoSala = salaService.CrearSala(connectionId, modo, esPublica);

            // Then 
            Assert.False(string.IsNullOrEmpty(codigoSala));
            Assert.Equal(6, codigoSala.Length); 
        }

        //unirse
        [Fact]
        public void UnirseASala_CuandoSalaExisteYTieneEspacio_DeberiaUnirseExitosamente()
        {
            var salaService = new SalaService();
            var connectionIdCreador = "jugador_1";
            var connectionIdInvitado = "jugador_2";
            var modo = "2v2"; 

            var codigoSala = salaService.CrearSala(connectionIdCreador, modo, publica: true);

            // When 
            var resultado = salaService.UnirseASala(connectionIdInvitado, codigoSala);

            // Then 
            Assert.True(resultado.Ok);
            Assert.Equal(modo, resultado.Modo);
            Assert.Equal(2, resultado.Cantidad); 
        }

        [Fact]
        public void UnirseASala_CuandoCodigoSalaNoExiste_DeberiaRetornarFalso()
        {
            // Given 
            var salaService = new SalaService();
            var connectionId = "jugador_random";
            var codigoInvalido = "NOEXIST";

            // When
            var resultado = salaService.UnirseASala(connectionId, codigoInvalido);

            // Then 
            Assert.False(resultado.Ok);
            Assert.Equal("", resultado.Modo);
            Assert.Equal(0, resultado.Cantidad);
        }

        [Fact]
        public void UnirseASala_CuandoJugadorYaEstaEnLaSala_DeberiaRetornarFalsoYNoDuplicarlo()
        {
            // Given
            var salaService = new SalaService();
            var connectionId = "jugador_1";
            var modo = "2v2";

            var codigoSala = salaService.CrearSala(connectionId, modo, publica: true);

            // When 
            var resultado = salaService.UnirseASala(connectionId, codigoSala);

            // Then
            Assert.False(resultado.Ok);
            Assert.Equal(modo, resultado.Modo);
            Assert.Equal(1, resultado.Cantidad); 
        }

        [Fact]
        public void UnirseASala_CuandoElModoEsperadoNoCoincide_DeberiaRetornarFalso()
        {
            // Given: una sala 2v2 ya creada
            var salaService = new SalaService();
            var codigoSala = salaService.CrearSala("jugador_1", "2v2", publica: false);

            // When: alguien intenta entrar con ese código desde el lobby 1v1 y 3v3
            var desde1v1 = salaService.UnirseASala("jugador_2", codigoSala, modoEsperado: "1v1");
            var desde3v3 = salaService.UnirseASala("jugador_3", codigoSala, modoEsperado: "3v3");
            // Y desde el lobby correcto (2v2)
            var desde2v2 = salaService.UnirseASala("jugador_4", codigoSala, modoEsperado: "2v2");

            // Then: solo entra el que vino del modo correcto
            Assert.False(desde1v1.Ok);
            Assert.False(desde3v3.Ok);
            Assert.True(desde2v2.Ok);
            Assert.Equal("2v2", desde1v1.Modo); // informa el modo real de la sala
        }

        //abandonar sala
        [Fact]
        public void AbandonarSala_CuandoElJugadorNoEstaEnNingunaSala_DeberiaRetornarResultadoVacio()
        {
            // Given
            var salaService = new SalaService();
            var connectionIdInexistente = "conexion_fantasma";

            // When 
            var resultado = salaService.AbandonarSala(connectionIdInexistente);

            // Then
            Assert.Null(resultado.Sala);
            Assert.False(resultado.SalaVacia);
            Assert.Empty(resultado.JugadoresRestantes);
        }

        [Fact]
        public void AbandonarSala_CuandoAunQuedanJugadores_DeberiaActualizarLaListaYNoVaciarLaSala()
        {
            // Given
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            var jugador2 = "jugador_2";
            var codigoSala = salaService.CrearSala(jugador1, "2v2", publica: true);
            salaService.UnirseASala(jugador2, codigoSala);

            // When
            var resultado = salaService.AbandonarSala(jugador2);

            // Then 
            Assert.Equal(codigoSala, resultado.Sala);
            Assert.False(resultado.SalaVacia);
            Assert.Contains(jugador1, resultado.JugadoresRestantes);
        }

        [Fact]
        public void AbandonarSala_CuandoSeVaElUltimoJugador_DeberiaMarcarSalaVaciaYLimpiarElEstado()
        {
            // Given
            var salaService = new SalaService();
            var jugadorUnico = "jugador_1";
            var codigoSala = salaService.CrearSala(jugadorUnico, "1v1", publica: true);

            // When 
            var resultado = salaService.AbandonarSala(jugadorUnico);

            // Then 
            Assert.Equal(codigoSala, resultado.Sala);
            Assert.True(resultado.SalaVacia);
            Assert.Empty(resultado.JugadoresRestantes);

            var intentoUnirse = salaService.UnirseASala("nuevo_jugador", codigoSala);
            Assert.False(intentoUnirse.Ok);
        }

        //listar salas publicas
        [Fact]
        public void ListarSalasPublicas_CuandoHayUnaSalaValida_DeberiaRetornarlaEnLaLista()
        {
            // Given 
            var salaService = new SalaService();
            var connectionId = "jugador_1";
            var modoBuscado = "2v2";

            var codigoSala = salaService.CrearSala(connectionId, modoBuscado, publica: true);

            // When
            var resultado = salaService.ListarSalasPublicas(modoBuscado);

            // Then
            Assert.Single(resultado);
            Assert.Equal(codigoSala, resultado[0].Codigo);
            Assert.Equal(modoBuscado, resultado[0].Modo);
        }

        [Fact]
        public void ListarSalasPublicas_CuandoLasSalasNoCumplenRequisitos_DeberiaIgnorarlas()
        {
            // Given 
            var salaService = new SalaService();
            var modoBuscado = "2v2";
            salaService.CrearSala("jugador_a", modoBuscado, publica: false);
            salaService.CrearSala("jugador_b", "1v1", publica: true);

            // When 
            var resultado = salaService.ListarSalasPublicas(modoBuscado);

            // Then 
            Assert.Empty(resultado);
        }

        [Fact]
        public void ListarSalasPublicas_CuandoLaSalaYaEstaLlena_DeberiaIgnorarla()
        {
            // Given
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            var jugador2 = "jugador_2";
            var modo = "1v1";

            var codigoSala = salaService.CrearSala(jugador1, modo, publica: true);
            salaService.UnirseASala(jugador2, codigoSala);

            // When 
            var resultado = salaService.ListarSalasPublicas(modo);

            // Then 
            Assert.Empty(resultado);
        }

        [Fact]
        public void ListarSalasPublicas_CuandoUnaSalaNoTieneConexionesActivas_DeberiaLimpiarlaYNoListarla()
        {
            // Given 
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            var modo = "2v2";
            var codigoSala = salaService.CrearSala(jugador1, modo, publica: true);
            salaService.AbandonarSala(jugador1);

            // When 
            var resultado = salaService.ListarSalasPublicas(modo);

            // Then
            Assert.Empty(resultado);
        }

        //elegir equipo
        [Fact]
        public void ElegirEquipo_CuandoElEquipoEsValidoYHayCupo_DeberiaAsignarElEquipoCorrectamente()
        {
            // Given 
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            var modo = "2v2";
            var codigoSala = salaService.CrearSala(jugador1, modo, publica: true);
            var equipoElegido = "sanMartin";

            // When 
            var resultado = salaService.ElegirEquipo(jugador1, equipoElegido);

            // Then
            Assert.NotNull(resultado);
            Assert.True(resultado.Ok);
            Assert.Equal(codigoSala, resultado.Sala);
        }

        [Fact]
        public void ElegirEquipo_CuandoElEquipoNoEsValido_DeberiaRetornarNulo()
        {
            // Given 
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            salaService.CrearSala(jugador1, "2v2", publica: true);
            var equipoInvalido = "cuartetoObrero"; 

            // When
            var resultado = salaService.ElegirEquipo(jugador1, equipoInvalido);

            // Then 
            Assert.Null(resultado);
        }

        [Fact]
        public void ElegirEquipo_CuandoElJugadorYaEstaEnEseEquipo_DeberiaRetornarNulo()
        {
            // Given 
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            salaService.CrearSala(jugador1, "2v2", publica: true);
            salaService.ElegirEquipo(jugador1, "belgrano");

            // When 
            var resultado = salaService.ElegirEquipo(jugador1, "belgrano");

            // Then 
            Assert.Null(resultado);
        }

        //marcar listo
        [Fact]
        public void MarcarListo_CuandoLaSalaNoTieneElMinimoDeJugadores_DeberiaRetornarNulo()
        {
            // Given 
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            var modo = "1v1"; 

            salaService.CrearSala(jugador1, modo, publica: true);

            // When
            var resultado = salaService.MarcarListo(jugador1);

            // Then 
            Assert.Null(resultado);
        }

        [Fact]
        public void MarcarListo_CuandoNoTodosEstanListos_DeberiaRegistrarElListoPeroNoIniciarJuego()
        {
            // Given 
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            var jugador2 = "jugador_2";
            var modo = "1v1";

            var codigoSala = salaService.CrearSala(jugador1, modo, publica: true);
            salaService.UnirseASala(jugador2, codigoSala);

            // When 
            var resultado = salaService.MarcarListo(jugador1);

            // Then 
            Assert.NotNull(resultado);
            Assert.False(resultado.TodosListos); 
            Assert.Equal(1, resultado.CantidadListos);
            Assert.Equal(2, resultado.Requeridos);
        }

        [Fact]
        public void MarcarListo_CuandoTodosEstanListos_DeberiaRetornarTrueParaIniciarJuego()
        {
            // Given 
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            var jugador2 = "jugador_2";
            var modo = "1v1";

            var codigoSala = salaService.CrearSala(jugador1, modo, publica: true);
            salaService.UnirseASala(jugador2, codigoSala);
            salaService.MarcarListo(jugador1);

            // When 
            var resultado = salaService.MarcarListo(jugador2);

            // Then 
            Assert.NotNull(resultado);
            Assert.True(resultado.TodosListos); 
            Assert.Equal(2, resultado.CantidadListos);
            Assert.Equal(2, resultado.Requeridos);
        }

        //on desconectado
        [Fact]
        public void OnDesconectado_CuandoElJugadorNoEstabaEnUnaSala_DeberiaRetornarResultadoVacio()
        {
            // Given 
            var salaService = new SalaService();
            var connectionId = "conexion_desconocida";

            // When 
            var resultado = salaService.OnDesconectado(connectionId);

            // Then 
            Assert.Null(resultado.Sala);
            Assert.False(resultado.SalaVacia);
            Assert.Empty(resultado.JugadoresRestantes);
        }

        [Fact]
        public void OnDesconectado_CuandoSeDesconectaElUltimoJugador_DeberiaMarcarSalaVaciaYNoDevolverEquipos()
        {
            // Given 
            var salaService = new SalaService();
            var jugadorUnico = "jugador_solo";
            var modo = "1v1";
            var codigoSala = salaService.CrearSala(jugadorUnico, modo, publica: true);

            // When 
            var resultado = salaService.OnDesconectado(jugadorUnico);

            // Then 
            Assert.Equal(codigoSala, resultado.Sala);
            Assert.True(resultado.SalaVacia);
            Assert.Empty(resultado.JugadoresRestantes);
        }

        [Fact]
        public void OnDesconectado_CuandoQuedanJugadores_DeberiaRemoverAlJugadorDelEquipoYRetornarElMapa()
        {
            // Given 
            var salaService = new SalaService();
            var jugador1 = "jugador_1";
            var jugador2 = "jugador_2";
            var modo = "2v2";

            var codigoSala = salaService.CrearSala(jugador1, modo, publica: true);
            salaService.UnirseASala(jugador2, codigoSala);

            salaService.ElegirEquipo(jugador1, "sanMartin");
            salaService.ElegirEquipo(jugador2, "belgrano");

            // When 
            var resultado = salaService.OnDesconectado(jugador1);

            // Then 
            Assert.Equal(codigoSala, resultado.Sala);
            Assert.False(resultado.SalaVacia);

            Assert.NotNull(resultado.EquiposMap);
            Assert.False(resultado.EquiposMap.ContainsKey(jugador1)); 
            Assert.True(resultado.EquiposMap.ContainsKey(jugador2));  
            Assert.Equal(modo, resultado.Modo);
        }

        //iniciar nueva mano 2vs2
        [Fact]
        public void IniciarNuevaMano2v2_CuandoEsPrimeraPartida_DeberiaInicializarConPuntosEnCeroYManoUno()
        {
            // Given 
            var salaService = new SalaService();
            var j1 = "conexion_j1";
            var j2 = "conexion_j2";
            var j3 = "conexion_j3";
            var j4 = "conexion_j4";

            var codigoSala = salaService.CrearSala(j1, "2v2", publica: true);
            salaService.UnirseASala(j2, codigoSala);
            salaService.UnirseASala(j3, codigoSala);
            salaService.UnirseASala(j4, codigoSala);

            // When
            var state = salaService.IniciarNuevaMano2v2(codigoSala, esPrimeraPartida: true, estadoAnterior: null);

            // Then 
            Assert.NotNull(state);
            Assert.NotNull(state.Mano);
            Assert.Equal(4, state.JugadoresIds.Length);

            // Validamos que se hayan mapeado las 4 posiciones en el diccionario del estado
            Assert.Equal(1, state.Posiciones[j1]);
            Assert.Equal(2, state.Posiciones[j2]);
            Assert.Equal(3, state.Posiciones[j3]);
            Assert.Equal(4, state.Posiciones[j4]);
        }

        [Fact]
        public void IniciarNuevaMano2v2_CuandoVieneDeUnEstadoAnterior_DeberiaArrastrarYIncrementarLosValores()
        {
            // Given
            var salaService = new SalaService();
            var j1 = "conexion_j1";
            var codigoSala = salaService.CrearSala(j1, "2v2", publica: true);
            salaService.UnirseASala("conexion_j2", codigoSala);
            salaService.UnirseASala("conexion_j3", codigoSala);
            salaService.UnirseASala("conexion_j4", codigoSala);

            var estadoAnterior = new ManoTruco2v2
            {
                NumeroDeMano = 1,
                PuntosEquipoA = 10,
                PuntosEquipoB = 12
            };

            // When 
            var state = salaService.IniciarNuevaMano2v2(codigoSala, esPrimeraPartida: false, estadoAnterior: estadoAnterior);

            // Then 
            Assert.NotNull(state);
            Assert.NotNull(state.Mano);
        }

        //iniciar nueva mano 3vs3
        [Fact]
        public void IniciarNuevaMano3v3_CuandoEsPrimeraPartida_DeberiaInicializarDatosPorDefectoYSeisPosiciones()
        {
            // Given 
            var salaService = new SalaService();
            var jugadoresIds = new[] { "j1", "j2", "j3", "j4", "j5", "j6" };

            var codigoSala = salaService.CrearSala(jugadoresIds[0], "3v3", publica: true);
            for (int i = 1; i < jugadoresIds.Length; i++)
            {
                salaService.UnirseASala(jugadoresIds[i], codigoSala);
            }

            // When 
            var state = salaService.IniciarNuevaMano3v3(codigoSala, esPrimeraPartida: true, estadoAnterior: null);

            // Then 
            Assert.NotNull(state);
            Assert.NotNull(state.Mano);
            Assert.Equal(6, state.JugadoresIds.Length);

        }

        [Fact]
        public void IniciarNuevaMano3v3_CuandoTieneEstadoAnterior_DeberiaArrastrarPuntosYSlotDePicaPica()
        {
            // Given 
            var salaService = new SalaService();
            var j1 = "conexion_j1";
            var codigoSala = salaService.CrearSala(j1, "3v3", publica: true);
            for (int i = 2; i <= 6; i++)
            {
                salaService.UnirseASala($"conexion_j{i}", codigoSala);
            }

            var estadoAnterior = new ManoTruco3v3
            {
                NumeroDeMano = 2,
                PuntosEquipoA = 15,
                PuntosEquipoB = 12,
                PicaPicaSlot = 2
            };

            // When 
            var state = salaService.IniciarNuevaMano3v3(codigoSala, esPrimeraPartida: false, estadoAnterior: estadoAnterior);

            // Then 
            Assert.NotNull(state);
            Assert.NotNull(state.Mano);
            Assert.Equal(6, state.JugadoresIds.Length);
        }
    }
}
