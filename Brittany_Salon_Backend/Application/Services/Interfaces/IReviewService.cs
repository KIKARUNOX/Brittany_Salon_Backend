using Brittany_Salon_Backend.Application.DTOs.Review;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewReadDto>> GetAllAsync();
        Task<ReviewReadDto?> GetByIdAsync(int id);
        Task<List<ReviewReadDto>> GetByClientIdAsync(int clientId);
        Task<List<ReviewReadDto>> GetByEmployeeIdAsync(int employeeId);
        Task<ReviewReadDto> CreateAsync(ReviewCreateDto dto);
        Task<ReviewReadDto?> AddResponseAsync(int reviewId, ReviewResponseDto dto);
        Task<bool> UpdateAsync(int id, ReviewUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<double> GetAverageRatingByEmployeeAsync(int employeeId);
    }
}
