using Brittany_Salon_Backend.Application.DTOs.Employee;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brittany_Salon_Backend.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _db;

        public EmployeeService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto dto)
        {
            // Validación: email repetido
            var emailExists = await _db.Employees.AnyAsync(x => x.Email == dto.Email);
            if (emailExists)
                throw new InvalidOperationException("Ya existe un empleado con ese correo electrónico.");

            var entity = new Employee
            {
                Name = dto.Name.Trim(),
                Phone = dto.Phone,
                Email = dto.Email.Trim(),
                Password = dto.Password,
                Image = dto.Image?.Trim(),
                Specialty = dto.Specialty?.Trim(),
                IsActive = dto.IsActive ?? true,
                DateCreated = DateTime.Now
            };

            _db.Employees.Add(entity);
            await _db.SaveChangesAsync();

            return new EmployeeReadDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Phone = entity.Phone,
                Email = entity.Email,
                Image = entity.Image,
                Specialty = entity.Specialty,
                DateCreated = entity.DateCreated,
                IsActive = entity.IsActive
            };
        }
    }
}
