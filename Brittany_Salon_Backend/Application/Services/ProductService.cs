using Brittany_Salon_Backend.Application.DTOs.Product;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Brittany_Salon_Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Infrastructure.Logging;
// using Brittany_Salon_Backend.Application.Validators; // si luego haces ProductValidator

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
            // Si luego haces ProductValidator, lo pones igual que Service:
            // var validationErrors = ProductValidator.ValidateCreate(dto, _logger);
            // if (validationErrors.Count > 0) throw new ValidationException(validationErrors);

            var entity = new Product
            {
                ProductName = dto.ProductName.Trim(),
                ProductDescription = dto.ProductDescription?.Trim(),
                Price = dto.Price,
                ImageUrl = null,
                ExpirationDate = dto.ExpirationDate,
                IsActive = true
            };

            _db.Products.Add(entity);
            await _db.SaveChangesAsync();

            // Misma lógica que Service: procesar imagen después de tener ID
            if (dto.Image != null && dto.Image.Length > 0)
            {
                await ProcessProductImageAsync(entity, dto.Image);
            }

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
                IsActive = entity.IsActive
            };
        }

        private async Task ProcessProductImageAsync(Product entity, IFormFile imageFile)
        {
            // Igual que Service: ProcessAndSaveImageAsync(file, "imageService", id)
            // Si quieres otra carpeta/clave, cambia "imageProduct" o algo, pero lo dejo similar:
            string imageUrl = await _imageService.ProcessAndSaveImageAsync(imageFile, "imageProduct", entity.ProductId);
            entity.ImageUrl = imageUrl;
            await _db.SaveChangesAsync();
        }

        // Para el futuro (cuando hagamos editar producto):
        private async Task UpdateProductImageAsync(Product entity, IFormFile newImage)
        {
            if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
            {
                _imageService.DeleteImage(entity.ImageUrl);
            }

            string imageUrl = await _imageService.ProcessAndSaveImageAsync(newImage, "imageProduct", entity.ProductId);
            entity.ImageUrl = imageUrl;
        }
    }
}
