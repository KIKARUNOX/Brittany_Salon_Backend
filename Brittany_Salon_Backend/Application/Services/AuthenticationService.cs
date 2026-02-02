using Brittany_Salon_Backend.Application.DTOs.Authentication;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brittany_Salon_Backend.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _db;

        public AuthenticationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Email y contraseña son requeridos.");

            var email = dto.Email.Trim().ToLower();

            //Es empleado?
            var employee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Email.ToLower() == email);

            if (employee != null)
            {
                if (employee.Password == dto.Password)
                {
                    return new LoginResponseDto
                    {
                        UserId = employee.Id,
                        Name = employee.Name,
                        Email = employee.Email,
                        Phone = employee.Phone,
                        Role = "EMPLOYEE",
                        Token = string.Empty,
                        Specialty = employee.Specialty,
                        ImageUrl = employee.Image,
                        IsActive = employee.IsActive,
                        CreatedAt = employee.DateCreated
                    };
                }
                else
                {
                    throw new InvalidOperationException("Contraseña incorrecta.");
                }
            }

            //Es cliente?
            var client = await _db.Clients
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email);

            if (client != null)
            {
                if (client.Password == dto.Password)
                {
                    return new LoginResponseDto
                    {
                        UserId = client.ClientId,
                        Name = client.Name,
                        Email = client.Email,
                        Phone = client.Phone,
                        Role = "CLIENT",
                        Token = string.Empty,
                        PendingBalance = client.PendingBalance,
                        ImageUrl = client.ImageUrl,
                        IsActive = client.IsActive,
                        CreatedAt = client.CreatedAt
                    };
                }
                else
                {
                    throw new InvalidOperationException("Contraseña incorrecta.");
                }
            }

            throw new InvalidOperationException("Cliente no encontrado.");
        }
    }
}