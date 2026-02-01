using Brittany_Salon_Backend.Application.DTOs.Appointment;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
        {
            var appointmentId = await _appointmentService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(Create),
                new { id = appointmentId },
                new { appointmentId }
            );
        }
        [HttpGet]
        public async Task<ActionResult<List<AppointmentReadDto>>> GetAll()
        {
            var result = await _appointmentService.GetAllAsync();
            return Ok(result);
        }
        [HttpGet("by-date")]
        public async Task<ActionResult<List<AppointmentReadDto>>> GetByDate([FromQuery] DateTime date)
        {
            var result = await _appointmentService.GetByDateAsync(date);
            return Ok(result);
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<List<AppointmentReadDto>>> GetByStatus([FromQuery] string status)
        {
            var result = await _appointmentService.GetByStatusAsync(status);
            return Ok(result);
        }
        [HttpGet("by-client/{clientId:int}")]
        public async Task<ActionResult<List<AppointmentReadDto>>> GetByClientId(int clientId)
        {
            var result = await _appointmentService.GetByClientIdAsync(clientId);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePending(int id, [FromBody] AppointmentUpdateDto dto)
        {
            var ok = await _appointmentService.UpdatePendingAsync(id, dto);
            if (!ok) return NotFound("Cita no encontrada.");

            return Ok("Cita actualizada.");
        }


        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentDetailDto>> GetById(int id)
        {
            var result = await _appointmentService.GetByIdAsync(id);
            if (result is null) return NotFound("Cita no encontrada.");

            return Ok(result);
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var ok = await _appointmentService.CancelAsync(id);
            if (!ok) return NotFound("Cita no encontrada.");

            return Ok("Cita cancelada.");
        }

        [HttpPut("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var ok = await _appointmentService.CompleteAsync(id);
            if (!ok) return NotFound("Cita no encontrada.");

            return Ok("Cita completada.");
        }


    }
}
