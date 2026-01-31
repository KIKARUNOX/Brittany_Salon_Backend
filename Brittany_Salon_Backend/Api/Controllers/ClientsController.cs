using Brittany_Salon_Backend.Application.DTOs.Client;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        /// <summary>
        /// Obtiene todos los clientes
        /// </summary>
        /// <param name="onlyActive">Si es true, solo retorna clientes activos</param>
        /// <returns>Lista de clientes</returns>
        /// <response code="200">Lista de clientes</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ClientReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ClientReadDto>>> GetAll([FromQuery] bool onlyActive = false)
        {
            var clients = await _clientService.GetAllAsync(onlyActive);
            return Ok(clients);
        }

        /// <summary>
        /// Busca clientes por nombre (búsqueda parcial)
        /// </summary>
        /// <param name="name">Texto a buscar en el nombre</param>
        /// <param name="onlyActive">Si es true, solo retorna clientes activos</param>
        /// <returns>Lista de clientes que coinciden</returns>
        /// <response code="200">Lista de clientes encontrados</response>
        /// <response code="400">Parámetro de búsqueda inválido</response>
        [HttpGet("search/by-name")]
        [ProducesResponseType(typeof(List<ClientReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<ClientReadDto>>> SearchByName(
            [FromQuery] string name,
            [FromQuery] bool onlyActive = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new ErrorResponse { Message = "El parámetro 'name' es requerido." });

            var clients = await _clientService.SearchByNameAsync(name, onlyActive);
            return Ok(clients);
        }

        /// <summary>
        /// Obtiene un cliente por su ID
        /// </summary>
        /// <param name="id">ID del cliente</param>
        /// <returns>Cliente encontrado</returns>
        /// <response code="200">Cliente encontrado</response>
        /// <response code="404">Cliente no encontrado</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClientReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClientReadDto>> GetById(int id)
        {
            var client = await _clientService.GetByIdAsync(id);

            if (client == null)
                return NotFound(new ErrorResponse { Message = "Cliente no encontrado." });

            return Ok(client);
        }

        /// <summary>
        /// Registra un nuevo cliente
        /// </summary>
        /// <param name="dto">Datos del cliente a registrar (multipart/form-data)</param>
        /// <returns>Cliente creado</returns>
        /// <response code="201">Cliente creado exitosamente</response>
        /// <response code="400">Errores de validación</response>
        /// <response code="409">Email ya registrado</response>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ClientReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ClientReadDto>> Create([FromForm] ClientCreateDto dto)
        {
            try
            {
                var created = await _clientService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.ClientId }, created);
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

        /// <summary>
        /// Actualiza un cliente existente
        /// </summary>
        /// <param name="id">ID del cliente a actualizar</param>
        /// <param name="dto">Datos a actualizar (solo los campos proporcionados se actualizan)</param>
        /// <returns>NoContent si se actualizó correctamente</returns>
        /// <response code="204">Cliente actualizado exitosamente</response>
        /// <response code="400">Errores de validación</response>
        /// <response code="404">Cliente no encontrado</response>
        /// <response code="409">Email ya registrado por otro cliente</response>
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromForm] ClientUpdateDto dto)
        {
            try
            {
                var updated = await _clientService.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(new ErrorResponse { Message = "Cliente no encontrado." });

                return NoContent();
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

        /// <summary>
        /// Desactiva un cliente (eliminación lógica)
        /// </summary>
        /// <param name="id">ID del cliente a desactivar</param>
        /// <returns>NoContent si se desactivó correctamente</returns>
        /// <response code="204">Cliente desactivado exitosamente</response>
        /// <response code="404">Cliente no encontrado</response>
        [HttpPatch("{id:int}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _clientService.DeactivateAsync(id);

            if (!result)
                return NotFound(new ErrorResponse { Message = "Cliente no encontrado." });

            return NoContent();
        }
    }
}