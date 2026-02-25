using Brittany_Salon_Backend.Application.DTOs.Authentication;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login de usuario (Employee o Client)
        /// </summary>
        /// <param name="dto">Email y contraseña del usuario</param>
        /// <returns>Tokens JWT y datos del usuario</returns>
        /// <response code="200">Login exitoso, retorna AccessToken y RefreshToken</response>
        /// <response code="401">Credenciales inválidas</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Message = "Datos de entrada inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return Unauthorized(new ErrorResponse { Message = "Error interno en el servidor" });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ErrorResponse { Message = "Error interno en el servidor" });
            }
            catch
            {
                return StatusCode(500, new ErrorResponse { Message = "Error interno en el servidor" });
            }
        }

        /// <summary>
        /// Refresca el Access Token usando un Refresh Token válido
        /// </summary>
        /// <param name="dto">Contiene el Refresh Token</param>
        /// <returns>Nuevos tokens JWT</returns>
        /// <response code="200">Refresh exitoso, retorna nuevos tokens</response>
        /// <response code="401">Refresh token inválido o expirado</response>
        /// <response code="400">Datos inválidos</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RefreshTokenResponseDto>> Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Message = "Datos de entrada inválidos",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.RefreshTokenAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return Unauthorized(new ErrorResponse { Message = "Error interno en el servidor" });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ErrorResponse { Message = "Error interno en el servidor" });
            }
            catch
            {
                return StatusCode(500, new ErrorResponse { Message = "Error interno en el servidor" });
            }
        }

        /// <summary>
        /// Revoca la sesión actual (logout)
        /// </summary>
        /// <param name="dto">Contiene el Refresh Token a revocar</param>
        /// <returns>Confirmación de logout</returns>
        /// <response code="200">Logout exitoso</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                await _authService.RevokeRefreshTokenAsync(dto.RefreshToken, "Logout del usuario");
                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch
            {
                return StatusCode(500, new ErrorResponse { Message = "Error al cerrar sesión" });
            }
        }
    }

    /// <summary>
    /// Respuesta de error estándar
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Field { get; set; }
        public List<string>? Errors { get; set; }
    }
}
