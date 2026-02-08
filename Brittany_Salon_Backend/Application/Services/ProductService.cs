using Brittany_Salon_Backend.Application.DTOs.Product;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Brittany_Salon_Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Infrastructure.Logging;
// using Brittany_Salon_Backend.Application.Validators;

namespace Brittany_Salon_Backend.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;
        private readonly IDevLogger _logger;

        public ProductService(AppDbContext db, IImageService imageService, IDevLogger logger)
        {
            _db = db;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task<ProductReadDto> CreateAsync(ProductCreateDto dto)
        {
            // (Opcional) Validación tipo ServiceValidator
            // var validationErrors = ProductValidator.ValidateCreate(dto, _logger);
            // if (validationErrors.Count > 0) throw new ValidationException(validationErrors);

            var categoryExists = await _db.Categories
                .AsNoTracking()
                .AnyAsync(c => c.CategoryId == dto.CategoryId && c.IsActive);

            if (!categoryExists)
                throw new InvalidOperationException("La categoría no existe o está inactiva.");

            var entity = new Product
            {
                ProductName = dto.ProductName.Trim(),
                ProductDescription = dto.ProductDescription?.Trim(),
                Price = dto.Price,
                ImageUrl = null,
                ExpirationDate = dto.ExpirationDate,
                IsActive = true,
                CategoryId = dto.CategoryId
            };

            _db.Products.Add(entity);
            await _db.SaveChangesAsync();

       
            if (dto.Image != null && dto.Image.Length > 0)
            {
                await ProcessProductImageAsync(entity, dto.Image);
            }

            
            await _db.Entry(entity).Reference(p => p.Category).LoadAsync();

            return MapToReadDto(entity);
        }

        private static ProductReadDto MapToReadDto(Product entity)
        {
            return new ProductReadDto
            {
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                ProductDescription = entity.ProductDescription,
                Price = entity.Price,
                ImageUrl = entity.ImageUrl,
                ExpirationDate = entity.ExpirationDate,
                IsActive = entity.IsActive,
                CategoryId = entity.CategoryId,
                CategoryName = entity.Category?.CategoryName ?? string.Empty
            };
        }

        private async Task ProcessProductImageAsync(Product entity, IFormFile imageFile)
        {
            string imageUrl = await _imageService.ProcessAndSaveImageAsync(imageFile, "imageProduct", entity.ProductId);
            entity.ImageUrl = imageUrl;
            await _db.SaveChangesAsync();
        }

        private async Task UpdateProductImageAsync(Product entity, IFormFile newImage)
        {
            if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
            {
                _imageService.DeleteImage(entity.ImageUrl);
            }

            string imageUrl = await _imageService.ProcessAndSaveImageAsync(newImage, "imageProduct", entity.ProductId);
            entity.ImageUrl = imageUrl;
        }
        public async Task<List<ProductReadDto>> GetAllAsync(bool? onlyActive = null)
        {
            IQueryable<Product> query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category);

            if (onlyActive.HasValue)
                query = query.Where(p => p.IsActive == onlyActive.Value);

            return await query
                .OrderBy(p => p.ProductName)
                .Select(p => new ProductReadDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    ExpirationDate = p.ExpirationDate,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.CategoryName : string.Empty
                })
                .ToListAsync();
        }

        public async Task<ProductReadDto?> GetByIdAsync(int id)
        {
            var p = await _db.Products
                .AsNoTracking()
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.ProductId == id);

            if (p is null) return null;

            return new ProductReadDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductDescription = p.ProductDescription,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                ExpirationDate = p.ExpirationDate,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.CategoryName : string.Empty
            };
        }
        public async Task<List<ProductReadDto>> SearchByNameAsync(string name, bool? onlyActive = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<ProductReadDto>();

            var searchTerm = name.Trim().ToLower();

            IQueryable<Product> query = _db.Products
                .AsNoTracking()
                .Include(p => p.Category);

            if (onlyActive.HasValue)
                query = query.Where(p => p.IsActive == onlyActive.Value);

            return await query
                .Where(p => p.ProductName.ToLower().Contains(searchTerm))
                .OrderBy(p => p.ProductName)
                .Select(p => new ProductReadDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    ExpirationDate = p.ExpirationDate,
                    IsActive = p.IsActive,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.CategoryName : string.Empty
                })
                .ToListAsync();
        }



    }
}
