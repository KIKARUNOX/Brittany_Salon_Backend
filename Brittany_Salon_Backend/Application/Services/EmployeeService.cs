using Brittany_Salon_Backend.Application.DTOs.Employee;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Application.Validators;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Brittany_Salon_Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Brittany_Salon_Backend.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;
        private readonly IDevLogger _logger;

        public EmployeeService(AppDbContext db, IImageService imageService, IDevLogger logger)
        {
            _db = db;
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los empleados
        /// </summary>
        public async Task<List<EmployeeReadDto>> GetAllAsync(bool onlyActive = false)
        {
            _logger.LogInfo("Obteniendo empleados. Solo activos: {OnlyActive}", onlyActive);

            var query = _db.Employees.AsNoTracking();

            if (onlyActive)
                query = query.Where(e => e.IsActive);

            var employees = await query
                .OrderBy(e => e.Name)
                .Select(e => MapToReadDto(e))
                .ToListAsync();

            _logger.LogInfo("Se encontraron {Count} empleados", employees.Count);
            return employees;
        }

        /// <summary>
        /// Obtiene un empleado por su ID
        /// </summary>
        public async Task<EmployeeReadDto?> GetByIdAsync(int id)
        {
            _logger.LogInfo("Buscando empleado con ID: {Id}", id);

            var employee = await _db.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                _logger.LogWarning("Empleado con ID {Id} no encontrado", id);
                return null;
            }

            _logger.LogInfo("Empleado encontrado: {Name}", employee.Name);
            return MapToReadDto(employee);
        }

        public async Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto dto)
        {
            _logger.LogInfo("Iniciando creacion de empleado: {Email}", dto.Email);

            // Paso 1: Validaciones de formato y reglas de negocio
            var validationErrors = EmployeeValidator.ValidateCreate(dto, _logger);
            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            // Paso 2: Normalizar datos
            var normalizedEmail = dto.Email.Trim().ToLower();
            var normalizedName = NormalizeName(dto.Name);
            var normalizedPhone = dto.Phone.Trim();

            // Paso 3: Validaciones contra base de datos
            await ValidateUniqueConstraintsAsync(normalizedEmail, normalizedPhone);

            // Paso 4: Crear entidad
            var entity = new Employee
            {
                Name = normalizedName,
                Phone = normalizedPhone,
                Email = normalizedEmail,
                Password = dto.Password, // TODO: En produccion, hashear la contrasena
                Specialty = dto.Specialty?.Trim(),
                IsActive = dto.IsActive ?? true,
                DateCreated = DateTime.Now
            };

            _db.Employees.Add(entity);
            await _db.SaveChangesAsync();

            // Paso 5: Procesar imagen si se proporciono
            if (dto.Image != null && dto.Image.Length > 0)
            {
                await ProcessEmployeeImageAsync(entity, dto.Image);
            }

            // Paso 6: Retornar DTO de respuesta
            return MapToReadDto(entity);
        }

        /// <summary>
        /// Valida que no existan duplicados en email y telefono
        /// </summary>
        private async Task ValidateUniqueConstraintsAsync(string email, string phone)
        {
            // Verificar email duplicado
            var emailExists = await _db.Employees.AnyAsync(x => x.Email == email);
            if (emailExists)
                throw new DuplicateResourceException("Email", "Ya existe un empleado registrado con este correo electronico.");

            // Verificar telefono duplicado
            var phoneExists = await _db.Employees.AnyAsync(x => x.Phone == phone);
            if (phoneExists)
                throw new DuplicateResourceException("Phone", "Ya existe un empleado registrado con este numero de telefono.");
        }

        /// <summary>
        /// Normaliza el nombre: capitaliza primera letra de cada palabra
        /// </summary>
        private static string NormalizeName(string name)
        {
            var trimmed = name.Trim();
            var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var normalized = words.Select(word =>
                char.ToUpper(word[0]) + word[1..].ToLower()
            );

            return string.Join(" ", normalized);
        }

        /// <summary>
        /// Procesa y guarda la imagen del empleado (IFormFile)
        /// </summary>
        private async Task ProcessEmployeeImageAsync(Employee entity, IFormFile imageFile)
        {
            try
            {
                string imageUrl = await _imageService.ProcessAndSaveImageAsync(imageFile, "imageUser", entity.Id);
                entity.Image = imageUrl;
                await _db.SaveChangesAsync();
            }
            catch (ArgumentException ex)
            {
                throw new ValidationException($"Error al procesar la imagen: {ex.Message}");
            }
        }

        /// <summary>
        /// Mapea la entidad Employee a DTO de lectura
        /// </summary>
        private static EmployeeReadDto MapToReadDto(Employee entity)
        {
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
