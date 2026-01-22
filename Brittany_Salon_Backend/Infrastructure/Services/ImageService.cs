using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Brittany_Salon_Backend.Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private const string ImageFolder = "public/imageUser";
        private const int MaxImageWidth = 800;
        private const int MaxImageHeight = 800;

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> ProcessAndSaveEmployeeImageAsync(IFormFile imageFile, int employeeId)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("El archivo de imagen no es válido.");

            // Crear directorio si no existe
            var imageDirectory = Path.Combine(_environment.ContentRootPath, ImageFolder);
            if (!Directory.Exists(imageDirectory))
                Directory.CreateDirectory(imageDirectory);

            // Generar nombre único para la imagen
            var fileName = $"employee_{employeeId}_{DateTime.Now:yyyyMMddHHmmss}.webp";
            var filePath = Path.Combine(imageDirectory, fileName);

            // Procesar y guardar la imagen como WebP
            using (var inputStream = imageFile.OpenReadStream())
            using (var image = await Image.LoadAsync(inputStream))
            {
                // Redimensionar si es necesario
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(MaxImageWidth, MaxImageHeight),
                    Mode = ResizeMode.Max
                }));

                // Guardar como WebP
                await image.SaveAsync(filePath, new WebpEncoder());
            }

            // Retornar URL relativa
            return $"/imageUser/{fileName}";
        }

        public bool DeleteEmployeeImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                // Extraer nombre del archivo de la URL
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_environment.ContentRootPath, ImageFolder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
