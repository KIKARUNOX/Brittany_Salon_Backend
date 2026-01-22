using Brittany_Salon_Backend.Application.DTOs.Employee;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Brittany_Salon_Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Brittany_Salon_Backend.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;

        public EmployeeService(AppDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }

        public async Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto dto)
        {
            // Validación: email repetido
            var emailExists = await _db.Employees.AnyAsync(x => x.Email == dto.Email);
            if (emailExists)
                throw new InvalidOperationException("Ya existe un empleado con ese correo electrónico.");

            // Crear entidad sin imagen primero para obtener el ID
            var entity = new Employee
            {
                Name = dto.Name.Trim(),
                Phone = dto.Phone,
                Email = dto.Email.Trim(),
                Password = dto.Password,
                Specialty = dto.Specialty?.Trim(),
                IsActive = dto.IsActive ?? true,
                DateCreated = DateTime.Now
            };

            _db.Employees.Add(entity);
            await _db.SaveChangesAsync();

            // Procesar imagen si se proporcionó
            if (!string.IsNullOrWhiteSpace(dto.ImageBase64))
            {
                try
                {
                    string imageUrl = await _imageService.ProcessAndSaveEmployeeImageAsync(dto.ImageBase64, entity.Id);
                    entity.Image = imageUrl;
                    await _db.SaveChangesAsync();
                }
                catch (ArgumentException ex)
                {
                    // Si falla el procesamiento de imagen, el empleado ya está creado pero sin imagen
                    throw new InvalidOperationException($"Empleado creado pero error al procesar imagen: {ex.Message}");
                }
            }

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
