using Brittany_Salon_Backend.Application.DTOs.Appointment;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AppointmentServiceEntity = Brittany_Salon_Backend.Domain.Entities.AppointmentService;


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
            // 1) Crear la cita (sin total todavía)
            var appointment = new Appointment
            {
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                AppointmentStatus = dto.AppointmentStatus,
                ClientId = dto.ClientId,
                IsActive = true
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            // 2) Obtener los IDs de los servicios enviados
            var serviceIds = dto.Services
                .Select(s => s.ServiceId)
                .ToList();

            // 3) Obtener los servicios reales desde BD
            var services = await _db.Services
                .Where(s => serviceIds.Contains(s.ServiceId))
                .ToListAsync();

            if (services.Count != serviceIds.Count)
            {
                throw new InvalidOperationException("Uno o más servicios no existen.");
            }

            // 4) Crear la relación AppointmentService y calcular el total
            decimal totalCost = 0;

            foreach (var service in services)
            {
                var appointmentService = new AppointmentServiceEntity
                {
                    AppointmentId = appointment.AppointmentId,
                    ServiceId = service.ServiceId,
                    ServicePrice = service.Price
                };

                totalCost += service.Price;
                _db.AppointmentServices.Add(appointmentService);
            }

            // 5) Actualizar el total de la cita
            appointment.TotalCost = totalCost;

            await _db.SaveChangesAsync();

            return appointment.AppointmentId;
        }
    }
}
