using Brittany_Salon_Backend.Application.DTOs.Payment;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brittany_Salon_Backend.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly IDevLogger _logger;

        public PaymentService(AppDbContext db, IDevLogger logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<PaymentReadDto>> GetAllAsync()
        {
            _logger.LogInfo("Obteniendo todos los pagos");

            var payments = await _db.Payments
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => MapToReadDto(p))
                .ToListAsync();

            _logger.LogInfo("Se encontraron {Count} pagos", payments.Count);
            return payments;
        }

        public async Task<PaymentReadDto?> GetByIdAsync(int id)
        {
            _logger.LogInfo("Buscando pago con ID: {Id}", id);

            var payment = await _db.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentId == id && p.IsActive);

            if (payment == null)
            {
                _logger.LogWarning("Pago con ID {Id} no encontrado", id);
                return null;
            }

            _logger.LogInfo("Pago encontrado");
            return MapToReadDto(payment);
        }

        public async Task<List<PaymentReadDto>> GetByAppointmentIdAsync(int appointmentId)
        {
            _logger.LogInfo("Buscando pagos para cita ID: {AppointmentId}", appointmentId);

            var payments = await _db.Payments
                .AsNoTracking()
                .Where(p => p.AppointmentId == appointmentId && p.IsActive)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => MapToReadDto(p))
                .ToListAsync();

            _logger.LogInfo("Se encontraron {Count} pagos para la cita", payments.Count);
            return payments;
        }

        public async Task<PaymentReadDto> CreateAsync(PaymentCreateDto dto)
        {
            _logger.LogInfo("Iniciando creación de pago para cita ID: {AppointmentId}", dto.AppointmentId);

            // Validar que la cita existe
            var appointment = await _db.Appointments.FindAsync(dto.AppointmentId);
            if (appointment == null)
            {
                throw new ArgumentException("La cita especificada no existe.");
            }

            // Crear entidad
            var entity = new Payment
            {
                AppointmentId = dto.AppointmentId,
                Amount = dto.Amount,
                PaymentDate = DateTime.Now,
                PaymentMethod = dto.PaymentMethod,
                Notes = dto.Notes,
                IsActive = dto.IsActive ?? true
            };

            _db.Payments.Add(entity);
            await _db.SaveChangesAsync();

            _logger.LogInfo("Pago creado exitosamente con ID: {Id}", entity.PaymentId);
            return MapToReadDto(entity);
        }

        public async Task<bool> UpdateAsync(int id, PaymentUpdateDto dto)
        {
            _logger.LogInfo("Iniciando actualización de pago ID: {Id}", id);

            var entity = await _db.Payments.FindAsync(id);
            if (entity == null || !entity.IsActive)
            {
                _logger.LogWarning("Pago con ID {Id} no encontrado", id);
                return false;
            }

            // Actualizar campos
            if (dto.Amount.HasValue)
                entity.Amount = dto.Amount.Value;

            if (!string.IsNullOrWhiteSpace(dto.PaymentMethod))
                entity.PaymentMethod = dto.PaymentMethod;

            if (dto.Notes != null)
                entity.Notes = dto.Notes;

            if (dto.IsActive.HasValue)
                entity.IsActive = dto.IsActive.Value;

            await _db.SaveChangesAsync();

            _logger.LogInfo("Pago {Id} actualizado exitosamente", id);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInfo("Desactivando pago ID: {Id}", id);

            var payment = await _db.Payments.FindAsync(id);
            if (payment == null)
            {
                _logger.LogWarning("Pago con ID {Id} no encontrado", id);
                return false;
            }

            payment.IsActive = false;
            await _db.SaveChangesAsync();

            _logger.LogInfo("Pago {Id} desactivado exitosamente", id);
            return true;
        }

        private static PaymentReadDto MapToReadDto(Payment payment)
        {
            return new PaymentReadDto
            {
                PaymentId = payment.PaymentId,
                AppointmentId = payment.AppointmentId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Notes = payment.Notes,
                IsActive = payment.IsActive
            };
        }
    }
}