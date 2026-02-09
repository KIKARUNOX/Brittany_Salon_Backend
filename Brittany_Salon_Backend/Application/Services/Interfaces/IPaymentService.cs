using Brittany_Salon_Backend.Application.DTOs.Payment;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<List<PaymentReadDto>> GetAllAsync();
        Task<PaymentReadDto?> GetByIdAsync(int id);
        Task<List<PaymentReadDto>> GetByAppointmentIdAsync(int appointmentId);
        Task<List<PaymentReadDto>> GetByClientIdAsync(int clientId);
        Task<List<PaymentReadDto>> GetAllWithInactiveAsync();
        Task<PaymentReadDto> CreateAsync(PaymentCreateDto dto);
        Task<bool> UpdateAsync(int id, PaymentUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}