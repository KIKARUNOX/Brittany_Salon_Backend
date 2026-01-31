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

            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.ClientId <= 0)
                throw new InvalidOperationException("ClientId inválido.");

            if (dto.StartTime == default)
                throw new InvalidOperationException("StartTime inválido.");

            if (dto.Services is null || dto.Services.Count == 0)
                throw new InvalidOperationException("Debe seleccionar al menos un servicio.");

            // Creo la cita sin total y sin hora final todavía (la calculo con base en los servicios)
            var appointment = new Appointment
            {
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                EndTime = dto.StartTime, // luego lo actualizo
                AppointmentStatus = dto.AppointmentStatus,
                ClientId = dto.ClientId,
                IsActive = true
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            decimal totalCost = 0m;

            // Traigo los servicios del request y calculo costo y duración base
            var serviceIds = dto.Services
                .Select(s => s.ServiceId)
                .Distinct()
                .ToList();

            if (serviceIds.Count == 0)
                throw new InvalidOperationException("Debe seleccionar al menos un servicio.");

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

            var baseDurationMinutes = services.Sum(s => s.DurationMinutes);

            // Aplico costos y tiempo adicional por largo de pelo solo para servicios de tipo "cabello"
            var hairServiceCount = services.Count(s =>
                !string.IsNullOrWhiteSpace(s.ServiceType) &&
                s.ServiceType.Trim().ToLower() == "cabello"
            );

            if (hairServiceCount > 0 && dto.HairLengthOption is null)
                throw new InvalidOperationException("Debe seleccionar el largo del cabello para servicios de tipo 'cabello'.");

            decimal hairCostPerService = dto.HairLengthOption switch
            {
                null => 0m,
                1 => 5000m,
                2 => 10000m,
                3 => 15000m,
                4 => 20000m,
                5 => 25000m,
                6 => 30000m,
                7 => 35000m,
                8 => 40000m,
                9 => 45000m,
                _ => throw new InvalidOperationException("Opción de largo de pelo inválida.")
            };

            var hairExtraMinutesPerService = dto.HairLengthOption is null ? 0 : dto.HairLengthOption.Value * 10;

            totalCost += hairCostPerService * hairServiceCount;

            var totalDurationMinutes = baseDurationMinutes + (hairExtraMinutesPerService * hairServiceCount);

            appointment.EndTime = appointment.StartTime.AddMinutes(totalDurationMinutes);

            // Si llegan productos, sumo el costo y creo la relación
            var productIds = (dto.Products ?? new List<AppointmentProductCreateDto>())
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

            // Actualizo el total final ya con servicios, productos y adicional por largo de pelo
            appointment.TotalCost = totalCost;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return appointment.AppointmentId;
        }
    }
}
