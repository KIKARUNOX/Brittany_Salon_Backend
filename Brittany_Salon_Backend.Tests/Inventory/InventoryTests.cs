using Brittany_Salon_Backend.Application.DTOs.Inventory;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Application.Services;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class InventoryTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static (InventoryService svc, Mock<IDevLogger> logger, AppDbContext db) Build()
    {
        var db = CreateDb();
        var logger = new Mock<IDevLogger>();
        var svc = new InventoryService(db, logger.Object);
        return (svc, logger, db);
    }

    [Fact]
    public async Task GetAllActiveAsync_WithActiveInventory_ReturnsActiveItems()
    {
        // Arrange
        var (svc, _, db) = Build();
        
        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 100,
            IsActive = true
        };

        var inventory1 = new Inventory
        {
            InventoryId = 1,
            ProductId = 1,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = "Shelf A",
            IsActive = true,
            Product = product
        };

        var inventory2 = new Inventory
        {
            InventoryId = 2,
            ProductId = 1,
            Quantity = 0,
            MinimumStock = 5,
            MaximumStock = 50,
            Location = "Shelf B",
            IsActive = false,
            Product = product
        };

        db.Products.Add(product);
        db.Inventory.AddRange(inventory1, inventory2);
        await db.SaveChangesAsync();

        // Act
        var result = await svc.GetAllActiveAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].InventoryId);
        Assert.True(result[0].IsActive);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllInventory()
    {
        // Arrange
        var (svc, _, db) = Build();

        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 100,
            IsActive = true
        };

        var inventory1 = new Inventory
        {
            InventoryId = 1,
            ProductId = 1,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = "Shelf A",
            IsActive = true,
            Product = product
        };

        var inventory2 = new Inventory
        {
            InventoryId = 2,
            ProductId = 1,
            Quantity = 0,
            MinimumStock = 5,
            MaximumStock = 50,
            Location = "Shelf B",
            IsActive = false,
            Product = product
        };

        db.Products.Add(product);
        db.Inventory.AddRange(inventory1, inventory2);
        await db.SaveChangesAsync();

        // Act
        var result = await svc.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsInventoryItem()
    {
        // Arrange
        var (svc, _, db) = Build();

        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 100,
            IsActive = true
        };

        var inventory = new Inventory
        {
            InventoryId = 1,
            ProductId = 1,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = "Shelf A",
            IsActive = true,
            Product = product
        };

        db.Products.Add(product);
        db.Inventory.Add(inventory);
        await db.SaveChangesAsync();

        // Act
        var result = await svc.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.InventoryId);
        Assert.Equal(50, result.Quantity);
        Assert.Equal(100, result.MaximumStock);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var (svc, _, db) = Build();

        // Act
        var result = await svc.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByProductIdAsync_WithValidProductId_ReturnsInventoryItem()
    {
        // Arrange
        var (svc, _, db) = Build();

        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 100,
            IsActive = true
        };

        var inventory = new Inventory
        {
            InventoryId = 1,
            ProductId = 1,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = "Shelf A",
            IsActive = true,
            Product = product
        };

        db.Products.Add(product);
        db.Inventory.Add(inventory);
        await db.SaveChangesAsync();

        // Act
        var result = await svc.GetByProductIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProductId);
        Assert.Equal(50, result.Quantity);
    }

    [Fact]
    public async Task GetByProductIdAsync_WithInvalidProductId_ReturnsNull()
    {
        // Arrange
        var (svc, _, db) = Build();

        // Act
        var result = await svc.GetByProductIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllActiveAsync_WithNoActiveInventory_ReturnsEmptyList()
    {
        // Arrange
        var (svc, _, db) = Build();

        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 100,
            IsActive = true
        };

        var inventory = new Inventory
        {
            InventoryId = 1,
            ProductId = 1,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = "Shelf A",
            IsActive = false,
            Product = product
        };

        db.Products.Add(product);
        db.Inventory.Add(inventory);
        await db.SaveChangesAsync();

        // Act
        var result = await svc.GetAllActiveAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesInventoryItem()
    {
        // Arrange
        var (svc, _, db) = Build();

        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 100,
            IsActive = true
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var createDto = new InventoryCreateDto
        {
            ProductId = 1,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = "Shelf A",
            Notes = "Test notes"
        };

        // Act
        var result = await svc.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ProductId);
        Assert.Equal(50, result.Quantity);
        Assert.Equal(10, result.MinimumStock);
        Assert.Equal(100, result.MaximumStock);
        Assert.Equal("Shelf A", result.Location);
        Assert.Equal("Test notes", result.Notes);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentProduct_ThrowsNotFoundException()
    {
        // Arrange
        var (svc, _, db) = Build();

        var createDto = new InventoryCreateDto
        {
            ProductId = 999,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = "Shelf A"
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => svc.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateProductId_ThrowsInvalidOperationException()
    {
        // Arrange
        var (svc, _, db) = Build();

        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 100,
            IsActive = true
        };

        var existingInventory = new Inventory
        {
            ProductId = 1,
            Quantity = 30,
            MinimumStock = 5,
            MaximumStock = 50,
            Location = "Existing",
            IsActive = true
        };

        db.Products.Add(product);
        db.Inventory.Add(existingInventory);
        await db.SaveChangesAsync();

        var createDto = new InventoryCreateDto
        {
            ProductId = 1,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = "Shelf A"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithNullLocation_CreatesInventoryItem()
    {
        // Arrange
        var (svc, _, db) = Build();

        var product = new Product
        {
            ProductId = 1,
            ProductName = "Test Product",
            Price = 100,
            IsActive = true
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var createDto = new InventoryCreateDto
        {
            ProductId = 1,
            Quantity = 50,
            MinimumStock = 10,
            MaximumStock = 100,
            Location = null
        };

        // Act
        var result = await svc.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Location);
        Assert.True(result.IsActive);
    }
}
