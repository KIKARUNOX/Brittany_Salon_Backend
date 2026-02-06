using Brittany_Salon_Backend.Application.DTOs.Product;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductReadDto> CreateAsync(ProductCreateDto dto);

        // Para después
        // Task<List<ProductReadDto>> GetAllAsync(bool onlyActive = false);
        // Task<ProductReadDto?> GetByIdAsync(int id);
    }
}
