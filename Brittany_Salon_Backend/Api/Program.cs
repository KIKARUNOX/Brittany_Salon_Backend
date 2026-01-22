using Brittany_Salon_Backend.Application.Services;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // Habilita Controllers

builder.Services.AddEndpointsApiExplorer(); // Permite descubrir endpoints para Swagger
builder.Services.AddSwaggerGen(); // Genera Swagger UI

builder.Services.AddDbContext<AppDbContext>(options => // Configura EF Core + SQL Server
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IServiceService, ServiceService>(); // Inyección de dependencias
builder.Services.AddScoped<IEmployeeService, EmployeeService>(); // Inyección de dependencias Employee

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita Swagger
    app.UseSwaggerUI(); // Habilita Swagger UI
}

app.UseHttpsRedirection(); // Redirige HTTP -> HTTPS

app.MapControllers(); // Mapea rutas de Controllers

app.Run(); // Inicia la API
