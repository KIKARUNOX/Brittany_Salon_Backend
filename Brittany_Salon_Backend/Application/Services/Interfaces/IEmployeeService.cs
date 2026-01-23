using Brittany_Salon_Backend.Application.DTOs.Employee;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IEmployeeService
    {
        /// <summary>
        /// Obtiene todos los empleados
        /// </summary>
        /// <param name="onlyActive">Si es true, solo retorna empleados activos</param>
        Task<List<EmployeeReadDto>> GetAllAsync(bool onlyActive = false);

        /// <summary>
        /// Obtiene un empleado por su ID
        /// </summary>
        Task<EmployeeReadDto?> GetByIdAsync(int id);

        /// <summary>
        /// Crea un nuevo empleado
        /// </summary>
        Task<EmployeeReadDto> CreateAsync(EmployeeCreateDto dto);
    }
}
