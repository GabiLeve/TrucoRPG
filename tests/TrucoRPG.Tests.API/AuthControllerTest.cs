using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TrucoRPG.API.Controllers;
using TrucoRPG.API.Models;
using TrucoRPG.Dominio.Repositorios;
using TrucoRPG.Dominio.DTOs; 
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

public class AuthControllerTests
{
    private readonly Mock<IRegisterUseCase> _registerUseCaseMock;
    private readonly Mock<ILoginUseCase> _loginUseCaseMock;
    private readonly Mock<ICambiarPasswordUseCase> _cambiarPasswordUseCaseMock;
    private readonly Mock<ISolicitarResetPasswordUseCase> _solicitarResetUseCaseMock;
    private readonly Mock<IResetPasswordUseCase> _resetPasswordUseCaseMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        // GIVEN: Ahora mockeamos interfaces. ¡Mucho más limpio!
        _registerUseCaseMock = new Mock<IRegisterUseCase>();
        _loginUseCaseMock = new Mock<ILoginUseCase>();
        _cambiarPasswordUseCaseMock = new Mock<ICambiarPasswordUseCase>();
        _solicitarResetUseCaseMock = new Mock<ISolicitarResetPasswordUseCase>();
        _resetPasswordUseCaseMock = new Mock<IResetPasswordUseCase>();
        _loggerMock = new Mock<ILogger<AuthController>>();

        _authController = new AuthController(
            _registerUseCaseMock.Object,
            _loginUseCaseMock.Object,
            _cambiarPasswordUseCaseMock.Object,
            _solicitarResetUseCaseMock.Object,
            _resetPasswordUseCaseMock.Object,
            _loggerMock.Object
        );
    }

    //registrar
    [Fact]
    public async Task Registrar_CuandoElRegistroEsExitoso_DebeRetornarOkConTokenDto()
    {
        //Given
        var dto = new RegisterDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "SecurePassword123"
        };
        var tokenEsperado = "token_de_prueba_123";

        _registerUseCaseMock
            .Setup(x => x.EjecutarAsync(dto.UserName, dto.Email, dto.Password))
            .ReturnsAsync(tokenEsperado);

        //When
        var resultado = await _authController.Registrar(dto);

        //Then
        var actionResult = Assert.IsType<ActionResult<TokenDto>>(resultado);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var tokenDto = Assert.IsType<TokenDto>(okResult.Value);

        Assert.Equal(tokenEsperado, tokenDto.Token);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public async Task Registrar_CuandoElCasoDeUsoFalla_DebeLanzarExcepcionOBadRequest()
    {
        // GIVEN
        var dto = new RegisterDto
        {
            UserName = "existente",
            Email = "error@example.com",
            Password = "Password123"
        };
        _registerUseCaseMock
            .Setup(x => x.EjecutarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("El email ya está en uso."));

        // WHEN
       
        Func<Task> accion = () => _authController.Registrar(dto);

        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(accion);
        // THEN
        // Opcional: Podés verificar que el mensaje de la excepción sea el correcto
        Assert.Equal("El email ya está en uso.", excepcion.Message);
    }

    //iniciar sesion
    [Fact]
    public async Task IniciarSesion_CuandoLasCredencialesSonCorrectas_DebeRetornarOkConTokenDto()
    {
        // Given
        var dto = new LoginDto
        {
            Email = "usuario@example.com",
            Password = "PasswordCorrecto123"
        };
        var tokenEsperado = "token_jwt_valido_xyz";

        _loginUseCaseMock
            .Setup(x => x.EjecutarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(tokenEsperado);

        // When
        var resultado = await _authController.IniciarSesion(dto);

        // Then
        var actionResult = Assert.IsType<ActionResult<TokenDto>>(resultado);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var tokenDto = Assert.IsType<TokenDto>(okResult.Value);

        Assert.Equal(tokenEsperado, tokenDto.Token);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public async Task IniciarSesion_CuandoLasCredencialesSonIncorrectas_DebeLanzarExcepcion()
    {
        // Given
        var dto = new LoginDto
        {
            Email = "usuario@example.com",
            Password = "PasswordIncorrecto"
        };
        _loginUseCaseMock
            .Setup(x => x.EjecutarAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new System.UnauthorizedAccessException("Credenciales inválidas."));

        // When    
        Func<Task> accion = () => _authController.IniciarSesion(dto);

        // Then
        var excepcion = await Assert.ThrowsAsync<System.UnauthorizedAccessException>(accion);
        Assert.Equal("Credenciales inválidas.", excepcion.Message);
    }

    //cambiar password
    [Fact]
    public async Task CambiarPassword_CuandoElCambioEsExitoso_DebeRetornarOk()
    {
        // Given
        var dto = new CambiarPasswordDto ( "Vieja123!", "Nueva123!" );
        var userId = "usuario-guid-123";
        var usuarioFicticio = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = usuarioFicticio }
        };

        _cambiarPasswordUseCaseMock
            .Setup(x => x.EjecutarAsync(userId, dto.PasswordActual, dto.PasswordNueva))
            .Returns(Task.CompletedTask); 

        // When
        var resultado = await _authController.CambiarPassword(dto);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public async Task CambiarPassword_CuandoLaContrasenaActualEsIncorrecta_DebeRetornarBadRequest()
    {
        // Given
        var dto = new CambiarPasswordDto ("Incorrecta!", "Nueva123!" );
        var userId = "usuario-guid-123";

        var usuarioFicticio = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = usuarioFicticio }
        };
        _cambiarPasswordUseCaseMock
            .Setup(x => x.EjecutarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("La contraseña actual no coincide."));

        // When
        var resultado = await _authController.CambiarPassword(dto);

        // Then
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CambiarPassword_CuandoNoSeEncuentraElUserIdEnClaims_DebeRetornarUnauthorized()
    {
        // GIVEN
        var dto = new CambiarPasswordDto ("Vieja123!","Nueva123!" );

        // Creamos un contexto con un usuario vacío (sin el Claim de NameIdentifier)
        var usuarioFicticio = new ClaimsPrincipal(new ClaimsIdentity());

        _authController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = usuarioFicticio }
        };

        // WHEN
        var resultado = await _authController.CambiarPassword(dto);

        // THEN
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(resultado);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
    }

    //recuperar contraseña
    [Fact]
    public async Task RecuperarPassword_CuandoElEmailEsValido_DebeRetornarOk()
    {
        // Given
        var dto = new ForgotPasswordDto("test@example.com");

        _solicitarResetUseCaseMock
            .Setup(x => x.EjecutarAsync(dto.Email))
            .Returns(Task.CompletedTask); 

        // When
        var resultado = await _authController.RecuperarPassword(dto);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public async Task RecuperarPassword_CuandoElCasoDeUsoLanzaExcepcion_DebeCapturarlaYRetornarOkIgual()
    {
        // Given
        var dto = new ForgotPasswordDto("error@example.com");

        _solicitarResetUseCaseMock
            .Setup(x => x.EjecutarAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Error simulado de base de datos o SMTP"));

        // When
        var resultado = await _authController.RecuperarPassword(dto);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    //restablecer contraseña
    [Fact]
    public async Task RestablecerPassword_CuandoElTokenYDatosSonValidos_DebeRetornarOk()
    {
        // Given
        var dto = new ResetPasswordDto("usuario@example.com", "token_valido_123", "NuevaPass123!");

        _resetPasswordUseCaseMock
            .Setup(x => x.EjecutarAsync(dto.Email, dto.Token, dto.NuevaPassword))
            .Returns(Task.CompletedTask); 

        // When
        var resultado = await _authController.RestablecerPassword(dto);

        // Then
        var okResult = Assert.IsType<OkObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact]
    public async Task RestablecerPassword_CuandoElTokenExpiroOEsInvalido_DebeRetornarBadRequest()
    {
        // Given
        var dto = new ResetPasswordDto("usuario@example.com", "token_expirado_999", "NuevaPass123!");
        var mensajeError = "El token de restablecimiento es inválido o ha expirado.";

        _resetPasswordUseCaseMock
            .Setup(x => x.EjecutarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException(mensajeError));

        // When
        var resultado = await _authController.RestablecerPassword(dto);

        // Then
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }
}
