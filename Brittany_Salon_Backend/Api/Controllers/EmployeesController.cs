using Brittany_Salon_Backend.Application.DTOs.Employee;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        /// <summary>
        /// Registra un nuevo empleado
        /// </summary>
        /// <param name="dto">Datos del empleado a registrar</param>
        /// <returns>Empleado creado</returns>
        /// <response code="201">Empleado creado exitosamente</response>
        /// <response code="400">Errores de validación</response>
        /// <response code="409">Email o teléfono ya registrado</response>
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeReadDto>> Create([FromBody] EmployeeCreateDto dto)
        {
            try
            {
                var created = await _employeeService.CreateAsync(dto);
                return CreatedAtAction(nameof(Create), new { id = created.Id }, created);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Se encontraron errores de validación.",
                    Errors = ex.Errors
                });
            }
            catch (DuplicateResourceException ex)
            {
                return Conflict(new ErrorResponse
                {
                    Message = ex.Message,
                    Field = ex.Field
                });
            }
        }
    }

    /// <summary>
    /// Respuesta estándar para errores de validación
    /// </summary>
    public class ValidationErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];
    }

    /// <summary>
    /// Respuesta estándar para errores generales
    /// </summary>
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Field { get; set; }
    }
}
