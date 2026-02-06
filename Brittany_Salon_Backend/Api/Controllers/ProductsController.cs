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
        public ActionResult GetById(int id)
        {
            return Ok();
        }
    }
}
