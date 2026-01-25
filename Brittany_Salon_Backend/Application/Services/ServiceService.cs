using Brittany_Salon_Backend.Application.DTOs.Service;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Brittany_Salon_Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using Brittany_Salon_Backend.Application.Validators;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Infrastructure.Logging;

namespace Brittany_Salon_Backend.Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;
        private readonly IDevLogger _logger;

        public ServiceService(AppDbContext db, IImageService imageService, IDevLogger logger)
        {
            _db = db;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<List<ServiceReadDto>> GetAllAsync(bool onlyActive = false)
        {
            var query = _db.Services.AsNoTracking();

            if (onlyActive)
                query = query.Where(s => s.IsActive);

            return await query
                .OrderBy(s => s.ServiceName)
                .Select(s => new ServiceReadDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    ServiceDescription = s.ServiceDescription,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    ImageUrl = s.ImageUrl,
                    ServiceType = s.ServiceType,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }

        public async Task<ServiceReadDto?> GetByIdAsync(int id)
        {
            var s = await _db.Services.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ServiceId == id);

            if (s is null) return null;

            return new ServiceReadDto
            {
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                ServiceDescription = s.ServiceDescription,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes,
                ImageUrl = s.ImageUrl,
                ServiceType = s.ServiceType,
                IsActive = s.IsActive
            };
        }

        public async Task<ServiceReadDto> CreateAsync(ServiceCreateDto dto)
        {
            var validationErrors = ServiceValidator.ValidateCreate(dto, _logger);
            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            // 2) Crear entidad (sin imagen todavía)
            var entity = new Service
            {
                ServiceName = dto.ServiceName.Trim(),
                ServiceDescription = dto.ServiceDescription?.Trim(),
                Price = dto.Price,
                DurationMinutes = dto.DurationMinutes,
                ImageUrl = null,
                ServiceType = dto.ServiceType?.Trim(),
                IsActive = dto.IsActive ?? true
            };

            _db.Services.Add(entity);
            await _db.SaveChangesAsync();

            // 3) Procesar imagen si viene
            if (dto.Image != null && dto.Image.Length > 0)
            {
                await ProcessServiceImageAsync(entity, dto.Image);
            }

            // 4) Retornar
            return MapToReadDto(entity);
        }

        public async Task<bool> UpdateAsync(int id, ServiceUpdateDto dto)
        {
            // 1) Buscar
            var entity = await _db.Services.FindAsync(id);
            if (entity is null) return false;

            var validationErrors = ServiceValidator.ValidateUpdate(dto, _logger);
            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);


            // 3) Actualizar campos
            entity.ServiceName = dto.ServiceName.Trim();
            entity.ServiceDescription = dto.ServiceDescription?.Trim();
            entity.Price = dto.Price;
            entity.DurationMinutes = dto.DurationMinutes;
            entity.ServiceType = dto.ServiceType?.Trim();
            entity.IsActive = dto.IsActive;

            // 4) Imagen (si viene nueva)
            if (dto.Image != null && dto.Image.Length > 0)
            {
                await UpdateServiceImageAsync(entity, dto.Image);
            }

            // 5) Guardar todo
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var entity = await _db.Services.FirstOrDefaultAsync(x => x.ServiceId == id);
            if (entity is null) return false;

            entity.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateImageUrlAsync(int serviceId, string imageUrl)
        {
            var service = await _db.Services.FindAsync(serviceId);
            if (service == null) return false;

            service.ImageUrl = imageUrl.Trim();
            await _db.SaveChangesAsync();
            return true;
        }

        private static ServiceReadDto MapToReadDto(Service entity)
        {
            return new ServiceReadDto
            {
                ServiceId = entity.ServiceId,
                ServiceName = entity.ServiceName,
                ServiceDescription = entity.ServiceDescription,
                Price = entity.Price,
                DurationMinutes = entity.DurationMinutes,
                ImageUrl = entity.ImageUrl,
                ServiceType = entity.ServiceType,
                IsActive = entity.IsActive
            };
        }

        /// <summary>
        /// Procesa y guarda la imagen del servicio (CREATE)
        /// </summary>
        private async Task ProcessServiceImageAsync(Service entity, IFormFile imageFile)
        {
            string imageUrl = await _imageService.ProcessAndSaveImageAsync(imageFile, "imageService", entity.ServiceId);
            entity.ImageUrl = imageUrl;
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Actualiza la imagen del servicio (UPDATE) eliminando la anterior si existe
        /// </summary>
        private async Task UpdateServiceImageAsync(Service entity, IFormFile newImage)
        {
            // eliminar anterior si existe
            if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
            {
                _imageService.DeleteImage(entity.ImageUrl);
            }

            // guardar nueva
            string imageUrl = await _imageService.ProcessAndSaveImageAsync(newImage, "imageService", entity.ServiceId);
            entity.ImageUrl = imageUrl;
        }
        public async Task<List<ServiceReadDto>> SearchByNameAsync(string name, bool onlyActive = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<ServiceReadDto>();

            var searchTerm = name.Trim().ToLower();

            var query = _db.Services.AsNoTracking();

            if (onlyActive)
                query = query.Where(s => s.IsActive);

            return await query
                .Where(s => s.ServiceName.ToLower().Contains(searchTerm))
                .OrderBy(s => s.ServiceName)
                .Select(s => new ServiceReadDto
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    ServiceDescription = s.ServiceDescription,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    ImageUrl = s.ImageUrl,
                    ServiceType = s.ServiceType,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }
        public async Task<bool> ReactivateAsync(int id)
        {
            var entity = await _db.Services.FindAsync(id);
            if (entity == null) return false;

            if (entity.IsActive) return true;

            entity.IsActive = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePermanentlyAsync(int id)
        {
            var entity = await _db.Services.FindAsync(id);
            if (entity == null) return false;

            if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
            {
                _imageService.DeleteImage(entity.ImageUrl);
            }

            _db.Services.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }


    }
}
