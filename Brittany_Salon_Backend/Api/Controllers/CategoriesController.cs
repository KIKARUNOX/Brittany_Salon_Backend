using Brittany_Salon_Backend.Application.DTOs.Category;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryReadDto>>> GetAll([FromQuery] bool onlyActive = true)
        {
            var result = await _categoryService.GetAllAsync(onlyActive);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryReadDto>> GetById(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
            if (result is null) return NotFound("Categoría no encontrada.");
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryReadDto>> Create([FromBody] CategoryCreateDto dto)
        {
            var created = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.CategoryId }, created);
        }
    }
}
