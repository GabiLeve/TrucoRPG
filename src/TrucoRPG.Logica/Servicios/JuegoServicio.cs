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
                    // Primera parda → quien gana la segunda gana la mano, sin importar la tercera
                    return ganadorSegunda;
                }

                if (ganadorSegunda == "Parda")
                {
                    if (ganadorTercera == null)
                        return null;

                    if (ganadorTercera == "Parda")
                        return jugadorMano;

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
