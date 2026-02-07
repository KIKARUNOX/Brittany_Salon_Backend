using Brittany_Salon_Backend.Application.DTOs.Category;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brittany_Salon_Backend.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CategoryReadDto> CreateAsync(CategoryCreateDto dto)
        {
            var exists = await _db.Categories
                .AnyAsync(c => c.CategoryName.ToLower() == dto.CategoryName.Trim().ToLower());

            if (exists)
                throw new InvalidOperationException("Ya existe una categoría con ese nombre.");

            var entity = new Category
            {
                CategoryName = dto.CategoryName.Trim(),
                CategoryDescription = dto.CategoryDescription?.Trim(),
                IsActive = true
            };

            _db.Categories.Add(entity);
            await _db.SaveChangesAsync();

            return MapToReadDto(entity);
        }

        public async Task<List<CategoryReadDto>> GetAllAsync(bool onlyActive = true)
        {
            var query = _db.Categories.AsNoTracking();

            if (onlyActive)
                query = query.Where(c => c.IsActive);

            return await query
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategoryReadDto
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDescription,
                    IsActive = c.IsActive
                })
                .ToListAsync();
        }

        public async Task<CategoryReadDto?> GetByIdAsync(int id)
        {
            var entity = await _db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (entity == null) return null;

            return MapToReadDto(entity);
        }

        private static CategoryReadDto MapToReadDto(Category entity)
        {
            return new CategoryReadDto
            {
                CategoryId = entity.CategoryId,
                CategoryName = entity.CategoryName,
                CategoryDescription = entity.CategoryDescription,
                IsActive = entity.IsActive
            };
        }
    }
}
