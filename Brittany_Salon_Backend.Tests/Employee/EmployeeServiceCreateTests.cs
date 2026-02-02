using Brittany_Salon_Backend.Application.DTOs.Employee;
using Brittany_Salon_Backend.Application.Exceptions;
using Brittany_Salon_Backend.Application.Services;
using Brittany_Salon_Backend.Application.Validators;
using Brittany_Salon_Backend.Domain.Entities;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Brittany_Salon_Backend.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text;
using Xunit;

public class EmployeeServiceCreateTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static (EmployeeService svc, Mock<IImageService> img, Mock<IDevLogger> log, AppDbContext db) Build()
    {
        var db = CreateDb();
        var img = new Mock<IImageService>();
        var log = new Mock<IDevLogger>();

        var svc = new EmployeeService(db, img.Object, log.Object);
        return (svc, img, log, db);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesEmployee_AndNormalizesNameEmail()
    {
        var (svc, _, _, db) = Build();

        var dto = new EmployeeCreateDto
        {
            Name = "Isaac Chevez",              
            Phone = "88887777",               
            Email = "isaac.chevez@test.com",    
            Password = "Password123!",       
            Specialty = "Colorista",
            IsActive = true
        };

        var created = await svc.CreateAsync(dto);

        Assert.True(created.Id > 0);
        Assert.Equal("Isaac Chevez", created.Name);
        Assert.Equal("isaac.chevez@test.com", created.Email);
        Assert.Equal("88887777", created.Phone);
        Assert.Equal("Colorista", created.Specialty);

        // Confirmar persistencia
        var entity = await db.Employees.FirstOrDefaultAsync(e => e.Id == created.Id);
        Assert.NotNull(entity);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailDuplicated_ThrowsDuplicateResourceException()
    {
        var (svc, _, log, db) = Build();

        // Arrange: empleado existente
        db.Employees.Add(new Employee
        {
            Name = "Ana",
            Phone = "88887777",
            Email = "duplicado@test.com",
            Password = "Password123!",
            IsActive = true
        });
        await db.SaveChangesAsync();

       
        var dto = new EmployeeCreateDto
        {
            Name = "Juan Perez",
            Phone = "88887777",
            Email = "duplicado@test.com",
            Password = "Password123!",
            Specialty = "Colorista",
            IsActive = true
        };

        
        var errors = EmployeeValidator.ValidateCreate(dto, log.Object);
        Assert.True(errors.Count == 0, "Validator errors: " + string.Join(" | ", errors));

        
        var ex = await Assert.ThrowsAsync<DuplicateResourceException>(() => svc.CreateAsync(dto));
        Assert.Equal("Email", ex.Field);
    }

    [Fact]
    public async Task CreateAsync_WithImage_CallsImageService_AndSetsImageUrl()
    {
        var (svc, img, log, db) = Build();

        img.Setup(x => x.ProcessAndSaveImageAsync(It.IsAny<IFormFile>(), "imageUser", It.IsAny<int>()))
           .ReturnsAsync("https://cdn.test/employee-1.png");

        // Crear un IFormFile falso (imagen)
        var content = "fake image bytes";
        var fileBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(fileBytes);

        IFormFile formFile = new FormFile(stream, 0, fileBytes.Length, "Image", "test.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var dto = new EmployeeCreateDto
        {
            Name = "Maria Lopez",
            Phone = "88887777",
            Email = "maria@test.com",
            Password = "Password123!",     
            Specialty = "Colorista",       
            IsActive = true,
            Image = formFile
        };

        var created = await svc.CreateAsync(dto);

        img.Verify(x => x.ProcessAndSaveImageAsync(It.IsAny<IFormFile>(), "imageUser", created.Id), Times.Once);

       
        var entity = await db.Employees.FirstAsync(e => e.Id == created.Id);
        Assert.Equal("https://cdn.test/employee-1.png", entity.Image);
    }

}
