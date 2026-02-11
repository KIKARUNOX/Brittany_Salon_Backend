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

        private ActionResult BuildBadRequest(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDto dto)
        {
            try
            {
                var appointmentId = await _appointmentService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = appointmentId },
                    new { appointmentId }
                );
            }
            catch (Brittany_Salon_Backend.Application.Exceptions.ValidationException ex)
            {
                return BadRequest(new
                {
                    message = "Se encontraron errores de validación.",
                    errors = ex.Errors
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Ocurrió un error inesperado." });
            }
        }


        // GET: api/appointments
        [HttpGet]
        public async Task<ActionResult<List<AppointmentReadDto>>> GetAll()
        {
            try
            {
                var result = await _appointmentService.GetAllAsync();
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // GET: api/appointments/by-date?date=2026-02-01
        [HttpGet("by-date")]
        public async Task<ActionResult<List<AppointmentReadDto>>> GetByDate([FromQuery] DateTime date)
        {
            try
            {
                var result = await _appointmentService.GetByDateAsync(date);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // GET: api/appointments/by-status?status=Pendiente
        [HttpGet("by-status")]
        public async Task<ActionResult<List<AppointmentReadDto>>> GetByStatus([FromQuery] string status)
        {
            try
            {
                var result = await _appointmentService.GetByStatusAsync(status);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // GET: api/appointments/by-client/1
        [HttpGet("by-client/{clientId:int}")]
        public async Task<ActionResult<List<AppointmentReadDto>>> GetByClientId(int clientId)
        {
            try
            {
                var result = await _appointmentService.GetByClientIdAsync(clientId);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // PUT: api/appointments/10
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePending(int id, [FromBody] AppointmentUpdateDto dto)
        {
            try
            {
                var ok = await _appointmentService.UpdatePendingAsync(id, dto);
                if (!ok) return NotFound(new { message = "Cita no encontrada." });

                return Ok(new { message = "Cita actualizada." });
            }
            catch (Brittany_Salon_Backend.Application.Exceptions.ValidationException ex)
            {
                return BadRequest(new
                {
                    message = "Se encontraron errores de validación.",
                    errors = ex.Errors
                });
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (Exception ex)
            {
                return BuildServerError(ex);
            }
        }
        private IActionResult BuildServerError(Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Ocurrió un error inesperado.",
                detail = ex.Message
            });
        }


        // GET: api/appointments/10
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppointmentDetailDto>> GetById(int id)
        {
            try
            {
                var result = await _appointmentService.GetByIdAsync(id);
                if (result is null) return NotFound(new { message = "Cita no encontrada." });

                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // PUT: api/appointments/10/cancel
        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var ok = await _appointmentService.CancelAsync(id);
                if (!ok) return NotFound(new { message = "Cita no encontrada." });

                return Ok(new { message = "Cita cancelada." });
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // PUT: api/appointments/10/complete
        [HttpPut("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var ok = await _appointmentService.CompleteAsync(id);
                if (!ok) return NotFound(new { message = "Cita no encontrada." });

                return Ok(new { message = "Cita completada." });
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // POST: api/appointments/validate-availability
        [HttpPost("validate-availability")]
        public async Task<ActionResult<AppointmentAvailabilityResponseDto>> ValidateAvailability([FromBody] AppointmentAvailabilityRequestDto dto)
        {
            try
            {
                var result = await _appointmentService.ValidateAvailabilityAsync(dto);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // GET: api/appointments/10/pending-balance
        [HttpGet("{id:int}/pending-balance")]
        public async Task<IActionResult> GetPendingBalance(int id)
        {
            try
            {
                var pendingBalance = await _appointmentService.GetPendingBalanceAsync(id);
                return Ok(new { appointmentId = id, pendingBalance });
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }

        // GET: api/appointments/10/pending-balance-client
        [HttpGet("{id:int}/pending-balance-client")]
        public async Task<IActionResult> GetPendingBalanceClient(int id)
        {
            try
            {
                var result = await _appointmentService.GetPendingBalanceClientAsync(id);
                return Ok(new { appointmentId = id, pendingBalance = result });
            }
            catch (ArgumentNullException ex)
            {
                return BuildBadRequest(ex);
            }
            catch (InvalidOperationException ex)
            {
                return BuildBadRequest(ex);
            }
        }
    }
}
