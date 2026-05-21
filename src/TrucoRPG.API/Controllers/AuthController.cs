using Microsoft.AspNetCore.Mvc;
using TrucoRPG.API.Models;
using TrucoRPG.Logica.UseCases;

namespace TrucoRPG.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUseCase _registerUseCase;
        private readonly LoginUseCase    _loginUseCase;

        public AuthController(RegisterUseCase registerUseCase, LoginUseCase loginUseCase)
        {
            _registerUseCase = registerUseCase;
            _loginUseCase    = loginUseCase;
        }

        /// <summary>Registra un nuevo usuario y devuelve un JWT.</summary>
        [HttpPost("register")]
        public async Task<ActionResult<TokenDto>> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var token = await _registerUseCase.EjecutarAsync(dto.UserName, dto.Email, dto.Password);
                return Ok(new TokenDto { Token = token });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>Autentica un usuario existente y devuelve un JWT.</summary>
        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _loginUseCase.EjecutarAsync(dto.Email, dto.Password);
                return Ok(new TokenDto { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }
    }
}
