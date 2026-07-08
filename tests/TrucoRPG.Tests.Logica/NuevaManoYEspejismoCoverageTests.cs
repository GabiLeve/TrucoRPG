using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de NuevaManoUseCase (validaciones y traslado de estado)
    /// y de las guardas de EspejismoServicio.
    /// </summary>
    public class NuevaManoYEspejismoCoverageTests : IDisposable
    {
        public void Dispose()
        {
            AzarServicio.TirarProbabilidadOverride = null;
            AzarServicio.MonedaCaraOverride = null;
        }

        // ── NuevaManoUseCase ─────────────────────────────────────────────

        [Fact]
        public void Ejecutar_SinManoAnterior_CreaLaPrimeraMano()
        {
            var mano = new NuevaManoUseCase().Ejecutar(null);

            Assert.NotNull(mano);
            Assert.Equal(1, mano.NumeroDeMano);
            Assert.Equal(0, mano.PuntosHumano);
            Assert.Equal(0, mano.PuntosMaquina);
        }

        [Fact]
        public void Ejecutar_ManoAnteriorInexistente_LanzaKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => new NuevaManoUseCase().Ejecutar(Guid.NewGuid()));
        }

        [Fact]
        public void Ejecutar_ConPartidaTerminada_LanzaInvalidOperationException()
        {
            var anterior = new NuevaManoUseCase().Ejecutar(null);
            anterior.PartidaTerminada = true;
            PartidaMemoriaServicio.Actualizar(anterior);

            var ex = Assert.Throws<InvalidOperationException>(
                () => new NuevaManoUseCase().Ejecutar(anterior.Id));
            Assert.Contains("ya terminó", ex.Message);
        }

        [Fact]
        public void Ejecutar_ConManoAnterior_TrasladaPuntosYNumeroDeMano()
        {
            var anterior = new NuevaManoUseCase().Ejecutar(null);
            anterior.PuntosHumano = 7;
            anterior.PuntosMaquina = 4;
            anterior.NivelMentiraEnvidoMaquina = 30;
            anterior.NivelMentiraTrucoMaquina = 40;
            PartidaMemoriaServicio.Actualizar(anterior);

            var nueva = new NuevaManoUseCase().Ejecutar(anterior.Id);

            Assert.Equal(anterior.NumeroDeMano + 1, nueva.NumeroDeMano);
            Assert.Equal(7, nueva.PuntosHumano);
            Assert.Equal(4, nueva.PuntosMaquina);
            Assert.Equal(30, nueva.NivelMentiraEnvidoMaquina);
            Assert.Equal(40, nueva.NivelMentiraTrucoMaquina);
        }

        [Fact]
        public void EjecutarNuevaPartida_SinConfiguracion_CreaManoInicial()
        {
            var mano = new NuevaManoUseCase().EjecutarNuevaPartida(null);

            Assert.NotNull(mano);
            Assert.Equal(0, mano.PuntosHumano);
            Assert.False(mano.PartidaTerminada);
        }

        [Fact]
        public void EjecutarNuevaPartida_EnModoHistoria_NoProcesaLaIniciativaAutomatica()
        {
            var config = new ConfiguracionPartida
            {
                Modo = ModoJuego.Historia,
                RivalDeLaMaquina = ClaseRival.LuzMala,
                RivalNivel = 1
            };

            var mano = new NuevaManoUseCase().EjecutarNuevaPartida(config);

            Assert.NotNull(mano);
            Assert.Equal(ModoJuego.Historia, mano.Configuracion.Modo);
        }

        // ── EspejismoServicio: guardas ───────────────────────────────────

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
                ManoIniciadaPor = IdJugador.Maquina,
                TurnoActual = IdJugador.Maquina,
                CartaMaquinaEnMesa = new Carta { Numero = 3, Palo = "Oro", ValorTruco = 10 },
                Humano = new Jugador { Mano = new List<Carta> { new Carta { Numero = 1, Palo = "Espada", ValorTruco = 14 } } },
                Maquina = new Jugador { Mano = new List<Carta> { new Carta { Numero = 7, Palo = "Copa", ValorTruco = 4 } } }
            };
        }

        [Fact]
        public void Intentar_SiElRivalNoEsLuzMala_DevuelveFalse()
        {
            var mano = CrearManoLuzMala();
            mano.Configuracion.RivalDeLaMaquina = ClaseRival.Lobizon;

            Assert.False(EspejismoServicio.IntentarAlJugarPrimeraCarta(mano));
        }

        [Fact]
        public void Intentar_SiLaManoLaInicioElHumano_DevuelveFalse()
        {
            var mano = CrearManoLuzMala();
            mano.ManoIniciadaPor = IdJugador.Humano;

            Assert.False(EspejismoServicio.IntentarAlJugarPrimeraCarta(mano));
        }

        [Fact]
        public void Intentar_SiYaHayBazasJugadas_DevuelveFalse()
        {
            var mano = CrearManoLuzMala();
            mano.Bazas.Add(new Baza());

            Assert.False(EspejismoServicio.IntentarAlJugarPrimeraCarta(mano));
        }

        [Fact]
        public void Intentar_SinCartaDeLaMaquinaEnMesa_DevuelveFalse()
        {
            var mano = CrearManoLuzMala();
            mano.CartaMaquinaEnMesa = null;

            Assert.False(EspejismoServicio.IntentarAlJugarPrimeraCarta(mano));
        }

        [Fact]
        public void Intentar_SiElEspejismoYaEstaActivo_DevuelveFalse()
        {
            var mano = CrearManoLuzMala();
            mano.EspejismoActivo = true;

            Assert.False(EspejismoServicio.IntentarAlJugarPrimeraCarta(mano));
        }

        [Fact]
        public void Intentar_SiElAzarNoAcompania_DevuelveFalse()
        {
            AzarServicio.TirarProbabilidadOverride = _ => false;
            var mano = CrearManoLuzMala();

            Assert.False(EspejismoServicio.IntentarAlJugarPrimeraCarta(mano));
            Assert.False(mano.EspejismoActivo);
        }

        [Fact]
        public void ConfirmarOverlay_SiNoEstaBloqueando_NoHaceNada()
        {
            var mano = CrearManoLuzMala();
            mano.EspejismoBloqueando = false;

            EspejismoServicio.ConfirmarOverlay(mano);

            Assert.False(mano.EspejismoAlternando);
        }

        [Fact]
        public void ConfirmarOverlay_SiEstaBloqueando_PasaAAlternar()
        {
            var mano = CrearManoLuzMala();
            mano.EspejismoBloqueando = true;

            EspejismoServicio.ConfirmarOverlay(mano);

            Assert.False(mano.EspejismoBloqueando);
            Assert.True(mano.EspejismoAlternando);
        }

        [Fact]
        public void Finalizar_SiNoEstaActivo_NoHaceNada()
        {
            var mano = CrearManoLuzMala();
            mano.EspejismoCartaFalsa = new Carta { Numero = 5, Palo = "Basto" };

            EspejismoServicio.Finalizar(mano);

            Assert.NotNull(mano.EspejismoCartaFalsa); // no se limpió porque no estaba activo
        }

        [Fact]
        public void Finalizar_SiEstaActivo_LimpiaTodoElEstado()
        {
            var mano = CrearManoLuzMala();
            mano.EspejismoActivo = true;
            mano.EspejismoBloqueando = true;
            mano.EspejismoAlternando = true;
            mano.EspejismoMostrarFakePrimero = true;
            mano.EspejismoCartaFalsa = new Carta { Numero = 5, Palo = "Basto" };

            EspejismoServicio.Finalizar(mano);

            Assert.False(mano.EspejismoActivo);
            Assert.False(mano.EspejismoBloqueando);
            Assert.False(mano.EspejismoAlternando);
            Assert.False(mano.EspejismoMostrarFakePrimero);
            Assert.Null(mano.EspejismoCartaFalsa);
        }
    }
}
