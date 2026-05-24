using TrucoRPG.Dominio.Entities;

namespace TrucoRPG.Dominio.Habilidades
{
    /// <summary>
    /// Lectura del estado de habilidades para mostrar en UI (sin lógica de juego de truco).
    /// </summary>
    public static class HabilidadesConsultaServicio
    {
        public static VistaHabilidadesJugador ObtenerVista(ManoTruco mano, string idJugador)
        {
            var config = mano.Configuracion;
            var vista = new VistaHabilidadesJugador
            {
                HabilidadesActivasEnPartida = config.HabilidadesActivas,
                ClaseHeroe = idJugador == IdJugador.Humano ? config.HeroeDelHumano : null
            };

            if (!config.HabilidadesActivas || idJugador != IdJugador.Humano)
                return vista;

            var estado = mano.EstadoHabilidades.Obtener(idJugador)
                ?? mano.EstadoHabilidades.ObtenerOCrear(idJugador, config.HeroeDelHumano);

            vista.ActivaDisponible = estado.ActivaDisponible;
            vista.ActivaUsadaEnEstaMano = estado.ActivaUsadaEnEstaMano;
            vista.ManosDesdeUltimaActiva = estado.ManosDesdeUltimaActiva;
            vista.UltimoMensajeHabilidad = mano.UltimoMensajeHabilidad ?? estado.ClaseHeroe switch
            {
                ClaseHeroe.Manipulador => "Pasiva: 10% de chance de mejorar cada carta al repartir.",
                ClaseHeroe.Timbero     => "Pasiva: 20% cara (+1 pt) al inicio de cada mano.",
                ClaseHeroe.Fanfarron   => "Pasiva: ganás empates de envido.",
                ClaseHeroe.Mentiroso   => "Pasiva: el rival no ve cuándo podés activar.",
                _ => null
            };
            vista.ModoPartida = config.Modo;
            vista.NombreHeroe = config.HeroeDelHumano.HasValue
                ? NombreHeroe(config.HeroeDelHumano.Value)
                : null;
            vista.SumaValorTrucoMano = mano.Humano.Mano.Count > 0
                ? mano.Humano.Mano.Sum(c => c.ValorTruco)
                : null;
            vista.CartaReveladaRival = estado.CartaReveladaRival;

            return vista;
        }

        private static string NombreHeroe(ClaseHeroe hero) => hero switch
        {
            ClaseHeroe.Manipulador => "Manipulador",
            ClaseHeroe.Timbero     => "Timbero",
            ClaseHeroe.Fanfarron   => "Fanfarrón",
            ClaseHeroe.Mentiroso   => "Mentiroso",
            _ => hero.ToString()
        };
    }
}
