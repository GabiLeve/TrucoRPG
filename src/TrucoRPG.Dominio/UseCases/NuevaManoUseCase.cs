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

            if (manoAnteriorId.HasValue)
            {
                var anterior = PartidaMemoriaServicio.Obtener(manoAnteriorId.Value)
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

            HabilidadesOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

            if (mano.ManoIniciadaPor == IdJugador.Maquina)
                MaquinaServicio.ProcesarIniciativa(mano);

            PartidaMemoriaServicio.Guardar(mano);
            return mano;
        }

        public ManoTruco EjecutarNuevaPartida(ConfiguracionPartida? configuracion = null)
        {
            var config = configuracion ?? new ConfiguracionPartida();
            var mano = PartidaServicio.CrearManoNueva(configuracion: config);

            HabilidadesOrquestador.Disparar(mano, EventoPartida.PartidaIniciada);
            HabilidadesOrquestador.Disparar(mano, EventoPartida.ManoIniciada);

            if (mano.ManoIniciadaPor == IdJugador.Maquina)
                MaquinaServicio.ProcesarIniciativa(mano);

            PartidaMemoriaServicio.Guardar(mano);
            return mano;
        }

        private static ConfiguracionPartida ClonarConfiguracion(ConfiguracionPartida origen) =>
            new()
            {
                Modo = origen.Modo,
                HeroeDelHumano = origen.HeroeDelHumano
            };
    }
}
