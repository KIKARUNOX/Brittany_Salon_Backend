using Brittany_Salon_Backend.Application.DTOs.Authentication;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Brittany_Salon_Backend.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BCrypt.Net;

namespace Brittany_Salon_Backend.Application.Services
{
   //service de auth con jwt
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IDevLogger _logger;
        private readonly JwtSettings _jwtSettings;

        public AuthenticationService(
            AppDbContext db,
            ITokenService tokenService,
            IDevLogger logger,
            IOptions<JwtSettings> jwtSettings)
        {
            _db = db;
            _tokenService = tokenService;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
        }
        // Autentica usuario y genera Access Token + Refresh Token
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            try
            {
                // Validación de entrada
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    _logger?.LogWarning("Login: Email y contraseña son requeridos");
                    throw new ArgumentException("Email y contraseña son requeridos.");
                }

                var email = dto.Email.Trim().ToLower();

                // Intentar autenticar como Employee
                var employee = await _db.Employees
                    .FirstOrDefaultAsync(e => e.Email.ToLower() == email);

                if (employee != null)
                {
                    if (!employee.IsActive)
                    {
                        _logger?.LogWarning($"Login fallido: Empleado {email} inactivo");
                        throw new InvalidOperationException("Este empleado ha sido desactivado.");
                    }

                    if (!BCrypt.Net.BCrypt.Verify(dto.Password, employee.Password))
                    {
                        _logger?.LogWarning($"Login fallido: Contraseña incorrecta para empleado {email}");
                        throw new InvalidOperationException("Credenciales inválidas.");
                    }

                    return await CreateLoginResponse(employee.Id, employee.Email, "EMPLOYEE",
                        employee.Name, employee.Phone, employee.Specialty, employee.Image,
                        employee.IsActive, employee.DateCreated);
                }

                // Intentar autenticar como Client
                var client = await _db.Clients
                    .FirstOrDefaultAsync(c => c.Email.ToLower() == email);

                if (client != null)
                {
                    if (!client.IsActive)
                    {
                        _logger?.LogWarning($"Login fallido: Cliente {email} inactivo");
                        throw new InvalidOperationException("Esta cuenta ha sido desactivada.");
                    }

                    if (!BCrypt.Net.BCrypt.Verify(dto.Password, client.Password))
                    {
                        _logger?.LogWarning($"Login fallido: Contraseña incorrecta para cliente {email}");
                        throw new InvalidOperationException("Credenciales inválidas.");
                    }

                    return await CreateLoginResponse(client.ClientId, client.Email, "CLIENT",
                        client.Name, client.Phone, null, client.ImageUrl,
                        client.IsActive, client.CreatedAt, client.PendingBalance);
                }

                _logger?.LogWarning($"Login: Usuario no encontrado: {email}");
                throw new InvalidOperationException("Email no registrado.");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error en LoginAsync: {ex.Message}");
                throw;
            }
        }

        /// Refresca los tokens usando un Refresh Token válido
        public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            try
            {
                // Validar entrada
                if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                {
                    _logger?.LogWarning("RefreshToken: Token vacío");
                    throw new ArgumentException("El refresh token es requerido.");
                }
                var storedToken = await _db.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

                if (storedToken == null)
                {
                    _logger?.LogWarning($"RefreshToken: Token no encontrado en BD");
                    throw new InvalidOperationException("Refresh token inválido.");
                }
                if (storedToken.IsRevoked)
                {
                    _logger?.LogWarning($"RefreshToken: Token revocado para usuario {storedToken.UserId}");
                    throw new InvalidOperationException("Este refresh token ha sido revocado.");
                }
                if (storedToken.ExpirationDate < DateTime.UtcNow)
                {
                    _logger?.LogWarning($"RefreshToken: Token expirado para usuario {storedToken.UserId}");
                    throw new InvalidOperationException("Refresh token expirado.");
                }
                string email, role;
                if (storedToken.UserType == "EMPLOYEE")
                {
                    var employee = await _db.Employees.FindAsync(storedToken.UserId);
                    if (employee == null || !employee.IsActive)
                    {
                        _logger?.LogWarning($"RefreshToken: Empleado {storedToken.UserId} no encontrado o inactivo");
                        throw new InvalidOperationException("Usuario no encontrado o inactivo.");
                    }
                    email = employee.Email;
                    role = "EMPLOYEE";
                }
                else
                {
                    var client = await _db.Clients.FindAsync(storedToken.UserId);
                    if (client == null || !client.IsActive)
                    {
                        _logger?.LogWarning($"RefreshToken: Cliente {storedToken.UserId} no encontrado o inactivo");
                        throw new InvalidOperationException("Usuario no encontrado o inactivo.");
                    }
                    email = client.Email;
                    role = "CLIENT";
                }

                // Genera nuevos tokens
                var newAccessToken = _tokenService.GenerateAccessToken(storedToken.UserId, email, role);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                var newAccessTokenExpirationDate = DateTime.UtcNow
                    .AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
                var newRefreshTokenExpirationDate = DateTime.UtcNow
                    .AddDays(_jwtSettings.RefreshTokenExpirationDays);

                // Revoca el token anterior
                storedToken.Revoke("Nuevo refresh token generado");

                // Guarda el nuevo refresh token
                var newRefreshTokenEntity = new Domain.Entities.RefreshToken(
                    newRefreshToken,
                    storedToken.UserId,
                    storedToken.UserType,
                    newRefreshTokenExpirationDate);

                _db.RefreshTokens.Add(newRefreshTokenEntity);
                await _db.SaveChangesAsync();

                _logger?.LogInfo($"Tokens refrescados exitosamente para usuario {storedToken.UserId}");

                return new RefreshTokenResponseDto
                {
                    UserId = storedToken.UserId,
                    Role = role,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    AccessTokenExpirationDate = newAccessTokenExpirationDate,
                    RefreshTokenExpirationDate = newRefreshTokenExpirationDate
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error en RefreshTokenAsync: {ex.Message}");
                throw;
            }
        }

        //Revoca un refresh token
        public async Task RevokeRefreshTokenAsync(string refreshToken, string reason = "")
        {
            try
            {
                var storedToken = await _db.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (storedToken != null)
                {
                    storedToken.Revoke(reason);
                    await _db.SaveChangesAsync();
                    _logger?.LogInfo($"Refresh token revocado: {reason}");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error al revocar refresh token: {ex.Message}");
                // No lanzar excepción en revoke, solo log
            }
        }

       
        private async Task<LoginResponseDto> CreateLoginResponse(
            int userId, string email, string role, string name, string phone,
            string? specialty = null, string? imageUrl = null,
            bool isActive = true, DateTime? createdAt = null,
            decimal? pendingBalance = null)
        {
            var accessToken = _tokenService.GenerateAccessToken(userId, email, role);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var accessTokenExpirationDate = DateTime.UtcNow
                .AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
            var refreshTokenExpirationDate = DateTime.UtcNow
                .AddDays(_jwtSettings.RefreshTokenExpirationDays);

            var refreshTokenEntity = new Domain.Entities.RefreshToken(
                refreshToken,
                userId,
                role,
                refreshTokenExpirationDate);

            _db.RefreshTokens.Add(refreshTokenEntity);
            await _db.SaveChangesAsync();

            _logger?.LogInfo($"Login exitoso para usuario {email} con rol {role}");

            return new LoginResponseDto
            {
                UserId = userId,
                Name = name,
                Email = email,
                Phone = phone,
                Role = role,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpirationDate = accessTokenExpirationDate,
                RefreshTokenExpirationDate = refreshTokenExpirationDate,
                Specialty = specialty,
                PendingBalance = pendingBalance,
                ImageUrl = imageUrl,
                IsActive = isActive,
                CreatedAt = createdAt
            };
        }
    }
}
