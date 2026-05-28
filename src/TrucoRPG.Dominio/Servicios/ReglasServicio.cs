using TrucoRPG.Dominio.Entities; 

namespace TrucoRPG.Dominio.Servicios
{
    public static class ReglasServicio
    {
        public static List<CategoriaRegla> ObtenerReglasGenerales()
        {
            return new List<CategoriaRegla>
            {
                new CategoriaRegla
                {
                    Categoria = "El Truco",
                    Detalle = new List<ReglasDetalle>
                    {
                        new ReglasDetalle { Titulo = "Truco", Descripcion = "El canto básico. El que gana 2 de 3 rondas se lleva los puntos.", Puntos = 2 },
                        new ReglasDetalle { Titulo = "Retruco", Descripcion = "Se puede cantar arriba del Truco. Eleva la apuesta.", Puntos = 3 },
                        new ReglasDetalle { Titulo = "Vale Cuatro", Descripcion = "El grito máximo del juego. Solo para valientes o mentirosos.", Puntos = 4 }
                    }
                },
                new CategoriaRegla
                {
                    Categoria = "El Envido",
                    Detalle = new List<ReglasDetalle>
                    {
                        new ReglasDetalle { Titulo = "Envido", Descripcion = "Se cantan los puntos de dos cartas del mismo palo (palo + valor + 20).", Puntos = 2 },
                        new ReglasDetalle { Titulo = "Real Envido", Descripcion = "Un aumento directo al envido inicial.", Puntos = 3 },
                        new ReglasDetalle { Titulo = "Falta Envido", Descripcion = "Si están en las malas (parada de abajo), da los puntos que le faltan para ganar al que va ganando.", Puntos = 0 }
                    }
                },
                new CategoriaRegla
                {
                    Categoria = "Estrategia General",
                    Detalle = new List<ReglasDetalle>
                    {
                        new ReglasDetalle { Titulo = "La Primera", Descripcion = "Ganar la primera mano es clave. Si hay empate (parda) en las siguientes, gana el que metió la primera.", Puntos = 0 },
                        new ReglasDetalle { Titulo = "Ir al mazo", Descripcion = "Si la mano viene horrible, podés irte al mazo para regalarle solo los puntos mínimos al rival y pasar a la siguiente.", Puntos = 0 }
                    }
                }
            };
        }
    }
}
