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

        /// <summary>
        /// Descuenta la cantidad de un producto del inventario
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="quantity">Cantidad a descontar</param>
        Task<bool> DiscountQuantityAsync(int productId, int quantity);

        /// <summary>
        /// Descuenta múltiples productos del inventario
        /// </summary>
        /// <param name="products">Diccionario con ProductId y cantidad a descontar</param>
        Task<bool> DiscountMultipleAsync(Dictionary<int, int> products);
    }
}
