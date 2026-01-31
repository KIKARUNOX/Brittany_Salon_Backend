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
            await using var tx = await _db.Database.BeginTransactionAsync();

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

            decimal totalCost = 0;

            // 2) Servicios
            var serviceIds = dto.Services
                .Select(s => s.ServiceId)
                .Distinct()
                .ToList();

            if (serviceIds.Count > 0)
            {
                var services = await _db.Services
                    .Where(s => serviceIds.Contains(s.ServiceId))
                    .ToListAsync();

                if (services.Count != serviceIds.Count)
                    throw new InvalidOperationException("Uno o más servicios no existen.");

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
            }

            // 3) Productos
            var productIds = dto.Products
                .Select(p => p.ProductId)
                .Distinct()
                .ToList();

            if (productIds.Count > 0)
            {
                var products = await _db.Products
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToListAsync();

                if (products.Count != productIds.Count)
                    throw new InvalidOperationException("Uno o más productos no existen.");

                foreach (var product in products)
                {
                    var appointmentProduct = new AppointmentProduct
                    {
                        AppointmentId = appointment.AppointmentId,
                        ProductId = product.ProductId
                    };

                    totalCost += product.Price;
                    _db.AppointmentProducts.Add(appointmentProduct);
                }
            }

            // 4) Actualizar total
            appointment.TotalCost = totalCost;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return appointment.AppointmentId;
        }
    }
}
