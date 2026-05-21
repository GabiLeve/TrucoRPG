using TrucoDemo.Clases;

namespace TrucoDemo.Servicios
{
    public static class JuegoServicio
    {
        public static string ResolverBaza(Carta cartaHumano, Carta cartaMaquina)
        {
            if (cartaHumano.ValorTruco > cartaMaquina.ValorTruco)
                return "Humano";

            if (cartaMaquina.ValorTruco > cartaHumano.ValorTruco)
                return "Maquina";

            return "Parda";
        }

        public static string? ResolverGanadorMano(List<Baza> bazas, string jugadorMano)
        {
            if (bazas.Count == 0)
                return null;

            string? ganadorPrimera = ObtenerGanadorBaza(bazas, 0);
            string? ganadorSegunda = ObtenerGanadorBaza(bazas, 1);
            string? ganadorTercera = ObtenerGanadorBaza(bazas, 2);

            if (ganadorPrimera is "Humano" or "Maquina")
            {
                if (ganadorSegunda == ganadorPrimera)
                    return ganadorPrimera;

                if (ganadorSegunda == "Parda")
                    return ganadorPrimera;
            }

            if (ganadorPrimera == "Parda")
            {
                if (ganadorSegunda is "Humano" or "Maquina")
                {
                    // Parda en primera → gana quien gana la segunda (la carta más alta decide)
                    return ganadorSegunda;
                }

                if (ganadorSegunda == "Parda")
                {
                    // Parda en primera y segunda → va a la tercera
                    if (ganadorTercera == null)
                        return null;

                    if (ganadorTercera == "Parda")
                        return jugadorMano; // Todas pardas → gana el jugador "mano"

                    return ganadorTercera;
                }
            }

            if (ganadorPrimera is "Humano" or "Maquina")
            {
                if (ganadorSegunda is "Humano" or "Maquina" && ganadorSegunda != ganadorPrimera)
                {
                    if (ganadorTercera == null)
                        return null;

                    if (ganadorTercera == "Parda")
                        return ganadorPrimera;

                    return ganadorTercera;
                }
            }

            return null;
        }

        private static string? ObtenerGanadorBaza(List<Baza> bazas, int index)
        {
            return bazas.Count > index ? bazas[index].Ganador : null;
        }
    }
}
