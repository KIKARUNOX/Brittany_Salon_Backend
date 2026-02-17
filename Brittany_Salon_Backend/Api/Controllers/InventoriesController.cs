using Brittany_Salon_Backend.Application.DTOs.Inventory;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Brittany_Salon_Backend.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoriesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(List<InventoryReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<InventoryReadDto>>> GetAllActive()
        {
            var inventoryItems = await _inventoryService.GetAllActiveAsync();
            return Ok(inventoryItems);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<InventoryReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<InventoryReadDto>>> GetAll()
        {
            var inventoryItems = await _inventoryService.GetAllAsync();
            return Ok(inventoryItems);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InventoryReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryReadDto>> GetById(int id)
        {
            var inventoryItem = await _inventoryService.GetByIdAsync(id);
            if (inventoryItem == null)
                return NotFound();

            return Ok(inventoryItem);
        }

        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(InventoryReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryReadDto>> GetByProductId(int productId)
        {
            var inventoryItem = await _inventoryService.GetByProductIdAsync(productId);
            if (inventoryItem == null)
                return NotFound();

            return Ok(inventoryItem);
        }

        [HttpPost]
        [ProducesResponseType(typeof(InventoryReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<InventoryReadDto>> Create([FromBody] InventoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _inventoryService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.InventoryId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
