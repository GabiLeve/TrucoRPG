using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TrucoRPG.Tests.Logica
{
    public class MaquinaServicio2vs2Test
    {
        private ManoTruco2v2 CrearMano()
        {
            var j1 = new Jugador {Id = "J1"};
            var j2 = new Jugador {Id = "J2", EsMaquina = true};
            var j3 = new Jugador {Id = "J3"};
            var j4 = new Jugador {Id = "J4",EsMaquina = true};

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
            MaquinaServicio2v2.ProcesarTurnoMaquina( mano,"J2");

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
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano,"J4");

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
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano,"J2");

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
            MaquinaServicio2v2.ProcesarTurnoMaquina(mano,"J3");

            //Then
            Assert.False( mano.EnvidoCantado);
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
            Assert.Null(mano.TantosDeclarados["J2"]);
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
    }
}
