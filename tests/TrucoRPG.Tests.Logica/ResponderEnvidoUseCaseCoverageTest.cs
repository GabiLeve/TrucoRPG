using Xunit;
using System;
using System.Collections.Generic;
using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Servicios;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.Tests.Logica
{
    /// <summary>
    /// Cobertura de ramas de ResponderEnvidoUseCase: validaciones, todas las
    /// escalaciones posibles y "son buenas".
    /// </summary>
    public class ResponderEnvidoUseCaseCoverageTest
    {
        private static List<Carta> CartasConTanto33() => new()
        {
            new Carta { Numero = 7, Palo = "Oro" },
            new Carta { Numero = 6, Palo = "Oro" },
            new Carta { Numero = 1, Palo = "Basto" }
        };

        private static List<Carta> CartasConTanto26() => new()
        {
            new Carta { Numero = 4, Palo = "Copa" },
            new Carta { Numero = 2, Palo = "Copa" },
            new Carta { Numero = 1, Palo = "Espada" }
        };

        private ManoTruco CrearManoConEnvidoPendiente(Guid id, string tipo = "Envido")
        {
            var mano = new ManoTruco
            {
                Id = id,
                EnvidoCantado = true,
                EnvidoPendienteRespuestaHumano = true,
                TipoEnvidoCantado = tipo,
                CantorEnvido = "Maquina",
                NivelMentiraEnvidoMaquina = 0,
                Humano = new Jugador { Mano = CartasConTanto26() },
                Maquina = new Jugador { Mano = CartasConTanto33() }
            };
            PartidaMemoriaServicio.Guardar(mano);
            return mano;
        }

        // ── Validaciones ─────────────────────────────────────────────────

        [Fact]
        public void Ejecutar_PartidaTerminada_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoConEnvidoPendiente(id);
            mano.PartidaTerminada = true;

            Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: null));
        }

        [Fact]
        public void Ejecutar_SinEnvidoCantado_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoConEnvidoPendiente(id);
            mano.EnvidoCantado = false;

            var ex = Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: null));
            Assert.Contains("No hay un envido pendiente", ex.Message);
        }

        // ── Quiero simple ────────────────────────────────────────────────

        [Fact]
        public void Ejecutar_QuieroSimple_ResuelveElEnvidoYGanaLaMaquina()
        {
            var id = Guid.NewGuid();
            CrearManoConEnvidoPendiente(id); // humano 26 vs máquina 33

            var resultado = new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: null);

            Assert.True(resultado.EnvidoResuelto);
            Assert.Equal("Maquina", resultado.GanadorEnvido);
            Assert.Equal(2, resultado.PuntosEnvido);
        }

        [Fact]
        public void Ejecutar_QuieroSimpleConMejorTantoHumano_GanaElHumano()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoConEnvidoPendiente(id);
            mano.Humano.Mano = CartasConTanto33();
            mano.Maquina.Mano = CartasConTanto26();

            var resultado = new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: null);

            Assert.True(resultado.EnvidoResuelto);
            Assert.Equal("Humano", resultado.GanadorEnvido);
        }

        // ── Escalaciones ─────────────────────────────────────────────────

        [Fact]
        public void Ejecutar_EscalarAEnvidoEnvido_SiMaquinaNoQuiere_GanaElHumanoLosPuntosAnteriores()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoConEnvidoPendiente(id);
            mano.Maquina.Mano = new List<Carta>(); // tanto 0 y sin mentira => rechaza seguro

            var resultado = new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: "envido envido");

            Assert.True(resultado.EnvidoResuelto);
            Assert.Equal("Humano", resultado.GanadorEnvido);
            Assert.Equal(2, resultado.PuntosEnvido); // lo que valía el Envido original
            Assert.Contains("no quiso", resultado.EstadoEnvido);
        }

        [Fact]
        public void Ejecutar_EscalarARealEnvidoDesdeEnvidoEnvido_SiMaquinaAcepta_AcumulaLosPuntos()
        {
            var id = Guid.NewGuid();
            CrearManoConEnvidoPendiente(id, tipo: "EnvidoEnvido"); // máquina tanto 33 => acepta seguro

            var resultado = new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: "real envido");

            Assert.True(resultado.EnvidoResuelto);
            Assert.Equal(7, resultado.PuntosEnvido); // 4 del envido envido + 3 del real
            Assert.Equal("Maquina", resultado.GanadorEnvido);
        }

        [Fact]
        public void Ejecutar_EscalarAFaltaEnvidoDesdeRealEnvido_CalculaLaFaltaDelLider()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoConEnvidoPendiente(id, tipo: "RealEnvido");
            mano.PuntosMaquina = 25; // al líder le faltan 5 para 30

            var resultado = new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: "falta envido");

            Assert.True(resultado.EnvidoResuelto);
            Assert.Equal(5, resultado.PuntosEnvido);
        }

        [Fact]
        public void Ejecutar_EscalacionInvalidaParaElTipoActual_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            CrearManoConEnvidoPendiente(id, tipo: "RealEnvido");

            var ex = Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: "envido"));
            Assert.Contains("No podés escalar", ex.Message);
        }

        [Fact]
        public void Ejecutar_EscalacionDesconocida_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            CrearManoConEnvidoPendiente(id);

            Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().Ejecutar(id, aceptar: true, escalarA: "truco"));
        }

        // ── Son buenas ───────────────────────────────────────────────────

        private ManoTruco CrearManoParaSonBuenas(Guid id, string tipo = "Envido")
        {
            var mano = CrearManoConEnvidoPendiente(id, tipo);
            mano.EnvidoPendienteRespuestaHumano = false; // ya dijo quiero
            mano.EnvidoResuelto = false;
            PartidaMemoriaServicio.Actualizar(mano);
            return mano;
        }

        [Fact]
        public void EjecutarSonBuenas_ManoInexistente_LanzaKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(
                () => new ResponderEnvidoUseCase().EjecutarSonBuenas(Guid.NewGuid()));
        }

        [Fact]
        public void EjecutarSonBuenas_PartidaTerminada_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoParaSonBuenas(id);
            mano.PartidaTerminada = true;

            Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().EjecutarSonBuenas(id));
        }

        [Fact]
        public void EjecutarSonBuenas_SinEnvidoCantado_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoParaSonBuenas(id);
            mano.EnvidoCantado = false;

            Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().EjecutarSonBuenas(id));
        }

        [Fact]
        public void EjecutarSonBuenas_EnvidoYaResuelto_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoParaSonBuenas(id);
            mano.EnvidoResuelto = true;

            Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().EjecutarSonBuenas(id));
        }

        [Fact]
        public void EjecutarSonBuenas_SiCantoElHumano_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoParaSonBuenas(id);
            mano.CantorEnvido = "Humano";

            var ex = Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().EjecutarSonBuenas(id));
            Assert.Contains("solo aplica", ex.Message);
        }

        [Fact]
        public void EjecutarSonBuenas_ConRespuestaPendiente_LanzaInvalidOperationException()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoParaSonBuenas(id);
            mano.EnvidoPendienteRespuestaHumano = true;

            var ex = Assert.Throws<InvalidOperationException>(
                () => new ResponderEnvidoUseCase().EjecutarSonBuenas(id));
            Assert.Contains("aceptar el envido", ex.Message);
        }

        [Fact]
        public void EjecutarSonBuenas_CasoValido_LaMaquinaGanaLosPuntosDelTipo()
        {
            var id = Guid.NewGuid();
            CrearManoParaSonBuenas(id);

            var resultado = new ResponderEnvidoUseCase().EjecutarSonBuenas(id);

            Assert.True(resultado.EnvidoResuelto);
            Assert.True(resultado.SonBuenasDeclarado);
            Assert.Equal("Maquina", resultado.GanadorEnvido);
            Assert.Equal(2, resultado.PuntosEnvido);
            Assert.Equal("resuelto", resultado.FaseEnvido);
        }

        [Fact]
        public void EjecutarSonBuenas_ConFaltaEnvido_UsaLaFaltaDelLider()
        {
            var id = Guid.NewGuid();
            var mano = CrearManoParaSonBuenas(id, tipo: "FaltaEnvido");
            mano.PuntosHumano = 22; // líder: le faltan 8

            var resultado = new ResponderEnvidoUseCase().EjecutarSonBuenas(id);

            Assert.True(resultado.EnvidoResuelto);
            Assert.Equal(8, resultado.PuntosEnvido);
        }
    }
}
