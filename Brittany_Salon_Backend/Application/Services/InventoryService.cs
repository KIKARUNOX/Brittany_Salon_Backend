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

        /// <summary>
        /// Descuenta la cantidad de un producto del inventario
        /// </summary>
        public async Task<bool> DiscountQuantityAsync(int productId, int quantity)
        {
            if (productId <= 0)
            {
                _logger.LogWarning("ProductId inválido para descuento: {ProductId}", productId);
                return false;
            }

            if (quantity <= 0)
            {
                _logger.LogWarning("Cantidad inválida para descuento: {Quantity}", quantity);
                return false;
            }

            var inventory = await _db.Inventory
                .FirstOrDefaultAsync(i => i.ProductId == productId);

            if (inventory == null)
            {
                _logger.LogWarning("Inventario no encontrado para ProductId: {ProductId}", productId);
                return false;
            }

            if (inventory.Quantity < quantity)
            {
                _logger.LogWarning("Stock insuficiente para ProductId {ProductId}. Stock: {Stock}, Solicitado: {Requested}", 
                    productId, inventory.Quantity, quantity);
                return false;
            }

            inventory.Quantity -= quantity;
            inventory.LastUpdatedAt = DateTime.Now;

            _logger.LogInfo("Inventario descontado para ProductId {ProductId}: {Quantity} unidades", productId, quantity);
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Descuenta múltiples productos del inventario
        /// </summary>
        public async Task<bool> DiscountMultipleAsync(Dictionary<int, int> products)
        {
            if (products == null || products.Count == 0)
            {
                _logger.LogWarning("Diccionario de productos vacío para descuento múltiple");
                return false;
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                foreach (var product in products)
                {
                    var success = await DiscountQuantityAsync(product.Key, product.Value);
                    if (!success)
                    {
                        await tx.RollbackAsync();
                        _logger.LogWarning("Fallo en descuento múltiple para ProductId {ProductId}", product.Key);
                        return false;
                    }
                }

                await tx.CommitAsync();
                _logger.LogInfo("Descuento múltiple completado para {Count} productos", products.Count);
                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogWarning("Error en descuento múltiple: {Error}", ex.InnerException?.Message ?? ex.Message);
                return false;
            }
        }
    }
}
