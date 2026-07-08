using TrucoRPG.Dominio.Entities;
using TrucoRPG.Dominio.Habilidades;
using TrucoRPG.Dominio.Servicios;

namespace TrucoRPG.Dominio.UseCases
{
    public class NuevaManoUseCase
    {
        public ManoTruco Ejecutar(Guid? manoAnteriorId)
        {
            int numeroDeMano = 1, puntosHumano = 0, puntosMaquina = 0;
            int nivelMentiraEnvido = 0, nivelMentiraTruco = 0;
            ConfiguracionPartida configuracion = new();
            ManoTruco? anterior = null;

            if (manoAnteriorId.HasValue)
            {
                anterior = PartidaMemoriaServicio.Obtener(manoAnteriorId.Value)
                    ?? throw new KeyNotFoundException("No se encontró la mano anterior.");

                if (anterior.PartidaTerminada)
                    throw new InvalidOperationException(
                        "La partida ya terminó. El primero en llegar a 30 gana. Iniciá una nueva partida.");

                numeroDeMano       = anterior.NumeroDeMano + 1;
                puntosHumano       = anterior.PuntosHumano;
                puntosMaquina      = anterior.PuntosMaquina;
                nivelMentiraEnvido = anterior.NivelMentiraEnvidoMaquina;
                nivelMentiraTruco  = anterior.NivelMentiraTrucoMaquina;
                configuracion      = ClonarConfiguracion(anterior.Configuracion);
            }

            var mano = PartidaServicio.CrearManoNueva(
                numeroDeMano, puntosHumano, puntosMaquina, configuracion);

            mano.NivelMentiraEnvidoMaquina = nivelMentiraEnvido;
            mano.NivelMentiraTrucoMaquina  = nivelMentiraTruco;

            // El cooldown de la activa (manos desde el último uso) vive en EstadoHabilidades,
            // que se recrea con cada mano. Lo trasladamos del anterior para que el conteo
            // persista entre manos; si no, la habilidad volvería a estar disponible cada mano.
            TrasladarCooldownHabilidades(anterior, mano);
            TrasladarEstadoDestello(anterior, mano);
            MandingaServicio.TrasladarEstadoPartida(anterior, mano);

            HabilidadesOrquestador.Disparar(mano, EventoPartida.ManoIniciada);
            HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

            if (mano.GanadorMano == null)
                HabilidadesTurnoMaquinaServicio.Notificar(mano);

            if (mano.ManoIniciadaPor == IdJugador.Maquina && !mano.SalpicaduraBloqueando
                && !mano.TravesuraBloqueando && !mano.RasgunoBloqueando
                && !mano.AullidoBloqueando && !mano.DestelloBloqueando
                && !mano.EspejismoBloqueando && !mano.MandingaEspejoBloqueando
                && !mano.MandingaEnganoBloqueando && !mano.MandingaMaldicionBloqueando
                && mano.GanadorMano == null
                && !MaquinaServicio.EsModoHistoriaPasoAPaso(mano))
                MaquinaServicio.ProcesarIniciativa(mano);

            DestelloServicio.EvaluarTurnoHumano(mano);

            PartidaMemoriaServicio.Guardar(mano);
            return mano;
        }

        public ManoTruco EjecutarNuevaPartida(ConfiguracionPartida? configuracion = null)
        {
            var config = configuracion ?? new ConfiguracionPartida();
            var mano = PartidaServicio.CrearManoNueva(configuracion: config);

            HabilidadesOrquestador.Disparar(mano, EventoPartida.PartidaIniciada);
            HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.PartidaIniciada);
            HabilidadesOrquestador.Disparar(mano, EventoPartida.ManoIniciada);
            HabilidadesRivalOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

            if (mano.GanadorMano == null)
                HabilidadesTurnoMaquinaServicio.Notificar(mano);

            if (mano.ManoIniciadaPor == IdJugador.Maquina && !mano.SalpicaduraBloqueando
                && !mano.TravesuraBloqueando && !mano.RasgunoBloqueando
                && !mano.AullidoBloqueando && !mano.DestelloBloqueando
                && !mano.EspejismoBloqueando && !mano.MandingaEspejoBloqueando
                && !mano.MandingaEnganoBloqueando && !mano.MandingaMaldicionBloqueando
                && mano.GanadorMano == null
                && !MaquinaServicio.EsModoHistoriaPasoAPaso(mano))
                MaquinaServicio.ProcesarIniciativa(mano);

            DestelloServicio.EvaluarTurnoHumano(mano);

            PartidaMemoriaServicio.Guardar(mano);
            return mano;
        }

        /// <summary>
        /// Copia el estado de cooldown de la activa (manos desde el último uso y si ya se usó
        /// alguna vez) desde la mano anterior a la nueva, por cada jugador. Sin esto el contador
        /// se reiniciaría en cada mano y la habilidad quedaría siempre disponible.
        /// </summary>
        private static void TrasladarEstadoDestello(ManoTruco? anterior, ManoTruco nueva)
        {
            if (anterior == null) return;
            nueva.ContadorTurnosHumanoPartida = anterior.ContadorTurnosHumanoPartida;
            nueva.DestelloPendiente = anterior.DestelloPendiente;
            nueva.DestelloBazaObjetivo = anterior.DestelloBazaObjetivo;
        }

        private static void TrasladarCooldownHabilidades(ManoTruco? anterior, ManoTruco nueva)
        {
            if (anterior == null) return;

            foreach (var (id, estadoAnterior) in anterior.EstadoHabilidades.PorJugador)
            {
                var estadoNuevo = nueva.EstadoHabilidades.ObtenerOCrear(id, estadoAnterior.ClaseHeroe);
                estadoNuevo.ManosDesdeUltimaActiva = estadoAnterior.ManosDesdeUltimaActiva;
                estadoNuevo.ActivaUsadaAlgunaVez   = estadoAnterior.ActivaUsadaAlgunaVez;
            }
        }

        private static ConfiguracionPartida ClonarConfiguracion(ConfiguracionPartida origen) =>
            new()
            {
                Modo = origen.Modo,
                HeroeDelHumano = origen.HeroeDelHumano,
                RivalDeLaMaquina = origen.RivalDeLaMaquina,
                RivalNivel = origen.RivalNivel
            };
    }
}
