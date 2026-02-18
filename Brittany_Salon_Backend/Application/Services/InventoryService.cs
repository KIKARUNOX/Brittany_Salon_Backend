using Brittany_Salon_Backend.Application.DTOs.Inventory;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brittany_Salon_Backend.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _db;
        private readonly IDevLogger _logger;

        public InventoryService(AppDbContext db, IDevLogger logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<InventoryReadDto>> GetAllActiveAsync()
        {
            _logger.LogInfo("Obteniendo todos los registros activos del inventario");

            var inventoryItems = await _db.Inventory
                .AsNoTracking()
                .Where(i => i.IsActive)
                .OrderBy(i => i.Location)
                .ThenBy(i => i.ProductId)
                .Select(i => MapToReadDto(i))
                .ToListAsync();

            _logger.LogInfo("Se encontraron {Count} registros activos del inventario", inventoryItems.Count);
            return inventoryItems;
        }

        public async Task<List<InventoryReadDto>> GetAllAsync()
        {
            _logger.LogInfo("Obteniendo todos los registros del inventario");

            var inventoryItems = await _db.Inventory
                .AsNoTracking()
                .OrderBy(i => i.Location)
                .ThenBy(i => i.ProductId)
                .Select(i => MapToReadDto(i))
                .ToListAsync();

            _logger.LogInfo("Se encontraron {Count} registros del inventario", inventoryItems.Count);
            return inventoryItems;
        }
        public async Task<InventoryReadDto?> GetByIdAsync(int id)
        {
            _logger.LogInfo("Obteniendo registro del inventario con ID: {Id}", id);

            var inventory = await _db.Inventory
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InventoryId == id);

            if (inventory == null)
            {
                _logger.LogInfo("No se encontró registro del inventario con ID: {Id}", id);
                return null;
            }

            return MapToReadDto(inventory);
        }

        public async Task<InventoryReadDto?> GetByProductIdAsync(int productId)
        {
            _logger.LogInfo("Obteniendo inventario para producto con ID: {ProductId}", productId);

            var inventory = await _db.Inventory
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null)
            {
                _logger.LogInfo("No se encontró inventario para producto con ID: {ProductId}", productId);
                return null;
            }

            return MapToReadDto(inventory);
        }

        public async Task<InventoryReadDto> CreateAsync(InventoryCreateDto dto)
        {
            _logger.LogInfo("Iniciando creación de registro de inventario para producto ID: {ProductId}", dto.ProductId);

            var productExists = await _db.Products.AnyAsync(p => p.ProductId == dto.ProductId);
            if (!productExists)
            {
                _logger.LogWarning("Producto con ID {ProductId} no encontrado", dto.ProductId);
                throw new NotFoundException($"El producto con ID {dto.ProductId} no existe");
            }

            var inventoryExists = await _db.Inventory.AnyAsync(i => i.ProductId == dto.ProductId);
            if (inventoryExists)
            {
                _logger.LogWarning("Ya existe un inventario para el producto ID {ProductId}", dto.ProductId);
                throw new InvalidOperationException($"Ya existe un registro de inventario para el producto ID {dto.ProductId}");
            }

            var entity = new Inventory
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                MinimumStock = dto.MinimumStock,
                MaximumStock = dto.MaximumStock,
                Location = !string.IsNullOrWhiteSpace(dto.Location) ? dto.Location.Trim() : null,
                Notes = !string.IsNullOrWhiteSpace(dto.Notes) ? dto.Notes.Trim() : null,
                IsActive = true,
                LastUpdatedAt = DateTime.Now
            };

            _db.Inventory.Add(entity);
            await _db.SaveChangesAsync();

            _logger.LogInfo("Inventario creado exitosamente con ID: {InventoryId}", entity.InventoryId);
            return MapToReadDto(entity);
        }

        private static InventoryReadDto MapToReadDto(Inventory inventory)
        {
            return new InventoryReadDto
            {
                InventoryId = inventory.InventoryId,
                ProductId = inventory.ProductId,
                Quantity = inventory.Quantity,
                MinimumStock = inventory.MinimumStock,
                MaximumStock = inventory.MaximumStock,
                Location = inventory.Location,
                Notes = inventory.Notes,
                IsActive = inventory.IsActive,
                LastUpdatedAt = inventory.LastUpdatedAt
            };
        }
    }
}
