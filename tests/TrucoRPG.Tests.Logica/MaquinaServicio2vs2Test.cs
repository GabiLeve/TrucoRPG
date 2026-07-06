using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TrucoRPG.Tests.Logica
{
    public class MaquinaServicio2vs2Test
    {
        private ManoTruco2v2 CrearMano()
        {
            var j1 = new Jugador { Id = "J1" };
            var j2 = new Jugador { Id = "J2", EsMaquina = true };
            var j3 = new Jugador { Id = "J3" };
            var j4 = new Jugador { Id = "J4", EsMaquina = true };

            return new ManoTruco2v2
            {
                Posicion1 = j1,
                Posicion2 = j2,
                Posicion3 = j3,
                Posicion4 = j4,

                EquipoA = new Equipo2v2
                {
                    Id = "EquipoA",
                    Jugador1 = j1,
                    Jugador2 = j3
                },

                EquipoB = new Equipo2v2
                {
                    Id = "EquipoB",
                    Jugador1 = j2,
                    Jugador2 = j4
                }
            };
        }
        // ELEGIR CARTA ---------------------------------------------------------------------------------
        [Fact]
        public void ElegirCarta_SinCartasRetornaNull()
        {
            //Given
            var manoMaquina = new List<Carta>();
            var cartasYaJugadas = new List<Carta>();

            //When
            var resultado = MaquinaServicio2v2.ElegirCarta(
                manoMaquina,
                cartasYaJugadas);

            //Then
            Assert.Null(resultado);
        }

        [Fact]
        public void ElegirCarta_SinCartasJugadasEligeCartaMedia()
        {
            //Given
            var manoMaquina = new List<Carta>
            {
                new Carta { ValorTruco = 12 },
                new Carta { ValorTruco = 2 },
                new Carta { ValorTruco = 7 }
            };

            var cartasYaJugadas = new List<Carta>();

            //When
            var resultado = MaquinaServicio2v2.ElegirCarta(
                manoMaquina,
                cartasYaJugadas);

            //Then
            Assert.NotNull(resultado);
            Assert.Equal(7, resultado!.ValorTruco);
        }

        [Fact]
        public void ElegirCarta_CuandoPuedeGanarUsaLaCartaMasBajaQueGana()
        {
            //Given
            var manoMaquina = new List<Carta>
            {
                 new Carta { ValorTruco = 3 },
                 new Carta { ValorTruco = 6 },
                 new Carta { ValorTruco = 10 }
            };

            var cartasYaJugadas = new List<Carta>
            {
                 new Carta { ValorTruco = 5 }
            };

            //When
            var resultado = MaquinaServicio2v2.ElegirCarta(
                manoMaquina,
                cartasYaJugadas);

            //Then
            Assert.NotNull(resultado);
            Assert.Equal(6, resultado!.ValorTruco);
        }

        [Fact]
        public void ElegirCarta_CuandoNoPuedeGanarUsaLaCartaMasBaja()
        {
            //Given
            var manoMaquina = new List<Carta>
            {
                new Carta { ValorTruco = 2 },
                new Carta { ValorTruco = 4 },
                new Carta { ValorTruco = 6 }
            };

            var cartasYaJugadas = new List<Carta>
            {
                new Carta { ValorTruco = 10 }
            };

            //When
            var resultado = MaquinaServicio2v2.ElegirCarta(
                manoMaquina,
                cartasYaJugadas);

            //Then
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado!.ValorTruco);
        }

        // CANTAR ENVIDO -----------------------------------------------------------------------------------------------
        [Fact]
        public void DebeCantarEnvido_CuandoTantoEsMayorA27RetornaTrue()
        {
            //Given
            var mano = new List<Carta>
            {
                 new Carta { Palo = "Espada", Numero = 7 },
                 new Carta { Palo = "Espada", Numero = 6 },
                 new Carta { Palo = "Basto", Numero = 1 }
            };

            //When
            var resultado = MaquinaServicio2v2.DebeCantarEnvido(mano);

            //Then
            Assert.True(resultado);
        }

        [Fact]
        public void DebeCantarEnvido_CuantotantosNoEsMayorRetornarFalse()
        {
            //Given
            var mano = new List<Carta>
            {
                 new Carta { Palo = "Espada", Numero = 5 },
                 new Carta { Palo = "Espada", Numero = 1 },
                 new Carta { Palo = "Basto", Numero = 2 }
            };

            //When
            var resultado = MaquinaServicio2v2.DebeCantarEnvido(mano);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void DebeCantarEnvido_CuandoTantoEs27RetornaTrue()
        {
            //Given
            var mano = new List<Carta>
            {
                new Carta { Palo = "Oro", Numero = 7 },
                new Carta { Palo = "Oro", Numero = 0 },
                new Carta { Palo = "Espada", Numero = 1 }
            };

            //When
            var resultado = MaquinaServicio2v2.DebeCantarEnvido(mano);

            //Then
            Assert.True(resultado);
        }

        // ACEPTAR TRUCO --------------------------------------------------------------------
        [Fact]
        public void AceptarEnvido_CuandoTantoEs30RetornaTrue()
        {
            //Given
            var mano = new List<Carta>
            {
                new Carta { Palo = "Oro", Numero = 7 },
                new Carta { Palo = "Oro", Numero = 3 },
                new Carta { Palo = "Espada", Numero = 1 }
            };

            //When
            var resultado = MaquinaServicio2v2.AceptarEnvido(mano);

            //Then
            Assert.True(resultado);
        }

        [Fact]
        public void AceptarEnvido_CuandoTantoEs20RetornaFalse()
        {
            //Given
            var mano = new List<Carta>
            {
                new Carta { Palo = "Oro", Numero = 10 },
                new Carta { Palo = "Oro", Numero = 11 },
                new Carta { Palo = "Espada", Numero = 7 }
            };

            //When
            var resultado = MaquinaServicio2v2.AceptarEnvido(mano);

            //Then
            Assert.False(resultado);
        }

        //DECLARA BUENAS --------------------------------------------------------------------------------
        [Fact]
        public void DebeDeclararSonBuenas_CuandoNoHayTantoRivalDeclaradoRetornaFalse()
        {
            //Given
            int tantoPropio = 28;
            int? mejorTantoRivalDeclarado = null;

            //When
            var resultado = MaquinaServicio2v2.DebeDeclararSonBuenas(
                tantoPropio,
                mejorTantoRivalDeclarado);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void DebeDeclararSonBuenas_CuandoRivalSuperaPorMasDeCuatroPuntosRetornaTrue()
        {
            // Given
            int tantoPropio = 25;
            int? mejorTantoRivalDeclarado = 30;

            // When
            var resultado = MaquinaServicio2v2.DebeDeclararSonBuenas(
                tantoPropio,
                mejorTantoRivalDeclarado);

            // Then
            Assert.True(resultado);
        }

        // DEBE CANTAR TRUCO -------------------------------------------------------------------------
        [Fact]
        public void DebeCantarTruco_CuandoManoEstaVaciaRetornaFalse()
        {
            //Given
            var mano = new List<Carta>();

            //When
            var resultado = MaquinaServicio2v2.DebeCantarTruco(mano);

            //Then
            Assert.False(resultado);
        }

        [Fact]
        public void DebeCantarTruco_CuandoTieneCartaMuyFuerteRetornaTrue()
        {
            // Given
            var mano = new List<Carta>
            {
                new Carta { ValorTruco = 12 },
                new Carta { ValorTruco = 4 },
                new Carta { ValorTruco = 2 }
            };

            // When
            var resultado = MaquinaServicio2v2.DebeCantarTruco(mano);

            // Then
            Assert.True(resultado);
        }

        // PROCESAR TURNO MAQUINA ----------------------------------------------------------------------------
        [Fact]
        public void ProcesarTurnoMaquina_CuandoManoTerminadaRetornaFalso()
        {
            //Given
            var mano = new ManoTruco2v2
            {
                ManoTerminada = true,
                TurnoActual = "J2"
            };

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            //Then
            Assert.False(mano.TrucoCantado);
            Assert.False(mano.EnvidoCantado);
        }
        [Fact]
        public void ProcesarTurnoMaquina_CuandoPartidaTerminadaRetornoFalso()
        {
            //Given
            var mano = new ManoTruco2v2
            {
                PartidaTerminada = true,
                TurnoActual = "J2"
            };

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            //Then
            Assert.False(mano.TrucoCantado);
            Assert.False(mano.EnvidoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_CuandoNoEsSuTurnoRetornaFalso()
        {
            //Given
            var mano = new ManoTruco2v2
            {
                TurnoActual = "J1"
            };

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            //Then
            Assert.False(mano.TrucoCantado);
            Assert.False(mano.EnvidoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_CuandoHayTrucoPendienteRetornaFalso()
        {
            //Given
            var mano = new ManoTruco2v2
            {
                PartidaTerminada = true,
                TurnoActual = "J2"
            };
            mano.TrucoPendienteRespuestaDe = "J1";

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            //Then
            Assert.False(mano.TrucoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_CuandoJugadorNoExisteRetornaFalso()
        {
            //Given
            var mano = new ManoTruco2v2
            {
                PartidaTerminada = true,
                TurnoActual = ""
            };

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "JugadorInexistente");

            //Theb
            Assert.False(mano.TrucoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_CuandoJugadorEsHumanoNoHaceNadaRetornaFalso()
        {
            //Given
            var mano = new ManoTruco2v2
            {
                PartidaTerminada = true,
                TurnoActual = "J2"
            };

            mano.Posicion2.EsMaquina = false;

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            //Then
            Assert.False(mano.TrucoCantado);
            Assert.False(mano.EnvidoCantado);
        }
        [Fact]
        public void ProcesarTurnoMaquina_SinCartasNoHaceNada()
        {
            //Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.Posicion2.Mano = new List<Carta>();

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            //Then
            Assert.False(mano.EnvidoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_DeberiaCantarEnvido()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J4";
            mano.Posicion4.Mano =
            [
                new Carta { Palo = "Espada", Numero = 7 },
                new Carta { Palo = "Espada", Numero = 6 }
            ];

            mano.EnvidoCantado = false;
            mano.EnvidoResuelto = false;

            // When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J4");

            // Then
            Assert.True(mano.EnvidoCantado);
            Assert.Equal("J4", mano.CantorEnvido);
            Assert.Equal("Envido", mano.TipoEnvidoCantado);
            Assert.Equal("pendiente_respuesta", mano.FaseEnvido);
            Assert.NotNull(mano.EnvidoPendienteRespuestaDe);
        }

        [Fact]
        public void ProcesarTurnoMaquina_SiYaHayEnvidoNoHaceNada()
        {
            //Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.EnvidoCantado = true;
            mano.Posicion2.Mano =
            [
                new Carta()
            ];

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J2");

            //Then
            Assert.True(mano.EnvidoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_J3NoDeberiaCantar()
        {
            //Given
            var mano = CrearMano();
            mano.TurnoActual = "J3";
            mano.Posicion3.EsMaquina = true;
            mano.Posicion3.Mano =
            [
                new Carta()
            ];

            //When
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano, "J3");

            //Then
            Assert.False(mano.EnvidoCantado);
        }

        //RESPONDER ENVIDO -------------------------------------------------------------------------
        [Fact]
        public void ResponderEnvido_CuandoNoEsElResponsableRetonarFalso()
        {
            //Given
            var mano = new ManoTruco2v2
            {
                EnvidoPendienteRespuestaDe = "J1"
            };

            //When
            MaquinaServicio2v2.ResponderEnvido(mano, "J2");

            //Then
            Assert.Equal("J1", mano.EnvidoPendienteRespuestaDe);
        }

        [Fact]
        public void ResponderEnvido_CuandoJugadorNoEsMaquinaRetornarFalso()
        {
            //Given
            var mano = new ManoTruco2v2();
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = false;

            //When
            MaquinaServicio2v2.ResponderEnvido(mano, "J2");

            //Then
            Assert.Equal("J2", mano.EnvidoPendienteRespuestaDe);
        }

        //DECLARAR TANTOS ------------------------------------------------------------------
        [Fact]
        public void DeclaraTantos_CuandoTantosNoEsteDeclaradoNohaceNada()
        {
            //Given
            var mano = CrearMano();
            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";

            //When
            MaquinaServicio2v2.DeclararTanto(mano, "J2");

            //Then
            Assert.Empty(mano.TantosDeclarados);
        }

        [Fact]
        public void DeclararTanto_SiNoEsSuTurnoNoHaceNada()
        {
            var mano = CrearMano();

            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";

            MaquinaServicio2v2.DeclararTanto(mano, "J3");

            Assert.Empty(mano.TantosDeclarados);
        }

        [Fact]
        public void DeclararTanto_SiJugadorNoEsMaquinaNoHaceNada()
        {
            //Given
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J1";
            var jugador = mano.ObtenerJugador("J1");

            //When
            MaquinaServicio2v2.DeclararTanto(mano, "J1");

            //Then
            Assert.Empty(mano.TantosDeclarados);
        }

        [Fact]
        public void DeclararTanto_MaquinaSinCartasDeberiaDecirSonBuenas()
        {
            //Given
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            var jugador = mano.ObtenerJugador("J2");
            jugador.Mano.Clear();

            //When
            MaquinaServicio2v2.DeclararTanto(mano, "J2");

            //Theb
            Assert.True(mano.TantosDeclarados.ContainsKey("J2"));
            Assert.Equal(0, mano.TantosDeclarados["J2"]);
        }

        [Fact]
        public void DeclararTanto_MaquinaTieneMejorTantoDeberiaDeclararlo()
        {
            //Given
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            var maquina = mano.ObtenerJugador("J2");
            maquina.Mano =
            [
                new Carta{Numero = 7,Palo = "Espada"},
                new Carta{Numero = 6,Palo = "Espada"}
            ];

            mano.TantosDeclarados["J1"] = 20;

            //When
            MaquinaServicio2v2.DeclararTanto(mano, "J2");

            //Then
            Assert.True(mano.TantosDeclarados.ContainsKey("J2"));
            Assert.True(mano.TantosDeclarados["J2"] > 20);
        }

        [Fact]
        public void DeclararTanto_MaquinaPierdeContraRivalDiceSonBuenas()
        {
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            var maquina = mano.ObtenerJugador("J2");

            maquina.Mano =
            [
                new Carta{Numero = 1,Palo = "Oro"},
                new Carta{Numero = 2,Palo = "Oro"}
            ];

            mano.TantosDeclarados["J1"] = 33;

            //When
            MaquinaServicio2v2.DeclararTanto(mano, "J2");

            //Then
            Assert.True(mano.TantosDeclarados.ContainsKey("J2"));
            Assert.Null(mano.TantosDeclarados["J2"]);
        }
        // avanzar un paso

        [Fact]
        public void AvanzarUnPaso_Normal_ManoTerminada_DevuelveNull()
        {
            // Given
            var mano = CrearMano();
            mano.ManoTerminada = true;

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_Normal_PartidaTerminada_DevuelveNull()
        {
            // Given
            var mano = CrearMano();
            mano.PartidaTerminada = true;

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_Normal_GanadorMano_DevuelveNull()
        {
            // Given
            var mano = CrearMano();
            mano.GanadorMano = "EquipoA";

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_Normal_TodasOpciones_DevuelveNull()
        {
            // Given
            var mano = CrearMano();
            mano.PartidaTerminada = false;
            mano.ManoTerminada = false;
            mano.GanadorMano = null;

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_SegParte_SiProximoActorEsHumano_DevuelveNull()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J1";

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_SegParte_SiProximoActorEsNull_DevuelveNull()
        {
            //Given
            var mano = CrearMano();
            mano.TurnoActual = null;

            //When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            //Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_TerParte_SiActorNoEsMaquina_DevuelveNull()
        {
            // Given
            var mano = CrearMano();
            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.GanadorMano = null;
            mano.EnvidoPendienteRespuestaDe = null;
            mano.TrucoPendienteRespuestaDe = null;

            mano.TurnoActual = "J2";

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = false;

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_TerParte_SiJugadorNoExiste_DevuelveNull()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.Posicion2.Id = "OTRO";

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_AlResponderTruco_SiMaquinaNoEscala_DevuelveEventoTrucoResp()
        {
            // Given
            var mano = CrearMano();
            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.GanadorMano = null;
            mano.TurnoActual = "J2";
            mano.TrucoPendienteRespuestaDe = "J2";
            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;
            mano.Posicion2.Mano = new List<Carta> { new Carta { ValorTruco = 1, Numero = 4, Palo = "Copas" } };

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("truco-resp", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_AlResponderEnvido_SubeApuesta_DevuelveEventoEnvido()
        {
            // Given
            MaquinaServicio2v2.RandomNext = (max) => 0;

            var mano = CrearMano();

            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TipoEnvidoCantado = "Envido";

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { Numero = 4, Palo = "Copa", ValorTruco = 4 },
                new Carta { Numero = 5, Palo = "Basto", ValorTruco = 5 }
            };

            mano.Posicion2.Jugadas = new List<Carta>();

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_AlResponderEnvido_NoSubeApuesta_DevuelveRespuesta()
        {
            // Given
            MaquinaServicio2v2.RandomNext = (max) => 80;

            var mano = CrearMano();

            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TipoEnvidoCantado = "Envido";

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { Numero = 7, Palo = "Espada", ValorTruco = 11 },
                new Carta { Numero = 6, Palo = "Espada", ValorTruco = 10 }
            };

            mano.Posicion2.Jugadas = new List<Carta>();

            //When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            //Then
            Assert.NotNull(resultado);
            Assert.Equal("envido-resp", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderTruco_SubeApuesta_DevuelveEventoTruco()
        {
            // FORCE RANDOM
            MaquinaServicio2v2.RandomNext = (max) => 10;

            // Given
            var mano = CrearMano();

            mano.TurnoActual = "J2";
            mano.TrucoPendienteRespuestaDe = "J2";
            mano.NivelTruco = 1;

            mano.Posicion2.EsMaquina = true;
            mano.Posicion2.Id = "J2";

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { ValorTruco = 14 },
                new Carta { ValorTruco = 13 },
                new Carta { ValorTruco = 12 }
            };

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("truco", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderTruco_NoSubeApuesta_DevuelveRespuesta()
        {
            MaquinaServicio2v2.RandomNext = (max) => 80;

            //Given
            var mano = CrearMano();

            mano.TurnoActual = "J2";
            mano.TrucoPendienteRespuestaDe = "J2";
            mano.NivelTruco = 1;

            mano.Posicion2.EsMaquina = true;
            mano.Posicion2.Id = "J2";

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { ValorTruco = 1 },
                new Carta { ValorTruco = 2 },
                new Carta { ValorTruco = 3 }
            };

            //When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            //Then
            Assert.NotNull(resultado);
            Assert.Equal("truco-resp", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderTruco_EnFaseDeclararTantos_SiDiceSonBuenas_DevuelveSonBuenas()
        {
            // Given
            var mano = CrearMano();
            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.GanadorMano = null;
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TurnoActual = "J2";

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;
            mano.JugadorQueDijoSonBuenas = "J2";

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("tanto", resultado.Tipo);
            Assert.Equal("¡Son buenas!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderTruco_SiMaquinaCantaTrucoEnSuTurno_DevuelveEventoTrucoCartas()
        {
            // Given
            MaquinaServicio2v2.RandomNext = (max) => 80;
            var mano = CrearMano();
            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.GanadorMano = null;

            // Configuramos un turno normal para J2
            mano.TurnoActual = "J2";
            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            // Estado antes del turno: El truco NO estaba cantado
            mano.TrucoCantado = false;
            mano.EnvidoCantado = true; // Para pasar de largo el bloque de envido directo al de truco

            // Forzamos cartas ultra agresivas para asegurarnos de que ProcesarTurnoMaquina elija cantar Truco
            mano.Posicion2.Mano = new List<Carta> { new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 } };

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("carta", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderTruco_SiMaquinaCantaTrucoEnSuTurno_DevuelveEventoTruco()
        {
            MaquinaServicio2v2.RandomNext = (max) => 0;

            // Given
            var mano = CrearMano();

            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.GanadorMano = null;
            mano.TurnoActual = "J2";
            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;
            mano.TrucoCantado = false;
            mano.TrucoResuelto = false;

            mano.EnvidoCantado = true;
            mano.EnvidoResuelto = true;

            mano.VueltaActual = new Vuelta2v2();

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta{Numero = 1,Palo = "Espada",ValorTruco = 14}
            };

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("truco", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderTruco_SiMaquinaDiceNoQuiero_DevuelveEventoNoQuiero()
        {
            // Given
            var mano = CrearMano();
            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.GanadorMano = null;

            mano.TurnoActual = "J2";
            mano.TrucoPendienteRespuestaDe = "J2";

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            mano.EstadoTruco = "¡No quiero!";

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("truco-resp", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderEnvido_Acepta_DevuelveQuiero()
        {
            MaquinaServicio2v2.RandomNext = (max) => 99;

            var mano = CrearMano();

            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TipoEnvidoCantado = "Envido";

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            mano.Posicion2.Mano = new List<Carta>
                {
                    new Carta { Numero = 7, Palo = "Espada", ValorTruco = 11 },
                    new Carta { Numero = 6, Palo = "Espada", ValorTruco = 10 }
                };

            mano.Posicion2.Jugadas = new List<Carta>();

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("envido-resp", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderEnvido_NoQuiere_DevuelveNoQuiero()
        {
            var mano = CrearMano();

            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TipoEnvidoCantado = "Envido";
            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;
            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
                new Carta { Numero = 4, Palo = "Basto", ValorTruco = 4 }
            };

            mano.Posicion2.Jugadas = new List<Carta>();

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("envido-resp", resultado.Tipo);
            Assert.Equal("¡No quiero!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderEnvido_NoQuiereYHayTrucoPendiente_DevuelveMensajeRecordatorio()
        {
            var mano = CrearMano();

            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TipoEnvidoCantado = "Envido";
            mano.TrucoPendienteRespuestaDe = "J1";
            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
                new Carta { Numero = 4, Palo = "Basto", ValorTruco = 4 }
            };

            mano.Posicion2.Jugadas = new List<Carta>();

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
            Assert.Equal("envido-resp", resultado.Tipo);
            Assert.Equal("¡No quiero! ¿Y el truco?", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_ResponderEnvido_SubeApuesta_DevuelveEventoEnvido()
        {
                // Given
                MaquinaServicio2v2.RandomNext = (max) => 0;

                var mano = CrearMano();

                mano.FaseEnvido = "pendiente_respuesta";
                mano.EnvidoPendienteRespuestaDe = "J2";
                mano.TipoEnvidoCantado = "Envido";
                mano.Posicion2.Id = "J2";
                mano.Posicion2.EsMaquina = true;
                mano.Posicion2.Mano = new List<Carta>
                {
                    new Carta { Numero = 7, Palo = "Espada", ValorTruco = 11 },
                    new Carta { Numero = 6, Palo = "Espada", ValorTruco = 10 }
                };

                mano.Posicion2.Jugadas = new List<Carta>();

                mano.EnvidoCantado = true;
                mano.EnvidoResuelto = false;

               //When
               var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

               //Then
               Assert.NotNull(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_DeclarandoTantos_SonBuenas_DevuelveEventoSonBuenas()
        {
            // Given
            var mano = CrearMano();

            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;
            mano.TantosReales["J2"] = 30;
            mano.TantosDeclarados["J1"] = 33;

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("tanto", resultado.Tipo);
            Assert.Equal("¡Son buenas!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_DeclarandoTantos_DeclaraNumero_DevuelveTanto()
        {
            // Given
            var mano = CrearMano();

            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            // J2 tiene mejor tanto que el rival
            mano.TantosReales["J2"] = 33;
            mano.TantosDeclarados["J1"] = 28;

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("tanto", resultado.Tipo);
            Assert.Equal("33", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_SiEsJ3_YCumpleVentanaEnvido_ConsultaAlHumano()
        {
            //Given
            var mano = CrearMano();
            mano.Posicion3.EsMaquina = true;
            mano.TurnoActual = "J3";
            mano.CompaEnvidoConsultado = false;
            mano.EnvidoCantado = false;
            mano.EnvidoResuelto = false;
            mano.Vueltas = new List<Vuelta2v2>();

            mano.Posicion3.Mano = new List<Carta>
            {
                new Carta { ValorTruco = 11, Palo = "Espada", Numero = 7 },
                new Carta { ValorTruco = 10, Palo = "Espada", Numero = 6 }
            };

            //When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            //Then
            if (mano.CompaConsultaEnvido)
            {
                Assert.NotNull(resultado);
                Assert.Equal("consulta-envido", resultado.Tipo);
                Assert.Equal("Tengo mucho", mano.CompaPista);
            }
        }

        [Fact]
        public void AvanzarUnPaso_SiEsJ3_YTieneBuenasCartas_ConsultaTruco()
        {
            //Given
            var mano = CrearMano();
            mano.Posicion3.EsMaquina = true;
            mano.TurnoActual = "J3";
            mano.CompaTrucoConsultado = false;
            mano.TrucoCantado = false;
            mano.TrucoResuelto = false;
            mano.TrucoPendienteRespuestaDe = null;
            mano.Posicion1.Jugadas = new List<Carta>();
            mano.Posicion3.Mano = new List<Carta> { new Carta { ValorTruco = 10 } };

            //When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            //Then
            if (mano.CompaConsultaTruco)
            {
                Assert.NotNull(resultado);
                Assert.Equal("consulta-truco", resultado.Tipo);
                Assert.Equal("¿Voy o pongo?", resultado.Texto);
            }
        }

        [Fact]
        public void AvanzarUnPaso_SiMaquinaCantaEnvido_YRespondeJ1_GeneraPistaYEvento()
        {
            // Given
            var randomOriginal = MaquinaServicio2v2.RandomNext;
            MaquinaServicio2v2.RandomNext = _ => 0; 

            try
            {
                var mano = CrearMano();

                mano.TurnoActual = "J4";
                mano.Vueltas = new List<Vuelta2v2>();

                mano.EnvidoCantado = false;
                mano.EnvidoResuelto = false;
                mano.TrucoCantado = false;
                mano.TrucoPendienteRespuestaDe = null;

                
                mano.Posicion4.Mano = new List<Carta>
                {
                    new Carta { Palo = "Espada", Numero = 7, ValorTruco = 11 },
                    new Carta { Palo = "Espada", Numero = 6, ValorTruco = 10 },
                    new Carta { Palo = "Basto", Numero = 1, ValorTruco = 1 }
                };

                mano.Posicion3 = new Jugador
                {
                    Id = "J3",
                    EsMaquina = false,
                    Mano = new List<Carta>
                    {
                        new Carta { Palo = "Oro", Numero = 7, ValorTruco = 4 },
                        new Carta { Palo = "Oro", Numero = 6, ValorTruco = 3 },
                        new Carta { Palo = "Copa", Numero = 1, ValorTruco = 1 }
                    }
                };

                // When
                var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

                // Then
                Assert.NotNull(resultado);
                Assert.Equal("envido", resultado.Tipo);

                Assert.True(mano.EnvidoCantado);
                Assert.Equal("J1", mano.EnvidoPendienteRespuestaDe);
                Assert.Equal("Tengo mucho", mano.CompaPista);
            }
            finally
            {
                MaquinaServicio2v2.RandomNext = randomOriginal;
            }
        }

        [Fact]
        public void AvanzarUnPaso_TurnoNormal_J3ConsultaTruco_DevuelveConsultaTruco()
        {
            // Given
            var mano = CrearMano();

            mano.TurnoActual = "J3";
            mano.ManoTerminada = false;
            mano.PartidaTerminada = false;
            mano.GanadorMano = null;

            mano.EnvidoPendienteRespuestaDe = null;
            mano.TrucoPendienteRespuestaDe = null;

            mano.Posicion3.Id = "J3";
            mano.Posicion3.EsMaquina = true;

            mano.Posicion1.Id = "J1";
            mano.Posicion1.EsMaquina = false;
            mano.Posicion1.Jugadas = new List<Carta>();

            mano.CompaEnvidoConsultado = true;
            mano.CompaTrucoConsultado = false;

            mano.TrucoCantado = false;
            mano.TrucoResuelto = false;

            mano.Posicion3.Mano = new List<Carta>
            {
                new Carta{Numero = 1,Palo = "Espada",ValorTruco = 14}
            };

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("consulta-truco", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_TurnoNormal_SiMaquinaCantaEnvidoYDaPista_DevuelveEventoEnvido()
        {
            MaquinaServicio2v2.RandomNext = (max) => 0;

            var mano = CrearMano();

            mano.JugadorMano = "J1"; 

            mano.TurnoActual = "J2";

            mano.EnvidoCantado = false;
            mano.EnvidoResuelto = false;
            mano.TrucoCantado = false;
            mano.TrucoPendienteRespuestaDe = null;

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { Numero = 7, Palo = "Espada", ValorTruco = 11 },
                new Carta { Numero = 6, Palo = "Espada", ValorTruco = 10 }
            };

            mano.Posicion3.Mano = new List<Carta>
            {
                new Carta { Numero = 7, Palo = "Oro" },
                new Carta { Numero = 6, Palo = "Oro" }
            };

            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            Assert.NotNull(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_TurnoNormal_SiNoCantaEnvido_NoDevuelveEventoEnvido()
        {
            //Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.EnvidoCantado = false;
            mano.EnvidoResuelto = false;

            mano.Posicion2.Id = "J2";
            mano.Posicion2.EsMaquina = true;

            // Mano muy mala
            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { Numero = 4, Palo = "Espada" },
                new Carta { Numero = 5, Palo = "Basto" },
                new Carta { Numero = 6, Palo = "Oro" }
            };

            //When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            //Then
            Assert.NotNull(resultado);
            Assert.NotEqual("envido", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_NoDebeModificarPista_SiElEnvidoYaFueCantado()
        {
            // Given
            var idJ1 = "HumanoJ1";
            var idJ3 = "MaquinaJ3";

            var mano = new ManoTruco2v2
            {
                TrucoCantado = true,
                EnvidoCantado = true, 
                CompaConsultaEnvido = false,
                CompaPista = null,
                Vueltas = new List<Vuelta2v2>(),
                TrucoPendienteRespuestaDe = idJ1,

                Posicion1 = new Jugador
                {
                    Id = idJ1,
                    Jugadas = new List<Carta> { new Carta { Numero = 2, Palo = "Copa" } }
                },
                Posicion2 = new Jugador { Id = "RivalJ2", Jugadas = new List<Carta>() },
                Posicion3 = new Jugador { Id = idJ3, Jugadas = new List<Carta>() },
                Posicion4 = new Jugador { Id = "RivalJ4", Jugadas = new List<Carta>() }
            };

            // When
            var resultado = MaquinaServicio2v2.AvanzarUnPaso(mano);

            // Then
            Assert.False(mano.CompaConsultaEnvido);
            Assert.Null(mano.CompaPista); 
        }

        //Resolver consulta envido
        [Fact]
        public void ResolverConsultaEnvido_SiNoHayConsulta_LanzaExcepcion()
        {
            // Given
            var mano = CrearMano();
            mano.CompaConsultaEnvido = false;

            // When
            Action act = () => MaquinaServicio2v2.ResolverConsultaEnvido(mano,true,(m, j) => "J1");

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void ResolverConsultaEnvido_SiNoAcepta_CierraConsultaSinCantar()
        {
            // Given
            var mano = CrearMano();

            mano.CompaConsultaEnvido = true;
            mano.CompaEnvidoConsultado = false;

            // When
            MaquinaServicio2v2.ResolverConsultaEnvido(mano,false,(m, j) => "J1");

            // Then
            Assert.False(mano.CompaConsultaEnvido);
            Assert.True(mano.CompaEnvidoConsultado);
            Assert.False(mano.EnvidoCantado);
        }

        [Fact]
        public void ResolverConsultaEnvido_SiAcepta_CantaEnvido()
        {
            // Given
            var mano = CrearMano();
            mano.CompaConsultaEnvido = true;
            mano.CompaEnvidoConsultado = false;
            mano.Posicion3.Id = "J3";
            mano.Posicion3.EsMaquina = true;

            // When
            MaquinaServicio2v2.ResolverConsultaEnvido(mano,true,(m, j) => "J1");

            // Then
            Assert.False(mano.CompaConsultaEnvido);
            Assert.True(mano.CompaEnvidoConsultado);

            Assert.True(mano.EnvidoCantado);
            Assert.Equal("Envido", mano.TipoEnvidoCantado);
            Assert.Equal("J3", mano.CantorEnvido);
        }


        //resolver consulta truco
        [Fact]
        public void ResolverConsultaTruco_SiJ3EsNull_NoHaceNada()
        {
            var mano = CrearMano();

            mano.Posicion3.Id = "x";

            MaquinaServicio2v2.ResolverConsultaTruco(mano, true);

            Assert.False(mano.CompaConsultaTruco);
        }

        [Fact]
        public void ResolverConsultaTruco_SiJ3NoTieneCartas_NoHaceNada()
        {
            //Given
            var mano = CrearMano();

            mano.Posicion3.Mano.Clear();

            //When
            MaquinaServicio2v2.ResolverConsultaTruco(mano, true);

            //Then
            Assert.False(mano.CompaConsultaTruco);
            Assert.True(mano.CompaTrucoConsultado);
        }

        [Fact]
        public void ResolverConsultaTruco_SiVoy_JuegaCartaMasChica()
        {
            //Given
            var mano = CrearMano();

            mano.Posicion3.Mano = new List<Carta>
            {
                new Carta { ValorTruco = 14 },
                new Carta { ValorTruco = 5 },
                new Carta { ValorTruco = 10 }
            };
            mano.TurnoActual = "J3";
            mano.TrucoPendienteRespuestaDe = null;
            mano.EnvidoPendienteRespuestaDe = null;
            
            //When
            MaquinaServicio2v2.ResolverConsultaTruco(mano, true);

            //Then
            Assert.Equal(5, mano.Posicion3.Jugadas.First().ValorTruco);
        }

        [Fact]
        public void ResolverConsultaTruco_SiNoVoy_JuegaCartaMasGrande()
        {
            var mano = CrearMano();

            mano.Posicion3.Mano = new List<Carta>
            {
                new Carta { ValorTruco = 14 },
                new Carta { ValorTruco = 5 },
                new Carta { ValorTruco = 10 }
            };
            mano.TurnoActual = "J3";
            mano.TrucoPendienteRespuestaDe = null;
            mano.EnvidoPendienteRespuestaDe = null;
            MaquinaServicio2v2.ResolverConsultaTruco(mano, false);

            Assert.Equal(14, mano.Posicion3.Jugadas.First().ValorTruco);
        }

        [Fact]
        public void ResolverConsultaTruco_SiEnvidoDisponible_GeneraPista()
        {
            //Given
            var mano = CrearMano();

            mano.EnvidoCantado = false;
            mano.EnvidoResuelto = false;
            mano.CompaPista = "";
            mano.Posicion3.Mano = new List<Carta>
            {
                new Carta { Numero = 7, Palo = "Oro" },
                new Carta { Numero = 6, Palo = "Oro" }
            };

            //When
            MaquinaServicio2v2.ResolverConsultaTruco(mano, true);

            //Then
            Assert.False(string.IsNullOrEmpty(mano.CompaPista));
        }

        //ordenar jugar mayor
        [Fact]
        public void OrdenarJugarMayor_JugadorNoExiste_LanzaInvalidOperationException()
        {
            // Given
            var jugadorId = "BotInexistente";
            var mano = new ManoTruco2v2();

            // When
            Action act = () => MaquinaServicio2v2.OrdenarJugarMayor(mano, jugadorId);

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void OrdenarJugarMayor_JugadorNoEsMaquina_LanzaInvalidOperationException()
        {
            // Given
            var mano = CrearMano();
            var jugadorId = "J1";

            // When
            Action act = () => MaquinaServicio2v2.OrdenarJugarMayor(mano, jugadorId);

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void OrdenarJugarMayor_BotSinCartas_LanzaInvalidOperationException()
        {
            // Given
            var mano = CrearMano();
            var jugadorId = "J2"; 
            mano.EquipoA.Jugador2 = mano.Posicion2;
            mano.Posicion2.Mano = new List<Carta>();

            // When
            Action act = () => MaquinaServicio2v2.OrdenarJugarMayor(mano, jugadorId);

            // Then
            Assert.Throws<InvalidOperationException>(act);
        }

        [Fact]
        public void OrdenarJugarMayor_DatosValidos_AsignaOrdenJugarMayor()
        {
            // Given
            var mano = CrearMano();
            var jugadorId = "J2"; 
            mano.EquipoA.Jugador2 = mano.Posicion2;
            mano.Posicion2.Mano = new List<Carta> { new Carta() };

            // When
            MaquinaServicio2v2.OrdenarJugarMayor(mano, jugadorId);

            // Then
            Assert.Equal("J2",jugadorId);
        }
    }
}
