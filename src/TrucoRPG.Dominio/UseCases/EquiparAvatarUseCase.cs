using System;
using System.Threading.Tasks;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.Dominio.UseCases
{
    public class EquiparAvatarUseCase
    {
        private readonly IInventarioRepositorio _inventarioRepositorio;
        private readonly IUsuarioRepositorio _usuarioRepositorio; 

        public EquiparAvatarUseCase(
            IInventarioRepositorio inventarioRepositorio,
            IUsuarioRepositorio usuarioRepositorio)
        {
            _inventarioRepositorio = inventarioRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
        }

        public async Task<bool> Ejecutar(string idUsuario, string spriteKeyNuevo)
        {
            
            bool esDefault = System.Text.RegularExpressions.Regex.IsMatch(spriteKeyNuevo, @"^personaje\d+$");

            if (!esDefault)
            {
                var inventario = await _inventarioRepositorio.ObtenerInventarioDeUsuario(idUsuario);
                bool tieneItem = false;

                foreach (var linea in inventario)
                {
                   
                    if (linea.ItemTienda != null &&
                        !string.IsNullOrEmpty(linea.ItemTienda.SpriteKey) &&
                        spriteKeyNuevo.Contains(linea.ItemTienda.SpriteKey))
                    {
                        tieneItem = true;
                        break;
                    }
                }

                if (!tieneItem)
                {
                    throw new InvalidOperationException("No podés equiparte una prenda que no compraste.");
                }
            }

            var resultado = await _usuarioRepositorio.ActualizarSpriteAsync(idUsuario, spriteKeyNuevo);

            if (!resultado)
            {
                throw new InvalidOperationException("No se pudo actualizar la apariencia del usuario.");
            }

            return true;
        }
    }
}
