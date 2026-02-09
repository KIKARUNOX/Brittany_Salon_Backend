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

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryReadDto>> Update(int id,[FromBody] CategoryUpdateDto dto)
        {
            var updated = await _categoryService.UpdateAsync(id, dto);

            if (updated is null)
                return NotFound("Categoría no encontrada.");

            return Ok(updated);
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var ok = await _categoryService.DeactivateAsync(id);
            if (!ok) return NotFound("Categoría no encontrada.");
            return NoContent();
        }

        [HttpPatch("{id:int}/reactivate")]
        public async Task<IActionResult> Reactivate(int id)
        {
            var ok = await _categoryService.ReactivateAsync(id);
            if (!ok) return NotFound("Categoría no encontrada.");
            return NoContent();
        }


    }
}
