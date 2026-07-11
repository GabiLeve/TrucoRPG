using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de MandingaServicio: acumulación y liquidación de la
    /// maldición, el Espejo (copia de carta), traslado de estado y desbloqueos.
    /// </summary>
    public class MandingaServicioCoverageTests
    {
        private static ManoTruco CrearManoMandinga(int numeroDeMano = 1, int puntosHumano = 0)
        {
            var mano = new ManoTruco
            {
                Configuracion = new ConfiguracionPartida
                {
                    Modo = ModoJuego.Historia,
                    RivalDeLaMaquina = ClaseRival.Mandinga,
                    RivalNivel = 5
                },
                NumeroDeMano = numeroDeMano,
                PuntosHumano = puntosHumano,
                Humano = new Jugador
                {
                    Mano = new List<Carta>
                    {
                        new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
                        new Carta { Numero = 7, Palo = "Oro", ValorTruco = 11 },
                        new Carta { Numero = 3, Palo = "Basto", ValorTruco = 10 },
                    }
                },
                Maquina = new Jugador
                {
                    Mano = new List<Carta>
                    {
                        new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 },
                        new Carta { Numero = 5, Palo = "Espada", ValorTruco = 2 },
                        new Carta { Numero = 6, Palo = "Oro", ValorTruco = 3 },
                    }
                }
            };
            PartidaMemoriaServicio.Guardar(mano);
            return mano;
        }

        // ── AcumularPuntos ───────────────────────────────────────────────

        [Fact]
        public void AcumularPuntos_SumaAlAcumuladorDeCadaJugador()
        {
            var mano = CrearManoMandinga();

            MandingaServicio.AcumularPuntos(mano, IdJugador.Humano, 2);
            MandingaServicio.AcumularPuntos(mano, IdJugador.Maquina, 3);
            MandingaServicio.AcumularPuntos(mano, IdJugador.Humano, 0); // se ignora

            Assert.Equal(2, mano.PuntosHumanoAcumuladosMano);
            Assert.Equal(3, mano.PuntosMaquinaAcumuladosMano);
        }

        // ── LiquidarPuntosMaldicion ──────────────────────────────────────

        private static ManoTruco CrearManoConMaldicion(string ganadorMano, int hum, int maq)
        {
            var mano = CrearManoMandinga();
            mano.MandingaMaldicionActivaEnMano = true;
            mano.GanadorMano = ganadorMano;
            mano.PuntosHumanoAcumuladosMano = hum;
            mano.PuntosMaquinaAcumuladosMano = maq;
            return mano;
        }

        [Fact]
        public void Liquidar_SiGanaElHumano_PierdeUnPuntoYLaMaquinaCobraLoSuyo()
        {
            var mano = CrearManoConMaldicion(IdJugador.Humano, hum: 3, maq: 2);

            MandingaServicio.LiquidarPuntosMaldicion(mano);

            Assert.Equal(2, mano.PuntosHumano);   // 3 - 1
            Assert.Equal(2, mano.PuntosMaquina);  // lo acumulado por la máquina
            Assert.Equal(0, mano.PuntosHumanoAcumuladosMano);
            Assert.False(mano.MandingaMaldicionActivaEnMano);
        }

        [Fact]
        public void Liquidar_SiGanaLaMaquina_ElDiabloDuplica()
        {
            var mano = CrearManoConMaldicion(IdJugador.Maquina, hum: 3, maq: 2);

            MandingaServicio.LiquidarPuntosMaldicion(mano);

            Assert.Equal(4, mano.PuntosMaquina);  // 2 * 2
            Assert.Equal(3, mano.PuntosHumano);   // lo acumulado por el humano
        }

        [Fact]
        public void Liquidar_SiLaMaquinaLlegaA30_TerminaLaPartida()
        {
            var mano = CrearManoConMaldicion(IdJugador.Maquina, hum: 0, maq: 2);
            mano.PuntosMaquina = 27;

            MandingaServicio.LiquidarPuntosMaldicion(mano);

            Assert.True(mano.PartidaTerminada);
            Assert.Equal(IdJugador.Maquina, mano.GanadorPartida);
        }

        [Fact]
        public void Liquidar_SiElHumanoLlegaA30_TerminaLaPartida()
        {
            var mano = CrearManoConMaldicion(IdJugador.Humano, hum: 3, maq: 0);
            mano.PuntosHumano = 28;

            MandingaServicio.LiquidarPuntosMaldicion(mano);

            Assert.True(mano.PartidaTerminada);
            Assert.Equal(IdJugador.Humano, mano.GanadorPartida);
        }

        [Fact]
        public void Liquidar_SinMaldicionActiva_NoHaceNada()
        {
            var mano = CrearManoMandinga();
            mano.GanadorMano = IdJugador.Humano;
            mano.PuntosHumanoAcumuladosMano = 3;

            MandingaServicio.LiquidarPuntosMaldicion(mano);

            Assert.Equal(0, mano.PuntosHumano);
            Assert.Equal(3, mano.PuntosHumanoAcumuladosMano);
        }

        // ── Espejo ───────────────────────────────────────────────────────

        [Fact]
        public void OnManoIniciada_ConFase3YCartaLibre_ElEspejoCopiaLaCartaMasAlta()
        {
            var mano = CrearManoMandinga(numeroDeMano: 2, puntosHumano: 20); // desbloquea fase 3
            mano.MandingaJugadasHumanoManoAnterior = new List<Carta>
            {
                new Carta { Numero = 2, Palo = "Basto", ValorTruco = 9 },
                new Carta { Numero = 4, Palo = "Oro", ValorTruco = 1 },
            };

            MandingaServicio.OnManoIniciada(mano);

            Assert.True(mano.MandingaEspejoBloqueando);
            // La copia reemplaza la carta más baja de la máquina (el 4 de Copa)
            Assert.Contains(mano.Maquina.Mano, c => c.Numero == 2 && c.Palo == "Basto");
            Assert.DoesNotContain(mano.Maquina.Mano, c => c.Numero == 4 && c.Palo == "Copa");
        }

        [Fact]
        public void OnManoIniciada_SiLaCartaEstaEnJuego_NoActivaEspejo()
        {
            var mano = CrearManoMandinga(numeroDeMano: 2, puntosHumano: 20);
            // La única candidata es el ancho de espada, que el humano tiene en la mano
            mano.MandingaJugadasHumanoManoAnterior = new List<Carta>
            {
                new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
            };

            MandingaServicio.OnManoIniciada(mano);

            Assert.False(mano.MandingaEspejoBloqueando);
        }

        [Fact]
        public void ConfirmarEspejo_DesbloqueaYSigueConElEngano()
        {
            var mano = CrearManoMandinga(numeroDeMano: 2, puntosHumano: 20);
            mano.MandingaJugadasHumanoManoAnterior = new List<Carta>
            {
                new Carta { Numero = 2, Palo = "Basto", ValorTruco = 9 },
            };
            MandingaServicio.OnManoIniciada(mano); // espejo bloqueando + engaño programado

            MandingaServicio.ConfirmarEspejo(mano);

            Assert.False(mano.MandingaEspejoBloqueando);
            Assert.True(mano.MandingaEnganoBloqueando); // siguiente overlay de la cola

            MandingaServicio.ConfirmarEngano(mano);

            Assert.False(mano.MandingaEnganoBloqueando);
            Assert.True(mano.MandingaEnganoManoOculta);
            Assert.False(mano.MandingaMaldicionBloqueando); // mano par: sin pacto
        }

        [Fact]
        public void ConfirmarEspejo_SiNoEstaBloqueando_NoHaceNada()
        {
            var mano = CrearManoMandinga();
            mano.UltimoMensajeHabilidadRival = "previo";

            MandingaServicio.ConfirmarEspejo(mano);

            Assert.Equal("previo", mano.UltimoMensajeHabilidadRival);
        }

        [Fact]
        public void ConfirmarEngano_SiNoEstaBloqueando_NoHaceNada()
        {
            var mano = CrearManoMandinga();

            MandingaServicio.ConfirmarEngano(mano);

            Assert.False(mano.MandingaEnganoManoOculta);
        }

        [Fact]
        public void ConfirmarMaldicion_ActivaLaMaldicionSoloSiEstabaBloqueando()
        {
            var mano = CrearManoMandinga();
            MandingaServicio.ConfirmarMaldicion(mano);
            Assert.False(mano.MandingaMaldicionActivaEnMano);

            mano.MandingaMaldicionBloqueando = true;
            MandingaServicio.ConfirmarMaldicion(mano);
            Assert.True(mano.MandingaMaldicionActivaEnMano);
        }

        [Fact]
        public void OnManoIniciada_ConCadenciaDelEngano_SeRepiteCadaTresManos()
        {
            var mano = CrearManoMandinga(numeroDeMano: 6, puntosHumano: 15); // fase 2
            mano.MandingaPrimeraManoEngano = 3; // (6 - 3) % 3 == 0 => toca engaño

            MandingaServicio.OnManoIniciada(mano);

            Assert.True(mano.MandingaEnganoBloqueando);
        }

        // ── Estado entre manos y desbloqueos ─────────────────────────────

        [Fact]
        public void TrasladarEstadoPartida_SinManoAnterior_NoHaceNada()
        {
            var nueva = CrearManoMandinga();

            MandingaServicio.TrasladarEstadoPartida(null, nueva);

            Assert.False(nueva.MandingaFase2Desbloqueada);
        }

        [Fact]
        public void TrasladarEstadoPartida_CopiaFasesYJugadasAnteriores()
        {
            var anterior = CrearManoMandinga();
            anterior.MandingaFase2Desbloqueada = true;
            anterior.MandingaFase3Desbloqueada = true;
            anterior.MandingaPrimeraManoEngano = 4;
            anterior.MandingaJugadasHumanoManoAnterior = new List<Carta>
            {
                new Carta { Numero = 3, Palo = "Espada", ValorTruco = 13 }
            };
            var nueva = CrearManoMandinga(numeroDeMano: 2);

            MandingaServicio.TrasladarEstadoPartida(anterior, nueva);

            Assert.True(nueva.MandingaFase2Desbloqueada);
            Assert.True(nueva.MandingaFase3Desbloqueada);
            Assert.Equal(4, nueva.MandingaPrimeraManoEngano);
            Assert.Single(nueva.MandingaJugadasHumanoManoAnterior);
        }

        [Theory]
        [InlineData(5, false, false)]
        [InlineData(15, true, false)]
        [InlineData(25, true, true)]
        public void SincronizarDesbloqueosFases_SegunLosPuntosDelHumano(int puntos, bool fase2, bool fase3)
        {
            var mano = CrearManoMandinga(puntosHumano: puntos);

            MandingaServicio.SincronizarDesbloqueosFases(mano);

            Assert.Equal(fase2, mano.MandingaFase2Desbloqueada);
            Assert.Equal(fase3, mano.MandingaFase3Desbloqueada);
        }

        [Fact]
        public void RegistrarFinMano_GuardaSoloLasCartasJugadasPorElHumano()
        {
            var mano = CrearManoMandinga();
            mano.Bazas.Add(new Baza { CartaJugador = new Carta { Numero = 7, Palo = "Espada", ValorTruco = 12 } });
            mano.Bazas.Add(new Baza { CartaJugador = null });

            MandingaServicio.RegistrarFinMano(mano);

            Assert.Single(mano.MandingaJugadasHumanoManoAnterior);
            Assert.Equal(7, mano.MandingaJugadasHumanoManoAnterior[0].Numero);
        }

        [Fact]
        public void RegistrarFinMano_FueraDeHistoriaMandinga_NoHaceNada()
        {
            var mano = new ManoTruco
            {
                Configuracion = new ConfiguracionPartida { Modo = ModoJuego.Tradicional },
                MandingaJugadasHumanoManoAnterior = new List<Carta> { new Carta { Numero = 5, Palo = "Oro" } }
            };
            mano.Bazas.Add(new Baza { CartaJugador = new Carta { Numero = 7, Palo = "Espada" } });

            MandingaServicio.RegistrarFinMano(mano);

            Assert.Equal(5, mano.MandingaJugadasHumanoManoAnterior[0].Numero); // intacto
        }
    }
}
