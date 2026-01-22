using Brittany_Salon_Backend.Application.DTOs.Employee;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto dto);
    }
}
