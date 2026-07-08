using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de DestelloServicio (guardas de EvaluarTurnoHumano y
    /// jugada al azar) y de la apuesta del Timbero al sumar puntos.
    /// </summary>
    public class DestelloYTimberoCoverageTests
    {
        // ── DestelloServicio ─────────────────────────────────────────────

        private static ManoTruco CrearManoLuzMala()
        {
            return new ManoTruco
            {
                Configuracion = new ConfiguracionPartida
                {
                    Modo = ModoJuego.Historia,
                    RivalDeLaMaquina = ClaseRival.LuzMala,
                    RivalNivel = 4
                },
                TurnoActual = IdJugador.Humano,
                Humano = new Jugador
                {
                    Mano = new List<Carta>
                    {
                        new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 },
                        new Carta { Numero = 4, Palo = "Copa", ValorTruco = 1 },
                    }
                },
                Maquina = new Jugador { Mano = new List<Carta>() }
            };
        }

        [Fact]
        public void Evaluar_ConOtroRival_NoHaceNada()
        {
            var mano = CrearManoLuzMala();
            mano.Configuracion.RivalDeLaMaquina = ClaseRival.Lobizon;
            mano.DestelloPendiente = true;
            mano.DestelloBazaObjetivo = 1;

            DestelloServicio.EvaluarTurnoHumano(mano);

            Assert.False(mano.DestelloBloqueando);
        }

        [Fact]
        public void Evaluar_SiElEspejismoYaSeUso_NoHaceNada()
        {
            var mano = CrearManoLuzMala();
            mano.EspejismoUsadoEnMano = true;
            mano.DestelloPendiente = true;
            mano.DestelloBazaObjetivo = 1;

            DestelloServicio.EvaluarTurnoHumano(mano);

            Assert.False(mano.DestelloBloqueando);
        }

        [Fact]
        public void Evaluar_ConDestelloObjetivoEnEstaBaza_Bloquea()
        {
            var mano = CrearManoLuzMala();
            mano.DestelloPendiente = true;
            mano.DestelloBazaObjetivo = 1;

            DestelloServicio.EvaluarTurnoHumano(mano);

            Assert.True(mano.DestelloBloqueando);
            Assert.Contains("Destello", mano.UltimoMensajeHabilidadRival ?? "");
        }

        [Fact]
        public void Evaluar_SiElObjetivoEsOtraBaza_NoBloquea()
        {
            var mano = CrearManoLuzMala();
            mano.DestelloPendiente = true;
            mano.DestelloBazaObjetivo = 2; // estamos en la baza 1

            DestelloServicio.EvaluarTurnoHumano(mano);

            Assert.False(mano.DestelloBloqueando);
        }

        [Fact]
        public void Evaluar_NoEsMomentoDelHumano_NoHaceNada()
        {
            var casos = new List<ManoTruco>();

            var ganada = CrearManoLuzMala();
            ganada.GanadorMano = IdJugador.Humano;
            casos.Add(ganada);

            var conEnvido = CrearManoLuzMala();
            conEnvido.EnvidoPendienteRespuestaHumano = true;
            casos.Add(conEnvido);

            var conCartaJugada = CrearManoLuzMala();
            conCartaJugada.CartaHumanoEnMesa = new Carta { Numero = 4, Palo = "Copa" };
            casos.Add(conCartaJugada);

            var sinCartas = CrearManoLuzMala();
            sinCartas.Humano.Mano.Clear();
            casos.Add(sinCartas);

            var turnoMaquina = CrearManoLuzMala();
            turnoMaquina.TurnoActual = IdJugador.Maquina;
            casos.Add(turnoMaquina);

            foreach (var mano in casos)
            {
                mano.DestelloPendiente = true;
                mano.DestelloBazaObjetivo = 1;
                DestelloServicio.EvaluarTurnoHumano(mano);
                Assert.False(mano.DestelloBloqueando);
            }
        }

        [Fact]
        public void Evaluar_EnTercerBaza_YaNoActiva()
        {
            var mano = CrearManoLuzMala();
            mano.Bazas.Add(new Baza());
            mano.Bazas.Add(new Baza());
            mano.DestelloPendiente = true;
            mano.DestelloBazaObjetivo = 1;

            DestelloServicio.EvaluarTurnoHumano(mano);

            Assert.False(mano.DestelloBloqueando);
        }

        [Fact]
        public void Evaluar_ConContadorImpar_NoProgramaDestello()
        {
            var mano = CrearManoLuzMala();
            mano.ContadorTurnosHumanoPartida = 1;

            DestelloServicio.EvaluarTurnoHumano(mano);

            Assert.False(mano.DestelloPendiente);
            Assert.False(mano.DestelloBloqueando);
        }

        [Fact]
        public void Evaluar_ConContadorPar_ProgramaUnDestello()
        {
            var mano = CrearManoLuzMala();
            mano.ContadorTurnosHumanoPartida = 2;

            DestelloServicio.EvaluarTurnoHumano(mano);

            Assert.True(mano.DestelloPendiente);
            Assert.InRange(mano.DestelloBazaObjetivo, 1, 2);
            // Solo bloquea si el objetivo sorteado es la baza actual (la 1)
            Assert.Equal(mano.DestelloBazaObjetivo == 1, mano.DestelloBloqueando);
        }

        [Fact]
        public void CompletarDestello_LimpiaElEstado()
        {
            var mano = CrearManoLuzMala();
            mano.DestelloPendiente = true;
            mano.DestelloBazaObjetivo = 2;

            DestelloServicio.CompletarDestello(mano);

            Assert.False(mano.DestelloPendiente);
            Assert.Equal(0, mano.DestelloBazaObjetivo);
        }

        [Fact]
        public void JugarCartaAleatoria_SinCartas_LanzaInvalidOperationException()
        {
            var mano = CrearManoLuzMala();
            mano.Humano.Mano.Clear();

            Assert.Throws<InvalidOperationException>(() => DestelloServicio.JugarCartaAleatoria(mano));
        }

        [Fact]
        public void JugarCartaAleatoria_JuegaUnaCartaYCompletaElDestello()
        {
            var mano = CrearManoLuzMala();
            mano.DestelloPendiente = true;
            mano.DestelloBazaObjetivo = 1;

            DestelloServicio.JugarCartaAleatoria(mano);

            Assert.Single(mano.Humano.Mano);
            Assert.False(mano.DestelloPendiente);
            Assert.Contains("Destello", mano.UltimoMensajeHabilidadRival ?? "");
        }

        // ── Timbero: la apuesta al sumar puntos ──────────────────────────

        private static ManoTruco CrearManoTimberoConApuesta()
        {
            var mano = new ManoTruco
            {
                Configuracion = new ConfiguracionPartida
                {
                    Modo = ModoJuego.Historia,
                    HeroeDelHumano = ClaseHeroe.Timbero
                }
            };
            var estado = mano.EstadoHabilidades.ObtenerOCrear(IdJugador.Humano, ClaseHeroe.Timbero);
            estado.TimberoApuestaActiva = true;
            return mano;
        }

        [Fact]
        public void SumarPuntos_SiElTimberoGanaLaMano_DuplicaLosPuntos()
        {
            var mano = CrearManoTimberoConApuesta();

            JuegoServicio.SumarPuntos(mano, IdJugador.Humano, 2, OrigenPuntos.TrucoMano);

            Assert.Equal(4, mano.PuntosHumano);
            Assert.False(mano.EstadoHabilidades.Obtener(IdJugador.Humano)!.TimberoApuestaActiva);
            Assert.Contains("apuesta", mano.UltimoMensajeHabilidad ?? "");
        }

        [Fact]
        public void SumarPuntos_SiElTimberoPierdeLaMano_LaMaquinaCobraDosExtra()
        {
            var mano = CrearManoTimberoConApuesta();

            JuegoServicio.SumarPuntos(mano, IdJugador.Maquina, 2, OrigenPuntos.TrucoMano);

            Assert.Equal(4, mano.PuntosMaquina); // 2 + 2 de castigo
            Assert.False(mano.EstadoHabilidades.Obtener(IdJugador.Humano)!.TimberoApuestaActiva);
        }

        [Fact]
        public void SumarPuntos_ConOtroOrigen_LaApuestaNoSeToca()
        {
            var mano = CrearManoTimberoConApuesta();

            JuegoServicio.SumarPuntos(mano, IdJugador.Humano, 2, OrigenPuntos.Envido);

            Assert.Equal(2, mano.PuntosHumano);
            Assert.True(mano.EstadoHabilidades.Obtener(IdJugador.Humano)!.TimberoApuestaActiva);
        }
    }
}
