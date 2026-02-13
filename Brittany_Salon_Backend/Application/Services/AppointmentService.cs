using Brittany_Salon_Backend.Application.DTOs.Appointment;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Application.Validators;
using Brittany_Salon_Backend.Domain.Constants;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AppointmentServiceEntity = Brittany_Salon_Backend.Domain.Entities.AppointmentService;

namespace Brittany_Salon_Backend.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AppDbContext _db;
        private readonly IDevLogger _logger;

        public AppointmentService(AppDbContext db, IDevLogger logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> CreateAsync(AppointmentCreateDto dto)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            var validationErrors = AppointmentValidator.ValidateCreate(dto, _logger);
            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            var appointment = new Appointment
            {
                AppointmentDate = dto.AppointmentDate,
                StartTime = dto.StartTime,
                EndTime = dto.StartTime,
                AppointmentStatus = dto.AppointmentStatus,
                ClientId = dto.ClientId,
                IsActive = true
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            decimal totalCost = 0m;

            var serviceIds = dto.Services
                .Select(s => s.ServiceId)
                .Distinct()
                .ToList();

            var services = await _db.Services
                .Where(s => serviceIds.Contains(s.ServiceId))
                .ToListAsync();

            if (services.Count != serviceIds.Count)
                throw new InvalidOperationException("Uno o más servicios no existen.");

            foreach (var service in services)
            {
                _db.AppointmentServices.Add(new AppointmentServiceEntity
                {
                    AppointmentId = appointment.AppointmentId,
                    ServiceId = service.ServiceId,
                    ServicePrice = service.Price
                });

                totalCost += service.Price;
            }

            var baseDurationMinutes = services.Sum(s => s.DurationMinutes);

            var hairServiceCount = services.Count(s =>
                !string.IsNullOrWhiteSpace(s.ServiceType) &&
                s.ServiceType.Trim().ToLower() == "cabello"
            );

            var hairOption = dto.HairLengthOption.GetValueOrDefault(0);

            if (hairServiceCount > 0 && hairOption == 0)
                throw new InvalidOperationException("Debe seleccionar el largo del cabello para servicios de tipo 'cabello'.");

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

            appointment.EndTime = appointment.StartTime.AddMinutes(totalDurationMinutes);

            var availability = await ValidateAvailabilityAsync(new AppointmentAvailabilityRequestDto
            {
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                ServiceIds = serviceIds
            });

            if (!availability.IsAvailable)
                throw new InvalidOperationException(availability.Message);

            if (dto.Products != null && dto.Products.Count > 0)
            {
                // Obtain unique IDs to validate existence
                var uniqueProductIds = dto.Products
                    .Select(p => p.ProductId)
                    .Distinct()
                    .ToList();

                var products = await _db.Products
                    .Where(p => uniqueProductIds.Contains(p.ProductId))
                    .ToDictionaryAsync(p => p.ProductId, p => p);

                if (products.Count != uniqueProductIds.Count)
                    throw new InvalidOperationException("Uno o más productos no existen.");

                // Add products respecting quantities
                foreach (var productDto in dto.Products)
                {
                    if (!products.ContainsKey(productDto.ProductId))
                        continue;

                    var product = products[productDto.ProductId];
                    var quantity = productDto.Quantity > 0 ? productDto.Quantity : 1;

                    for (int i = 0; i < quantity; i++)
                    {
                        _db.AppointmentProducts.Add(new AppointmentProduct
                        {
                            AppointmentId = appointment.AppointmentId,
                            ProductId = product.ProductId
                        });

                        totalCost += product.Price;
                    }
                }
            }

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

            var validationErrors = AppointmentValidator.ValidateUpdate(dto, _logger);
            if (validationErrors.Count > 0)
                throw new ValidationException(validationErrors);

            await using var tx = await _db.Database.BeginTransactionAsync();

            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment is null) return false;

            if (!AppointmentStatuses.CanBeEdited(appointment.AppointmentStatus))
                throw new InvalidOperationException("Solo se puede editar una cita con estado Pendiente.");

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

            decimal totalCost = 0m;
            var baseDurationMinutes = services.Sum(s => s.DurationMinutes);

            foreach (var s in services)
                totalCost += s.Price;

            var hairServiceCount = services.Count(s =>
                !string.IsNullOrWhiteSpace(s.ServiceType) &&
                s.ServiceType.Trim().ToLower() == "cabello"
            );

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

            if (dto.Products != null && dto.Products.Count > 0)
            {
                // Obtain unique IDs to validate existence
                var uniqueProductIds = dto.Products
                    .Select(p => p.ProductId)
                    .Distinct()
                    .ToList();

                var products = await _db.Products
                    .Where(p => uniqueProductIds.Contains(p.ProductId))
                    .ToDictionaryAsync(p => p.ProductId, p => p);

                if (products.Count != uniqueProductIds.Count)
                    throw new InvalidOperationException("Uno o más productos no existen.");

                // Calculate cost respecting quantities
                foreach (var productDto in dto.Products)
                {
                    if (!products.ContainsKey(productDto.ProductId))
                        continue;

                    var product = products[productDto.ProductId];
                    var quantity = productDto.Quantity > 0 ? productDto.Quantity : 1;
                    totalCost += product.Price * quantity;
                }
            }

            var newStart = dto.StartTime;
            var newEnd = dto.StartTime.AddMinutes(totalDurationMinutes);

            var availability = await ValidateAvailabilityAsync(new AppointmentAvailabilityRequestDto
            {
                StartTime = newStart,
                EndTime = newEnd,
                ServiceIds = serviceIds,
                ExcludeAppointmentId = appointmentId
            });

            if (!availability.IsAvailable)
                throw new InvalidOperationException(availability.Message);

            appointment.StartTime = newStart;
            appointment.EndTime = newEnd;
            appointment.AppointmentDate = newStart.Date;
            appointment.TotalCost = totalCost;

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

            var existingApProducts = await _db.AppointmentProducts
                .Where(x => x.AppointmentId == appointmentId)
                .ToListAsync();

            if (existingApProducts.Count > 0)
                _db.AppointmentProducts.RemoveRange(existingApProducts);

            if (dto.Products != null && dto.Products.Count > 0)
            {
                // Obtain product dictionary again for adding
                var uniqueProductIds = dto.Products
                    .Select(p => p.ProductId)
                    .Distinct()
                    .ToList();

                var products = await _db.Products
                    .Where(p => uniqueProductIds.Contains(p.ProductId))
                    .ToDictionaryAsync(p => p.ProductId, p => p);

                // Add products respecting quantities
                foreach (var productDto in dto.Products)
                {
                    if (!products.ContainsKey(productDto.ProductId))
                        continue;

                    var quantity = productDto.Quantity > 0 ? productDto.Quantity : 1;

                    for (int i = 0; i < quantity; i++)
                    {
                        _db.AppointmentProducts.Add(new AppointmentProduct
                        {
                            AppointmentId = appointmentId,
                            ProductId = productDto.ProductId
                        });
                    }
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

            if (string.Equals(appointment.AppointmentStatus, AppointmentStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
                return true;

            if (AppointmentStatuses.IsFinalState(appointment.AppointmentStatus))
                throw new InvalidOperationException("No se puede cancelar una cita finalizada.");

            if (!AppointmentStatuses.CanBeCancelled(appointment.AppointmentStatus))
                throw new InvalidOperationException("No se puede cancelar una cita en este estado.");

            appointment.AppointmentStatus = AppointmentStatuses.Cancelled;
            appointment.IsActive = false;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new InvalidOperationException("AppointmentId inválido.");

            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment is null) return false;

            if (string.Equals(appointment.AppointmentStatus, AppointmentStatuses.Finalized, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(appointment.AppointmentStatus, AppointmentStatuses.Cancelled, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("No se puede completar una cita cancelada.");

            // Calcular saldo pendiente para determinar el estado final
            var totalPaid = await _db.Payments
                .Where(p => p.AppointmentId == appointmentId && p.IsActive)
                .SumAsync(p => p.Amount);

            var pendingBalance = (appointment.TotalCost ?? 0) - totalPaid;

            appointment.AppointmentStatus = pendingBalance > 0
                ? AppointmentStatuses.CompletedPendingPayment
                : AppointmentStatuses.Finalized;
            appointment.IsActive = false;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<AppointmentAvailabilityResponseDto> ValidateAvailabilityAsync(AppointmentAvailabilityRequestDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.StartTime == default || dto.EndTime == default)
                return new AppointmentAvailabilityResponseDto { IsAvailable = false, Message = "Fecha/hora inválidas." };

            if (dto.EndTime <= dto.StartTime)
                return new AppointmentAvailabilityResponseDto { IsAvailable = false, Message = "La hora de fin debe ser mayor a la hora de inicio." };

            if (dto.ServiceIds is null || dto.ServiceIds.Count == 0)
                return new AppointmentAvailabilityResponseDto { IsAvailable = false, Message = "Debe enviar al menos un servicio." };

            if (!IsWithinBusinessHours(dto.StartTime, dto.EndTime, out var hoursMsg))
                return new AppointmentAvailabilityResponseDto { IsAvailable = false, Message = hoursMsg };

            var serviceIds = dto.ServiceIds.Distinct().ToList();

            var conflict = await HasServiceConflictAsync(dto.StartTime, dto.EndTime, serviceIds, dto.ExcludeAppointmentId);
            if (conflict)
                return new AppointmentAvailabilityResponseDto { IsAvailable = false, Message = "El horario choca con otra cita que incluye uno o más de los mismos servicios." };

            return new AppointmentAvailabilityResponseDto { IsAvailable = true, Message = "Horario disponible." };
        }

        private bool IsWithinBusinessHours(DateTime start, DateTime end, out string message)
        {
            message = string.Empty;

            if (start.Date != end.Date)
            {
                message = "La cita debe iniciar y finalizar el mismo día.";
                return false;
            }

            var day = start.DayOfWeek;

            if (day == DayOfWeek.Sunday)
            {
                message = "Domingo: cerrado.";
                return false;
            }

            var open = new TimeSpan(9, 0, 0);
            var close = day == DayOfWeek.Saturday
                ? new TimeSpan(18, 0, 0)
                : new TimeSpan(20, 0, 0);

            var startTod = start.TimeOfDay;
            var endTod = end.TimeOfDay;

            if (startTod < open || endTod > close)
            {
                message = day == DayOfWeek.Saturday
                    ? "Sábado: horario permitido de 9:00 AM a 6:00 PM."
                    : "Lunes a Viernes: horario permitido de 9:00 AM a 8:00 PM.";
                return false;
            }

            return true;
        }

        private async Task<bool> HasServiceConflictAsync(
            DateTime start,
            DateTime end,
            List<int> serviceIds,
            int? excludeAppointmentId = null)
        {
            var cancelledStatus = AppointmentStatuses.Cancelled.ToLower();

            var query = _db.Appointments
                .AsNoTracking()
<<<<<<< HEAD
                .Where(a => a.StartTime < end && a.EndTime > start);

            query = query.Where(a =>
                a.AppointmentStatus == null || 
                a.AppointmentStatus.ToLower() != cancelledStatus
            );
=======
                .Where(a => a.StartTime < end && a.EndTime > start)
                .Where(a => a.AppointmentStatus == null || 
                            a.AppointmentStatus.ToLower() != cancelledStatus);
>>>>>>> 326b806600428510bdd4b6ef5d4a55d61e33dc74

            if (excludeAppointmentId.HasValue)
                query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);

            return await query
                .Join(_db.AppointmentServices.AsNoTracking(),
                      a => a.AppointmentId,
                      aps => aps.AppointmentId,
                      (a, aps) => new { a, aps })
                .AnyAsync(x => serviceIds.Contains(x.aps.ServiceId));
        }

        public async Task<decimal> GetPendingBalanceAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new InvalidOperationException("AppointmentId inválido.");

            var appointment = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Payments)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment is null)
                throw new InvalidOperationException("Cita no encontrada.");

            var totalPaid = appointment.Payments
                .Where(p => p.IsActive)
                .Sum(p => p.Amount);

            return (appointment.TotalCost ?? 0) - totalPaid;
        }

        public async Task<decimal> GetPendingBalanceClientAsync(int appointmentId)
        {
            if (appointmentId <= 0)
                throw new InvalidOperationException("AppointmentId inválido.");

            var appointment = await _db.Appointments
                .AsNoTracking()
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment is null)
                throw new InvalidOperationException("Cita no encontrada.");

            return appointment.Client.PendingBalance;
        }
    }
}
