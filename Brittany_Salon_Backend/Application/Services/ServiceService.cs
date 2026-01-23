using Brittany_Salon_Backend.Application.DTOs.Service;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;

namespace Brittany_Salon_Backend.Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly AppDbContext _db;

        public ServiceService(AppDbContext db)
        {
            _db = db;
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
            // Validación: nombre repetido (opcional pero recomendable)
            var nameExists = await _db.Services.AnyAsync(x => x.ServiceName == dto.ServiceName);
            if (nameExists)
                throw new InvalidOperationException("Ya existe un servicio con ese nombre.");

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

        public async Task<bool> UpdateAsync(int id, ServiceUpdateDto dto)
        {
            var entity = await _db.Services.FirstOrDefaultAsync(x => x.ServiceId == id);
            if (entity is null) return false;

            var nameTaken = await _db.Services.AnyAsync(x => x.ServiceName == dto.ServiceName && x.ServiceId != id);
            if (nameTaken)
                throw new InvalidOperationException("Ya existe otro servicio con ese nombre.");

            entity.ServiceName = dto.ServiceName.Trim();
            entity.ServiceDescription = dto.ServiceDescription?.Trim();
            entity.Price = dto.Price;
            entity.DurationMinutes = dto.DurationMinutes;
            entity.ServiceType = dto.ServiceType?.Trim();
            entity.IsActive = dto.IsActive;

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


    }
}
