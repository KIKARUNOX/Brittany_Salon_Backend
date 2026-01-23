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

        /// <summary>
        /// Actualiza un empleado existente
        /// </summary>
        /// <param name="id">ID del empleado a actualizar</param>
        /// <param name="dto">Datos a actualizar (solo los campos proporcionados)</param>
        /// <returns>True si se actualizo, False si no existe</returns>
        Task<bool> UpdateAsync(int id, EmployeeUpdateDto dto);

        /// <summary>
        /// Desactiva un empleado (eliminacion logica)
        /// </summary>
        /// <param name="id">ID del empleado a desactivar</param>
        /// <returns>True si se desactivo, False si no existe</returns>
        Task<bool> DeactivateAsync(int id);

        /// <summary>
        /// Reactiva un empleado previamente desactivado
        /// </summary>
        /// <param name="id">ID del empleado a reactivar</param>
        /// <returns>True si se reactivo, False si no existe</returns>
        Task<bool> ReactivateAsync(int id);
    }
}
