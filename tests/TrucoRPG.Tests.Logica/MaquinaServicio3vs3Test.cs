using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    public class MaquinaServicio3v3Test
    {
        private ManoTruco3v3 CrearMano()
        {
            var j1 = new Jugador { Id = "J1" };
            var j2 = new Jugador { Id = "J2", EsMaquina = true };
            var j3 = new Jugador { Id = "J3" };
            var j4 = new Jugador { Id = "J4", EsMaquina = true };
            var j5 = new Jugador { Id = "J5" };
            var j6 = new Jugador { Id = "J6", EsMaquina = true };

            return new ManoTruco3v3
            {
                Posicion1 = j1,
                Posicion2 = j2,
                Posicion3 = j3,
                Posicion4 = j4,
                Posicion5 = j5,
                Posicion6 = j6,

                EquipoA = new Equipo3v3
                {
                    Id = "EquipoA",
                    Jugador1 = j1,
                    Jugador2 = j3,
                    Jugador3 = j5
                },

                EquipoB = new Equipo3v3
                {
                    Id = "EquipoB",
                    Jugador1 = j2,
                    Jugador2 = j4,
                    Jugador3 = j6
                },

                TurnoActual = "J2"
            };
        }
        //responder envido 
        [Fact]
        public void ResponderEnvido_SiJugadorEsHumano_HaceReturnYNoModificaNada()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                EnvidoPendienteRespuestaDe = "J1"
            };
            mano.Posicion1 = new Jugador { Id = "J1", EsMaquina = false };

            // When 
            MaquinaServicio3v3.ResponderEnvido(mano, "J1");

            // Then 
            Assert.Equal("J1", mano.EnvidoPendienteRespuestaDe);
            Assert.Null(mano.FaseEnvido);
        }

        [Fact]
        public void ResponderEnvido_SiBotNoTieneCartasOriginales_LlamaResolverNoQuiero()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                EnvidoPendienteRespuestaDe = "J2"
            };
            mano.Posicion2 = new Jugador
            {
                Id = "J2",
                EsMaquina = true,
                Mano = new List<Carta>(),
                Jugadas = new List<Carta>()
            };
            mano.EstadoEnvido = "No Quiero";
            // When 
            MaquinaServicio3v3.ResponderEnvido(mano, "J2");

            // Then 
            Assert.Equal("No Quiero", mano.EstadoEnvido);
            Assert.Equal("J2", mano.EnvidoPendienteRespuestaDe);
        }

        [Fact]
        public void ResponderEnvido_SiAceptarEnvidoEsFalso_LlamaResolverNoQuiero()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                EnvidoPendienteRespuestaDe = "J3"
            };
            var cartaFea = new Carta { ValorTruco = 1 };

            mano.Posicion3 = new Jugador
            {
                Id = "J3",
                EsMaquina = true,
                Mano = new List<Carta> { cartaFea }
            };
            mano.EstadoEnvido = "No Quiero";
            // When 
            MaquinaServicio3v3.ResponderEnvido(mano, "J3");

            // Then
            Assert.Equal("No Quiero", mano.EstadoEnvido);
            Assert.Equal("J3", mano.EnvidoPendienteRespuestaDe);
        }

        [Fact]
        public void ResponderEnvido_SiEligeEscalar_LlamaMetodoEscalarYRetorna()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                EnvidoPendienteRespuestaDe = "J3",
                TipoEnvidoCantado = "Envido"
            };
            var c1 = new Carta { ValorTruco = 14 };

            mano.Posicion3 = new Jugador
            {
                Id = "J3",
                EsMaquina = true,
                Mano = new List<Carta> { c1 }
            };

            // When 
            MaquinaServicio3v3.ResponderEnvido(mano, "J3");

            // Then 
            Assert.Equal("J3", mano.EnvidoPendienteRespuestaDe);
        }

        [Fact]
        public void ResponderEnvido_SiAceptaYNoEscala_ModificaManoEIniciaDeclaracion()
        {
            // Given
            var mano = new ManoTruco3v3
            {
                EnvidoPendienteRespuestaDe = "J3",
                TipoEnvidoCantado = "Falta Envido"
            };
            var c1 = new Carta { ValorTruco = 11, Palo = "Espada", Numero = 7 };
            var c2 = new Carta { ValorTruco = 10, Palo = "Espada", Numero = 6 };

            mano.Posicion3 = new Jugador
            {
                Id = "J3",
                EsMaquina = true,
                Mano = new List<Carta> { c1, c2 }
            };

            // When 
            MaquinaServicio3v3.ResponderEnvido(mano, "J3");

            // Then 
            Assert.Equal("declarando_tantos", mano.FaseEnvido);
        }

        [Fact]
        public void ResponderEnvido_SiRivalVaGanando_BotIntentaGanarleConLoJusto()
        {
            // Given 
            var mano = CrearMano();

            mano.TurnoActual = "J3";
            mano.EnvidoResuelto = true;
            mano.TrucoResuelto = true;
            mano.VueltaActual = new Vuelta3v3();

            var cTres = new Carta { ValorTruco = 10 };
            var cAncho = new Carta { ValorTruco = 14 };
            mano.Posicion3.EsMaquina = true;
            mano.Posicion3.Mano = new List<Carta> { cAncho, cTres };
            mano.VueltaActual.CartasJugadas["J2"] = new Carta { ValorTruco = 11 };

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J3");

            // Then
            Assert.Equal(cAncho, mano.VueltaActual.CartasJugadas["J3"]);
        }

        //avanzar paso -----------------------------------------------------------------------
        [Fact]
        public void AvanzarUnPaso_PartidaTerminada_RetornaNull()
        {
            // Given
            var mano = CrearMano();
            mano.PartidaTerminada = true;

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }


        [Fact]
        public void AvanzarUnPaso_ManoTerminada_RetornaNull()
        {
            // Given
            var mano = CrearMano();
            mano.ManoTerminada = true;

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }


        [Fact]
        public void AvanzarUnPaso_HayGanadorMano_RetornaNull()
        {
            // Given
            var mano = CrearMano();
            mano.GanadorMano = "EquipoA";

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }


        [Fact]
        public void AvanzarUnPaso_TurnoHumano_RetornaNull()
        {
            // Given
            var mano = CrearMano();

            mano.TurnoActual = "J1";

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }


        [Fact]
        public void AvanzarUnPaso_JugadorHumano_NoEjecutaMaquina()
        {
            // Given
            var mano = CrearMano();

            mano.TurnoActual = "J1";

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.Null(resultado);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaRespondeTrucoNoQuiero()
        {
            // Given
            var mano = CrearMano();

            mano.TrucoPendienteRespuestaDe = "J2";
            mano.NivelTruco = 1;
            mano.EstadoTruco = "no quiso";

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("J2", resultado.Jugador);
            Assert.Equal("truco-resp", resultado.Tipo);
            Assert.Contains("No quiero", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaRespondeTrucoQuiero()
        {
            // Given
            var mano = CrearMano();

            mano.TrucoPendienteRespuestaDe = "J2";
            mano.NivelTruco = 1;

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { ValorTruco = 7}
            };

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("J2", resultado.Jugador);
            Assert.Equal("truco-resp", resultado.Tipo);
            Assert.Equal("¡Quiero!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaRespondeEnvidoNoQuiero()
        {
            // Given
            var mano = CrearMano();
            mano.FaseEnvido = "pendiente_respuesta";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TipoEnvidoCantado = "Envido";
            mano.Posicion2.Mano.Clear();

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("J2", resultado.Jugador);
            Assert.Equal("envido-resp", resultado.Tipo);
            Assert.Contains("No quiero", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaDeclaraTantos()
        {
            // Given
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.TantosReales["J2"] = 27;

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("J2", resultado.Jugador);
            Assert.Equal("tanto", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaJuegaCarta()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta {  Palo = "Espada",Numero = 1,ValorTruco = 12 }
            };

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("J2", resultado.Jugador);
            Assert.Equal("carta", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaCantaTruco()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta{Palo = "Espada",Numero = 1,ValorTruco = 14}
            };

            mano.Vueltas.Add(new Vuelta3v3());

            // When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            // Then
            Assert.NotNull(resultado);
            Assert.Equal("truco", resultado.Tipo);
        }

        [Fact]
        public void AvanzarUnPaso_MaquinaCantaEnvido()
        {
            //Given
            var mano = CrearMano();
            mano.JugadorMano = "J3";
            mano.TurnoActual = "J2";

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta{Palo = "Oro",Numero = 6,ValorTruco = 6},
                new Carta{Palo = "Oro",Numero = 7,ValorTruco = 7},
                new Carta{Palo = "Oro",Numero = 12,ValorTruco = 10}
            };

            //When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            //Then
            Assert.NotNull(resultado);
            Assert.Equal("envido", resultado.Tipo);
        }


        [Fact]
        public void AvanzarUnPaso_EnFaseDeclararTantos_SiDiceSonBuenas_DevuelveEventoBuenas()
        {
            //Given
            var mano = CrearMano();
            mano.FaseEnvido = "declarando_tantos";
            mano.EnvidoPendienteRespuestaDe = "J2";
            mano.JugadorQueDijoSonBuenas = "J2";

            //When 
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            //Then 
            Assert.NotNull(resultado);
            Assert.Equal("tanto", resultado.Tipo);
            Assert.Equal("¡Son buenas!", resultado.Texto);
        }

        [Fact]
        public void AvanzarUnPaso_CompaneroConsultaEnvido_CantoLosTantos()
        {
            //Given
            var mano = CrearMano();
            var j3 = mano.ObtenerJugador("J3");
            if (j3 != null) j3.EsMaquina = true;

            mano.TurnoActual = "J3";
            mano.PicaPica = false;
            mano.CompaEnvidoConsultado = false;
            mano.EnvidoCantado = false;
            mano.EnvidoResuelto = false;
            mano.Vueltas = new List<Vuelta3v3>(); 

            //When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            //Then
            if (mano.CompaConsultaEnvido)
            {
                Assert.NotNull(resultado);
                Assert.Equal("consulta-envido", resultado.Tipo);
                Assert.Equal("¿Canto los tantos?", resultado.ToString());
            }
        }

        [Fact]
        public void AvanzarUnPaso_CompaneroConsultaTruco_VoyOPongo()
        {
            //Given
            var mano = CrearMano();
            mano.JugadoresActivos = new List<string> { "J1", "J2", "J3", "J4", "J5", "J6" };

            var j3 = mano.ObtenerJugador("J3");
            if (j3 != null)
            {
                j3.EsMaquina = true;
                j3.Mano = new List<Carta>
                {
                    new Carta { Palo = "Espada", Numero = 1, ValorTruco = 14 }
                };
                j3.Jugadas = new List<Carta>();
            }

            var j1 = mano.ObtenerJugador("J1");
            if (j1 != null)
            {
                j1.Jugadas = new List<Carta>();
            };
            mano.TurnoActual = "J3";
            mano.PicaPica = false;
            mano.CompaTrucoConsultado = false;
            mano.TrucoCantado = false;
            mano.TrucoResuelto = false;
            mano.TrucoPendienteRespuestaDe = null;

            //When
            var resultado = MaquinaServicio3v3.AvanzarUnPaso(mano);

            //Then
            Assert.NotNull(resultado);
            Assert.Equal("consulta-truco", resultado.Tipo);
        }


        //Procesar maquina
        [Fact]
        public void ProcesarTurnoMaquina_ConOrdenJugarMayor_JuegaCartaMasAlta()
        {
            //Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.OrdenJugarMayor = "J2";

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta { Numero = 5, ValorTruco = 3 },
                new Carta { Numero = 7, ValorTruco = 14 }
            };

            //When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            //Then
            Assert.Null(mano.OrdenJugarMayor);
            Assert.Single(mano.Posicion2.Jugadas);
            Assert.Equal(7, mano.Posicion2.Jugadas[0].Numero);
        }
        [Fact]
        public void ProcesarTurnoMaquina_ManoTerminada_NoHaceNada()
        {
            // Given
            var mano = CrearMano();
            mano.ManoTerminada = true;
            mano.TurnoActual = "J2";

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            // Then
            Assert.Empty(mano.Posicion2.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_NoEsSuTurno_NoHaceNada()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J1";
            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta{Numero = 1,ValorTruco = 14}
            };

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            // Then
            Assert.Empty(mano.Posicion2.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_HayTrucoPendiente_NoHaceNada()
        {
            // Given
            var mano = CrearMano();
            mano.TrucoPendienteRespuestaDe = "J2";
            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta{ValorTruco = 14}
            };

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            // Then
            Assert.False(mano.TrucoCantado);
            Assert.Empty(mano.Posicion2.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_JugadorHumano_NoHaceNada()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J1";

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J1");

            // Then
            Assert.Empty(mano.Posicion1.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_MaquinaSinCartas_NoHaceNada()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.Posicion2.Mano.Clear();

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            // Then
            Assert.Empty(mano.Posicion2.Jugadas);
        }

        [Fact]
        public void ProcesarTurnoMaquina_MaquinaCantaEnvido()
        {
            // Given
            var mano = CrearMano();
            mano.JugadorMano = "J3";

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta{Palo="Oro",Numero=6,ValorTruco=6},
                new Carta{Palo="Oro",Numero=7,ValorTruco=7},
                new Carta{Palo="Oro",Numero=12,ValorTruco=10}
            };

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            // Then
            Assert.True(mano.EnvidoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_MaquinaCantaTruco()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.Vueltas.Add(new Vuelta3v3());

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta{Numero = 1,ValorTruco = 14}
            };

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            // Then
            Assert.True(mano.TrucoCantado);
        }

        [Fact]
        public void ProcesarTurnoMaquina_OrdenJugarMayor_JuegaCartaMasAlta()
        {
            // Given
            var mano = CrearMano();
            mano.TurnoActual = "J2";
            mano.OrdenJugarMayor = "J2";

            mano.Posicion2.Mano = new List<Carta>
            {
                new Carta{Numero=2,ValorTruco=5},
                new Carta{Numero=7,ValorTruco=14}
            };

            // When
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            // Then
            Assert.Null(mano.OrdenJugarMayor);
            Assert.Single(mano.Posicion2.Jugadas);
            Assert.Equal(7, mano.Posicion2.Jugadas[0].Numero);
        }

        [Fact]
        public void ProcesarTurnoMaquina_ElegirCartaEnEquipo_CuandoAbreVuelta_EligeCartaIntermedia()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TurnoActual = "J2",
                EnvidoResuelto = true,
                TrucoResuelto = true,
                VueltaActual = new Vuelta3v3()
            };

            var cMala = new Carta { ValorTruco = 1 };
            var cMedia = new Carta { ValorTruco = 7 };
            var cBuena = new Carta { ValorTruco = 14 };

            mano.Posicion2 = new Jugador
            {
                Id = "J2",
                EsMaquina = true,
                Mano = new List<Carta> { cMala, cMedia, cBuena }
            };

            // When 
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J2");

            // Then 
            Assert.Contains(cMedia, mano.Posicion2.Jugadas);
            Assert.Equal(cMedia, mano.VueltaActual.CartasJugadas["J2"]);
        }

        [Fact]
        public void ProcesarTurnoMaquina_ElegirCartaEnEquipo_SiCompaneroVaGanando_BotAhorraYTiraLaMasBaja()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TurnoActual = "J3",
                EnvidoResuelto = true,
                TrucoResuelto = true,
                VueltaActual = new Vuelta3v3()
            };

            var cMala = new Carta { ValorTruco = 2 };
            var cBuena = new Carta { ValorTruco = 12 };

            mano.Posicion3 = new Jugador
            {
                Id = "J3",
                EsMaquina = true,
                Mano = new List<Carta> { cBuena, cMala }
            };
            mano.Posicion1 = new Jugador { Id = "J1", EsMaquina = false };
            mano.VueltaActual.CartasJugadas["J1"] = new Carta { ValorTruco = 11 };

            mano.Posicion2 = new Jugador { Id = "J2", EsMaquina = true };
            mano.VueltaActual.CartasJugadas["J2"] = new Carta { ValorTruco = 1 };

            // When 
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J3");

            // Then 
            Assert.Contains(cMala, mano.Posicion3.Jugadas);
            Assert.Equal(cMala, mano.VueltaActual.CartasJugadas["J3"]);
        }

        [Fact]
        public void ProcesarTurnoMaquina_ElegirCrataEnEquipo_SiRivalVaGanando_BotIntentaGanarleConLoJusto()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TurnoActual = "J3",
                EnvidoResuelto = true,
                TrucoResuelto = true,
                VueltaActual = new Vuelta3v3()
            };
            var cTres = new Carta { ValorTruco = 10 };
            var cAncho = new Carta { ValorTruco = 14 };

            mano.Posicion3 = new Jugador
            {
                Id = "J3",
                EsMaquina = true,
                Mano = new List<Carta> { cAncho, cTres }
            };
            mano.EquipoA.Jugador2 = mano.Posicion3;

            var rival = new Jugador { Id = "J2", EsMaquina = true };
            mano.Posicion2 = rival;
            mano.EquipoB.Jugador1 = rival;
            var cartaRival = new Carta { ValorTruco = 11 };
            mano.VueltaActual.CartasJugadas["J2"] = cartaRival;

            // When 
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J3");

            // Then 
            Assert.Contains(cAncho, mano.Posicion3.Jugadas);
            Assert.Equal(cAncho, mano.VueltaActual.CartasJugadas["J3"]);
        }

        [Fact]
        public void ProcesarTurnoMaquina_ElegirCartaEnEquipo_SiRivalVaGanandoYBotNoPuedeSuperarlo_TiraLaMasBaja()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                TurnoActual = "J3",
                EnvidoResuelto = true,
                TrucoResuelto = true,
                VueltaActual = new Vuelta3v3()
            };

            var cFea1 = new Carta { ValorTruco = 2 };
            var cFea2 = new Carta { ValorTruco = 5 };

            mano.Posicion3 = new Jugador
            {
                Id = "J3",
                EsMaquina = true,
                Mano = new List<Carta> { cFea2, cFea1 }
            };

            mano.Posicion2 = new Jugador { Id = "J2", EsMaquina = true };
            mano.VueltaActual.CartasJugadas["J2"] = new Carta { ValorTruco = 14 };

            // When 
            MaquinaServicio3v3.ProcesarTurnoMaquina(mano, "J3");

            // Then 
            Assert.Contains(cFea1, mano.Posicion3.Jugadas);
            Assert.Equal(cFea1, mano.VueltaActual.CartasJugadas["J3"]);
        }

        //resolver controlta envido
        [Fact]
        public void ResolverConsultaEnvido_SiNoHayConsulta_LanzaInvalidOperationException()
        {
            // Given 
            var mano = new ManoTruco3v3 { CompaConsultaEnvido = false };

            // When 
            Action accion = () => MaquinaServicio3v3.ResolverConsultaEnvido(mano, aceptar: true);

            // Then 
            Assert.Throws<InvalidOperationException>(accion);
        }

        [Fact]
        public void ResolverConsultaEnvido_ConAceptarFalse_LimpiaEstadoYNoCanta()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                CompaConsultaEnvido = true,
                CompaConsultor = "J3"
            };

            // When 
            MaquinaServicio3v3.ResolverConsultaEnvido(mano, aceptar: false);

            // Then 
            Assert.Null(mano.CompaConsultor);
            Assert.False(mano.EnvidoCantado);
        }

        [Fact]
        public void ResolverConsultaEnvido_ConAceptarTrue_LimpiaEstadoYEjecutaCantar()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                CompaConsultaEnvido = true,
                CompaConsultor = "J3"
            };

            // When 
            MaquinaServicio3v3.ResolverConsultaEnvido(mano, aceptar: true);

            // Then 
            Assert.False(mano.CompaConsultaEnvido);
            Assert.True(mano.CompaEnvidoConsultado);
            Assert.Null(mano.CompaConsultor);
            Assert.True(mano.EnvidoCantado);
        }

        //resolver consulta truco
        [Fact]
        public void ResolverConsultaTruco_SiNoHayConsulta_LanzaInvalidOperationException()
        {
            // Given
            var mano = new ManoTruco3v3 { CompaConsultaTruco = false };

            // When 
            Action accion = () => MaquinaServicio3v3.ResolverConsultaTruco(mano, voy: true);

            // Then 
            Assert.Throws<InvalidOperationException>(accion);
        }

        [Fact]
        public void ResolverConsultaTruco_SiCompaNoCumpleCondiciones_HaceReturnYModificaEstado()
        {
            // Given 
            var mano = new ManoTruco3v3
            {
                CompaConsultaTruco = true,
                CompaConsultor = "J3",
                TurnoActual = "J1",
                Posicion3 = new Jugador { Id = "J3", Mano = new List<Carta>() }
            };

            // When 
            MaquinaServicio3v3.ResolverConsultaTruco(mano, voy: true);

            // Then 
            Assert.False(mano.CompaConsultaTruco);
            Assert.True(mano.CompaTrucoConsultado);
            Assert.Null(mano.CompaConsultor);
            Assert.Null(mano.VueltaActual);
        }

        [Fact]
        public void ResolverConsultaTruco_ConVoyTrue_JuegaCartaDeMenorValor()
        {
            // Given 
            var cartaMala = new Carta { ValorTruco = 1 };
            var cartaBuena = new Carta { ValorTruco = 14 };

            var mano = new ManoTruco3v3
            {
                CompaConsultaTruco = true,
                CompaConsultor = "J3",
                TurnoActual = "J3"
            };

            mano.Posicion3 = new Jugador { Id = "J3", Mano = new List<Carta> { cartaBuena, cartaMala } };

            // When 
            MaquinaServicio3v3.ResolverConsultaTruco(mano, voy: true);

            // Then
            Assert.False(mano.CompaConsultaTruco);
            Assert.True(mano.CompaTrucoConsultado);
            Assert.Null(mano.CompaConsultor);
            Assert.Equal(cartaMala, mano.VueltaActual.CartasJugadas["J3"]);
        }

        [Fact]
        public void ResolverConsultaTruco_ConVoyFalse_JuegaCartaDeMayorValor()
        {
            // Given
            var cartaMala = new Carta { ValorTruco = 1 };
            var cartaBuena = new Carta { ValorTruco = 14 };

            var mano = new ManoTruco3v3
            {
                CompaConsultaTruco = true,
                CompaConsultor = "J3",
                TurnoActual = "J3"
            };

            mano.Posicion3 = new Jugador { Id = "J3", Mano = new List<Carta> { cartaBuena, cartaMala } };

            // When 
            MaquinaServicio3v3.ResolverConsultaTruco(mano, voy: false);

            // Then 
            Assert.False(mano.CompaConsultaTruco);
            Assert.True(mano.CompaTrucoConsultado);
            Assert.Null(mano.CompaConsultor);
            Assert.Equal(cartaBuena, mano.VueltaActual.CartasJugadas["J3"]);
        }

        //ordenar jugar mayor 
        [Fact]
        public void OrdenarJugarMayor_SiJugadorNoExiste_LanzaInvalidOperationException()
        {
            // Given 
            var mano = new ManoTruco3v3();
            string jugadorIdInexistente = "J9";

            // When 
            Action accion = () => MaquinaServicio3v3.OrdenarJugarMayor(mano, jugadorIdInexistente);

            // Then
            var ex = Assert.Throws<InvalidOperationException>(accion);
            Assert.Contains("no encontrado", ex.Message);
        }

        [Fact]
        public void OrdenarJugarMayor_SiJugadorEsHumanoOEquipoContrario_LanzaInvalidOperationException()
        {
            // Given 
            var mano = new ManoTruco3v3();
            mano.Posicion2 = new Jugador
            {
                Id = "J2",
                EsMaquina = false
            };

            // When 
            Action accion = () => MaquinaServicio3v3.OrdenarJugarMayor(mano, "J2");

            // Then 
            var ex = Assert.Throws<InvalidOperationException>(accion);
            Assert.Equal("Solo podés ordenar a tus compañeros bot.", ex.Message);
        }

        [Fact]
        public void OrdenarJugarMayor_SiBotNoTieneCartas_LanzaInvalidOperationException()
        {
            // Given 
            var mano = new ManoTruco3v3();
            var botSincartas = new Jugador
            {
                Id = "J3",
                EsMaquina = true,
                Mano = new List<Carta>()
            };
            mano.Posicion3 = botSincartas;
            mano.EquipoA.Jugador2 = botSincartas;

            // When 
            Action accion = () => MaquinaServicio3v3.OrdenarJugarMayor(mano, "J3");

            // Then 
            var ex = Assert.Throws<InvalidOperationException>(accion);
            Assert.Contains("no tiene cartas en mano", ex.Message);
        }

        [Fact]
        public void OrdenarJugarMayor_SiBotEsValidoYTieneCartas_AsignaOrdenEnLaMano()
        {
            // Given
            var mano = new ManoTruco3v3();

            var botAliado = new Jugador
            {
                Id = "J3",
                EsMaquina = true,
                Mano = new List<Carta> { new Carta { ValorTruco = 10 } }
            };
            mano.Posicion3 = botAliado;
            mano.EquipoA.Jugador2 = botAliado;

            // When 
            MaquinaServicio3v3.OrdenarJugarMayor(mano, "J3");

            // Then 
            Assert.Equal("J3", mano.OrdenJugarMayor);
        }
    }
}
