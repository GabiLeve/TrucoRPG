using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUseCase               _registerUseCase;
        private readonly LoginUseCase                  _loginUseCase;
        private readonly CambiarPasswordUseCase        _cambiarPasswordUseCase;
        private readonly SolicitarResetPasswordUseCase _solicitarResetUseCase;
        private readonly ResetPasswordUseCase          _resetPasswordUseCase;
        private readonly ILogger<AuthController>       _logger;

        public AuthController(
            RegisterUseCase registerUseCase,
            LoginUseCase loginUseCase,
            CambiarPasswordUseCase cambiarPasswordUseCase,
            SolicitarResetPasswordUseCase solicitarResetUseCase,
            ResetPasswordUseCase resetPasswordUseCase,
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
        [HttpPost("register")]
        public async Task<ActionResult<TokenDto>> Register([FromBody] RegisterDto dto)
        {
            var token = await _registerUseCase.EjecutarAsync(dto.UserName, dto.Email, dto.Password);
            return Ok(new TokenDto { Token = token });
        }

        /// <summary>Autentica un usuario existente y devuelve un JWT.</summary>
        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto dto)
        {
            var token = await _loginUseCase.EjecutarAsync(dto.Email, dto.Password);
            return Ok(new TokenDto { Token = token });
        }

        /// <summary>Cambia la contraseña del usuario autenticado.</summary>
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            try
            {
                await _cambiarPasswordUseCase.EjecutarAsync(userId, dto.PasswordActual, dto.PasswordNueva);
                return Ok(new { message = "Contraseña actualizada correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Envía un email con el link para restablecer la contraseña.</summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
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
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _resetPasswordUseCase.EjecutarAsync(dto.Email, dto.Token, dto.NuevaPassword);
                return Ok(new { message = "Contraseña restablecida correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
