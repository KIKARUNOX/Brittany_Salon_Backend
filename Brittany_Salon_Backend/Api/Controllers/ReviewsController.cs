using Brittany_Salon_Backend.Application.DTOs.Review;
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
     //Obtener todos las reseñas
        [HttpGet]
        [ProducesResponseType(typeof(List<ReviewReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ReviewReadDto>>> GetAll()
        {
            var reviews = await _reviewService.GetAllAsync();
            return Ok(reviews);
        }
    }
}
