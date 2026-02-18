using Brittany_Salon_Backend.Application.DTOs.Review;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
      
      //Obtener todas las reseñas
        [HttpGet]
        [ProducesResponseType(typeof(List<ReviewReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReviewReadDto>>> GetAll()
        {
            var reviews = await _reviewService.GetAllAsync();
            return Ok(reviews);
        }

        //Obtener reseñas por ID de cliente
        [HttpGet("by-client/{clientId:int}")]
        [ProducesResponseType(typeof(List<ReviewReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ReviewReadDto>>> GetByClientId(int clientId)
        {
            var reviews = await _reviewService.GetByClientIdAsync(clientId);
            if (reviews == null || reviews.Count == 0)
                return NotFound($"No se encontraron reseñas para el cliente con ID {clientId}.");

            return Ok(reviews);
        }

     //Crear una reseña
        [HttpPost]
        [ProducesResponseType(typeof(ReviewReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReviewReadDto>> Create([FromBody] ReviewCreateDto reviewCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdReview = await _reviewService.CreateAsync(reviewCreateDto);
                return CreatedAtAction(nameof(GetAll), new { id = createdReview.ReviewId }, createdReview);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
        }

        //Agregar respuesta a una reseña
        [HttpPut("{reviewId:int}/response")]
        [ProducesResponseType(typeof(ReviewReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReviewReadDto>> AddResponse(int reviewId, [FromBody] ReviewResponseDto responseDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _reviewService.AddResponseAsync(reviewId, responseDto);
                if (updated == null)
                    return NotFound(new { message = "Reseña no encontrada." });

                return Ok(updated);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
