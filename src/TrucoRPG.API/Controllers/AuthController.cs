using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.UseCases;
using TrucoRPG.Dominio.Repositorios;

namespace TrucoRPG.API.Controllers
{
    /// <summary>
    /// Autenticación y gestión de credenciales: registro, login y manejo de contraseñas.
    /// Los endpoints devuelven un JWT que luego se envía en el header
    /// <c>Authorization: Bearer &lt;token&gt;</c> al resto de la API.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IRegisterUseCase               _registerUseCase;
        private readonly ILoginUseCase                  _loginUseCase;
        private readonly ICambiarPasswordUseCase        _cambiarPasswordUseCase;
        private readonly ISolicitarResetPasswordUseCase _solicitarResetUseCase;
        private readonly IResetPasswordUseCase          _resetPasswordUseCase;
        private readonly ILogger<AuthController>       _logger;

        public AuthController(
            IRegisterUseCase registerUseCase,
            ILoginUseCase loginUseCase,
            ICambiarPasswordUseCase cambiarPasswordUseCase,
            ISolicitarResetPasswordUseCase solicitarResetUseCase,
            IResetPasswordUseCase resetPasswordUseCase,
            ILogger<AuthController> logger)
        {
            _registerUseCase        = registerUseCase;
            _loginUseCase           = loginUseCase;
            _cambiarPasswordUseCase = cambiarPasswordUseCase;
            _solicitarResetUseCase  = solicitarResetUseCase;
            _resetPasswordUseCase   = resetPasswordUseCase;
            _logger                 = logger;
        }

        /// <summary>Registra un nuevo usuario y devuelve un JWT.</summary>
        /// <param name="dto">Nombre de usuario, email y contraseña del nuevo jugador.</param>
        /// <response code="200">Usuario creado. Devuelve el token JWT.</response>
        /// <response code="400">Datos inválidos o el usuario/email ya existe (ProblemDetails).</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TokenDto>> Registrar([FromBody] RegisterDto dto)
        {
            var token = await _registerUseCase.EjecutarAsync(dto.UserName, dto.Email, dto.Password);
            return Ok(new TokenDto { Token = token });
        }

        /// <summary>Autentica un usuario existente y devuelve un JWT.</summary>
        /// <param name="dto">Email y contraseña del jugador.</param>
        /// <response code="200">Credenciales válidas. Devuelve el token JWT.</response>
        /// <response code="401">Email o contraseña incorrectos (ProblemDetails).</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokenDto>> IniciarSesion([FromBody] LoginDto dto)
        {
            var token = await _loginUseCase.EjecutarAsync(dto.Email, dto.Password);
            return Ok(new TokenDto { Token = token });
        }

        /// <summary>Cambia la contraseña del usuario autenticado. Requiere JWT.</summary>
        /// <param name="dto">Contraseña actual y la nueva.</param>
        /// <response code="200">Contraseña actualizada.</response>
        /// <response code="400">La contraseña actual es incorrecta o la nueva no cumple los requisitos.</response>
        /// <response code="401">No autenticado (falta o es inválido el JWT).</response>
        [HttpPut("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

                await _cambiarPasswordUseCase.EjecutarAsync(userId, dto.PasswordActual, dto.PasswordNueva);
                return Ok(new { message = "Contraseña actualizada correctamente." });
            
        }

        /// <summary>Envía un email con el link para restablecer la contraseña.</summary>
        /// <param name="dto">Email del usuario que olvidó la contraseña.</param>
        /// <remarks>
        /// Siempre responde 200 (aunque el email no exista) para no revelar qué
        /// emails están registrados.
        /// </remarks>
        /// <response code="200">Solicitud procesada.</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RecuperarPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                await _solicitarResetUseCase.EjecutarAsync(dto.Email);
                _logger.LogInformation("Email de reset enviado a {Email}", dto.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de reset a {Email}", dto.Email);
            }
            return Ok(new { message = "Si ese email está registrado, recibirás un link para restablecer tu contraseña." });
        }

        /// <summary>Restablece la contraseña usando el token del email.</summary>
        /// <param name="dto">Email, token recibido por mail y la nueva contraseña.</param>
        /// <response code="200">Contraseña restablecida.</response>
        /// <response code="400">Token inválido/expirado o contraseña no válida.</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestablecerPassword([FromBody] ResetPasswordDto dto)
        {
                await _resetPasswordUseCase.EjecutarAsync(dto.Email, dto.Token, dto.NuevaPassword);
                return Ok(new { message = "Contraseña restablecida correctamente." });
        }
    }
}
