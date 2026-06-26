using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    public static class HabilidadesOrquestador
    {
        public static void Disparar(ManoTruco mano, EventoPartida evento, object? datos = null)
        {
            if (!mano.Configuracion.HabilidadesActivas)
            {
                ActualizarVistaHumano(mano);
                return;
            }

            var contexto = new ContextoPartida(mano);
            var heroe = mano.Configuracion.HeroeDelHumano!.Value;

            contexto.EstadoDe(IdJugador.Humano, heroe);
            HeroeHabilidadFactory.Crear(heroe).OnEvento(contexto, evento, datos);

            ActualizarVistaHumano(mano);
        }

        public static ResultadoActivarHabilidad Activar(
            ManoTruco mano,
            SolicitudActivarHabilidad solicitud)
        {
            if (!mano.Configuracion.HabilidadesActivas)
                return ResultadoActivarHabilidad.Error("Las habilidades no están activas en este modo.");

            var contexto = new ContextoPartida(mano);
            var heroe = mano.Configuracion.HeroeDelHumano!.Value;
            var habilidad = HeroeHabilidadFactory.Crear(heroe);
            var resultado = habilidad.IntentarActivar(contexto, solicitud)
                ?? ResultadoActivarHabilidad.Error("No se pudo activar la habilidad.");

            ActualizarVistaHumano(mano);
            return resultado;
        }

        public static string ResolverGanadorEmpateEnvido(ManoTruco mano, string ganadorPorMano)
        {
            var resolucion = new ResolucionEmpateEnvido
            {
                GanadorPorMano = ganadorPorMano,
                GanadorFinal = ganadorPorMano
            };

            if (mano.Configuracion.HabilidadesActivas)
                Disparar(mano, EventoPartida.EmpateEnvido, resolucion);

            return resolucion.GanadorFinal;
        }

        public static void ActualizarVistaHumano(ManoTruco mano) =>
            mano.VistaHabilidadesHumano = HabilidadesConsultaServicio.ObtenerVista(mano, IdJugador.Humano);

        public static void ActualizarVistas(ManoTruco mano) => ActualizarVistaHumano(mano);
    }
}
