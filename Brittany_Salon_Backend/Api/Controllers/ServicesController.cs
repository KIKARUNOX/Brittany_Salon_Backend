using Brittany_Salon_Backend.Application.DTOs.Service;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;

        public ServicesController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        // GET: api/services?onlyActive=true
        [HttpGet]
        public async Task<ActionResult<List<ServiceReadDto>>> GetAll([FromQuery] bool onlyActive = false)
        {
            var result = await _serviceService.GetAllAsync(onlyActive);
            return Ok(result);
        }

        // GET: api/services/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceReadDto>> GetById(int id)
        {
            var result = await _serviceService.GetByIdAsync(id);
            if (result is null) return NotFound("Servicio no encontrado.");

            return Ok(result);
        }

        // POST: api/services
        [HttpPost]
        public async Task<ActionResult<ServiceReadDto>> Create([FromBody] ServiceCreateDto dto)
        {
            try
            {
                var created = await _serviceService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.ServiceId }, created);
            }
            catch (InvalidOperationException ex)
            {
                // Ej: nombre repetido
                return Conflict(ex.Message);
            }
        }

        // PUT: api/services/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceUpdateDto dto)
        {
            try
            {
                var updated = await _serviceService.UpdateAsync(id, dto);
                if (!updated) return NotFound("Servicio no encontrado.");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // PATCH: api/services/5/deactivate
        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _serviceService.DeactivateAsync(id);
            if (!ok) return NotFound("Servicio no encontrado.");

            return NoContent();
        }
    }
}
