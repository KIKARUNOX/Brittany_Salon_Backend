using Brittany_Salon_Backend.Application.DTOs.Product;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // POST: api/products
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductReadDto>> Create([FromForm] ProductCreateDto dto)
        {
            var created = await _productService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductReadDto>> GetById(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (result is null) return NotFound("Producto no encontrado.");
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductReadDto>>> GetAll([FromQuery] bool? onlyActive)
        {
            var result = await _productService.GetAllAsync(onlyActive);
            return Ok(result);
        }
        [HttpGet("search")]
        public async Task<ActionResult<List<ProductReadDto>>> SearchByName(
        [FromQuery] string name,
        [FromQuery] bool? onlyActive)
        {
            var result = await _productService.SearchByNameAsync(name, onlyActive);
            return Ok(result);
        }
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductReadDto>> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            var updated = await _productService.UpdateAsync(id, dto);
            if (updated is null) return NotFound("Producto no encontrado.");
            return Ok(updated);
        }


    }
}
