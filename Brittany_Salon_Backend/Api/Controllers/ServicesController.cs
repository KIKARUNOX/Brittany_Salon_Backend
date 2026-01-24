using Brittany_Salon_Backend.Application.DTOs.Service;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Brittany_Salon_Backend.Infrastructure.Services;


namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly IImageService _imageService;


        public ServicesController(IServiceService serviceService, IImageService imageService)
        {
            _serviceService = serviceService;
            _imageService = imageService;
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

        // POST: api/services  (FormData + imagen opcional)
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<ActionResult<ServiceReadDto>> Create([FromForm] ServiceCreateDto dto)
        {
            try
            {
                // 1) Crear servicio (sin imagen por ahora)
                var created = await _serviceService.CreateAsync(dto);

                // 2) Si viene imagen, guardarla y guardar URL en BD
                if (dto.Image != null && dto.Image.Length > 0)
                {
                    var url = await _imageService.ProcessAndSaveImageAsync(dto.Image, "imageService", created.ServiceId);
                    await _serviceService.UpdateImageUrlAsync(created.ServiceId, url);
                }

                // 3) Devolver el servicio ya con ImageUrl
                var updated = await _serviceService.GetByIdAsync(created.ServiceId);
                return CreatedAtAction(nameof(GetById), new { id = created.ServiceId }, updated ?? created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }


        // PUT: api/services/5
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] ServiceUpdateDto dto)
        {
            try
            {
                // 1) Actualizar datos normales del servicio
                var updated = await _serviceService.UpdateAsync(id, dto);
                if (!updated) return NotFound("Servicio no encontrado.");

                // 2) Si mandaron imagen, guardarla y actualizar el ImageUrl en BD
                if (dto.Image != null && dto.Image.Length > 0)
                {
                    var url = await _imageService.ProcessAndSaveImageAsync(dto.Image, "imageService", id);
                    await _serviceService.UpdateImageUrlAsync(id, url);
                }

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
