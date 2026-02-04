using Brittany_Salon_Backend.Application.Services;
using Brittany_Salon_Backend.Application.Services.Interfaces;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Persistence;
using Brittany_Salon_Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // Habilita Controllers

builder.Services.AddEndpointsApiExplorer(); // Permite descubrir endpoints para Swagger
builder.Services.AddSwaggerGen(); // Genera Swagger UI

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options => // Configura EF Core + SQL Server
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar DevLogger - Solo loggea en Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IDevLogger, DevLogger>();
}
else
{
    builder.Services.AddScoped<IDevLogger, NullDevLogger>();
}

builder.Services.AddScoped<IServiceService, ServiceService>(); // Inyección de dependencias
builder.Services.AddScoped<IEmployeeService, EmployeeService>(); // Inyección de dependencias Employee
builder.Services.AddScoped<IImageService, ImageService>(); // Inyección de dependencias Image
builder.Services.AddScoped<IAppointmentService, AppointmentService>(); //Inyección de dependencias Appointment
builder.Services.AddScoped<IClientService, ClientService>(); // Inyección de dependencias Client
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>(); // Inyección de dependencias Payment

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Habilita Swagger
    app.UseSwaggerUI(); // Habilita Swagger UI
}

app.UseHttpsRedirection(); // Redirige HTTP -> HTTPS

// Habilita CORS
app.UseCors("AllowFrontend");

// Habilita acceso a archivos estáticos desde /imageUser
var publicPath = Path.Combine(builder.Environment.ContentRootPath, "public");
if (!Directory.Exists(publicPath))
{
    Directory.CreateDirectory(publicPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicPath),
    RequestPath = ""
});

app.MapControllers(); // Mapea rutas de Controllers

app.Run(); // Inicia la API
