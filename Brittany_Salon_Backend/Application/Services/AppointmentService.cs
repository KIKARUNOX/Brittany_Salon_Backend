using Brittany_Salon_Backend.Application.DTOs.Appointment;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brittany_Salon_Backend.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _db;

        public AppointmentService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> CreateAsync(AppointmentCreateDto dto)
        {
            var entity = new Appointment
            {
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                AppointmentStatus = dto.AppointmentStatus,
                TotalCost = dto.TotalCost,
                ClientId = dto.ClientId,
                IsActive = true
            };

            _db.Appointments.Add(entity);
            await _db.SaveChangesAsync();

            return entity.AppointmentId;
        }
    }
}
