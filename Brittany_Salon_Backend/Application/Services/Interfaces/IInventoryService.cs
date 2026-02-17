using Brittany_Salon_Backend.Application.DTOs.Inventory;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<List<InventoryReadDto>> GetAllActiveAsync();
        Task<List<InventoryReadDto>> GetAllAsync();
        Task<InventoryReadDto?> GetByIdAsync(int id);
        Task<InventoryReadDto?> GetByProductIdAsync(int productId);
        Task<InventoryReadDto> CreateAsync(InventoryCreateDto dto);
    }
}
