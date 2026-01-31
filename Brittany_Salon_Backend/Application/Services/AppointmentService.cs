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
        public async Task<List<AppointmentReadDto>> GetAllAsync()
        {
            return await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Client)
                .OrderByDescending(a => a.StartTime)
                .Select(a => new AppointmentReadDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    AppointmentStatus = a.AppointmentStatus,
                    TotalCost = a.TotalCost,
                    IsActive = a.IsActive,
                    ClientId = a.ClientId,
                    ClientName = a.Client.Name
                })
                .ToListAsync();
        }
        public async Task<List<AppointmentReadDto>> GetByDateAsync(DateTime date)
        {
            var onlyDate = date.Date;

            return await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Client)
                .Where(a => a.AppointmentDate == onlyDate)
                .OrderBy(a => a.StartTime)
                .Select(a => new AppointmentReadDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    AppointmentStatus = a.AppointmentStatus,
                    TotalCost = a.TotalCost,
                    IsActive = a.IsActive,
                    ClientId = a.ClientId,
                    ClientName = a.Client.Name
                })
                .ToListAsync();
        }
        public async Task<List<AppointmentReadDto>> GetByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return new List<AppointmentReadDto>();

            var normalized = status.Trim().ToLower();

            return await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Client)
                .Where(a => a.AppointmentStatus != null && a.AppointmentStatus.Trim().ToLower() == normalized)
                .OrderByDescending(a => a.StartTime)
                .Select(a => new AppointmentReadDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    AppointmentStatus = a.AppointmentStatus,
                    TotalCost = a.TotalCost,
                    IsActive = a.IsActive,
                    ClientId = a.ClientId,
                    ClientName = a.Client.Name
                })
                .ToListAsync();
        }
        public async Task<List<AppointmentReadDto>> GetByClientIdAsync(int clientId)
        {
            if (clientId <= 0)
                throw new InvalidOperationException("ClientId inválido.");

            return await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Client)
                .Where(a => a.ClientId == clientId)
                .OrderByDescending(a => a.StartTime)
                .Select(a => new AppointmentReadDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    AppointmentStatus = a.AppointmentStatus,
                    TotalCost = a.TotalCost,
                    IsActive = a.IsActive,
                    ClientId = a.ClientId,
                    ClientName = a.Client.Name
                })
                .ToListAsync();
        }
        public async Task<bool> UpdatePendingAsync(int appointmentId, AppointmentUpdateDto dto)
        {
            if (appointmentId <= 0)
                throw new InvalidOperationException("AppointmentId inválido.");

            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.StartTime == default)
                throw new InvalidOperationException("StartTime inválido.");

            if (dto.Services is null || dto.Services.Count == 0)
                throw new InvalidOperationException("Debe seleccionar al menos un servicio.");

            await using var tx = await _db.Database.BeginTransactionAsync();

            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment is null) return false;

            var currentStatus = (appointment.AppointmentStatus ?? string.Empty).Trim().ToLower();
            if (currentStatus != "pendiente")
                throw new InvalidOperationException("Solo se puede editar una cita con estado Pendiente.");

            // Traigo servicios desde la BD y valido existencia
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

            // Calculo base de costo y duración
            decimal totalCost = 0m;
            var baseDurationMinutes = services.Sum(s => s.DurationMinutes);

            foreach (var s in services)
                totalCost += s.Price;

            // Calculo extras por largo de pelo para servicios de tipo "cabello"
            var hairServiceCount = services.Count(s =>
                !string.IsNullOrWhiteSpace(s.ServiceType) &&
                s.ServiceType.Trim().ToLower() == "cabello"
            );

            // Acepto HairLengthOption = 0 o null como "sin opción" y no aplico extra
            var hairOption = dto.HairLengthOption.GetValueOrDefault(0);

            decimal hairCostPerService = hairOption switch
            {
                0 => 0m,
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

            var hairExtraMinutesPerService = hairOption == 0 ? 0 : hairOption * 10;

            totalCost += hairCostPerService * hairServiceCount;
            var totalDurationMinutes = baseDurationMinutes + (hairExtraMinutesPerService * hairServiceCount);

            // Valido productos y los sumo al costo
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

                foreach (var p in products)
                    totalCost += p.Price;
            }

            // Actualizo tiempos recalculando EndTime desde el backend
            appointment.StartTime = dto.StartTime;
            appointment.EndTime = dto.StartTime.AddMinutes(totalDurationMinutes);
            appointment.AppointmentDate = dto.StartTime.Date;

            appointment.TotalCost = totalCost;

            // Reemplazo relaciones AppointmentService
            var existingApServices = await _db.AppointmentServices
                .Where(x => x.AppointmentId == appointmentId)
                .ToListAsync();

            if (existingApServices.Count > 0)
                _db.AppointmentServices.RemoveRange(existingApServices);

            foreach (var s in services)
            {
                _db.AppointmentServices.Add(new AppointmentServiceEntity
                {
                    AppointmentId = appointmentId,
                    ServiceId = s.ServiceId,
                    ServicePrice = s.Price
                });
            }

            // Reemplazo relaciones AppointmentProduct
            var existingApProducts = await _db.AppointmentProducts
                .Where(x => x.AppointmentId == appointmentId)
                .ToListAsync();

            if (existingApProducts.Count > 0)
                _db.AppointmentProducts.RemoveRange(existingApProducts);

            if (productIds.Count > 0)
            {
                foreach (var pid in productIds)
                {
                    _db.AppointmentProducts.Add(new AppointmentProduct
                    {
                        AppointmentId = appointmentId,
                        ProductId = pid
                    });
                }
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }

        public async Task<AppointmentDetailDto?> GetByIdAsync(int id)
        {
            if (id <= 0) return null;

            var appointment = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Client)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(x => x.Service)
                .Include(a => a.AppointmentProducts)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment is null) return null;

            return new AppointmentDetailDto
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                AppointmentStatus = appointment.AppointmentStatus,
                TotalCost = appointment.TotalCost,
                IsActive = appointment.IsActive,
                ClientId = appointment.ClientId,

                Client = new ClientMiniDto
                {
                    ClientId = appointment.Client.ClientId,
                    Name = appointment.Client.Name,
                    Email = appointment.Client.Email,
                    Phone = appointment.Client.Phone
                },

                Services = appointment.AppointmentServices
                    .Select(s => new AppointmentServiceDetailDto
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.Service.ServiceName,
                        ServicePrice = s.ServicePrice ?? 0m
                    })
                    .ToList(),

                Products = appointment.AppointmentProducts
                    .Select(p => new AppointmentProductDetailDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.Product.ProductName,
                        Price = p.Product.Price
                    })
                    .ToList()
            };
        }
        public async Task<bool> CancelAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new InvalidOperationException("AppointmentId inválido.");

            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment is null) return false;

            var currentStatus = (appointment.AppointmentStatus ?? string.Empty).Trim().ToLower();

            if (currentStatus == "cancelada")
                return true;

            if (currentStatus == "completada" || currentStatus == "finalizada")
                throw new InvalidOperationException("No se puede cancelar una cita completada.");

            appointment.AppointmentStatus = "Cancelada";
            appointment.IsActive = false;

            await _db.SaveChangesAsync();
            return true;
        }


    }
}
