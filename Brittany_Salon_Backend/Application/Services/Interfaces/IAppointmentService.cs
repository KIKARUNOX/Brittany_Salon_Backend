using Brittany_Salon_Backend.Application.DTOs.Appointment;
using System.Threading.Tasks;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<int> CreateAsync(AppointmentCreateDto dto);
    }
}
