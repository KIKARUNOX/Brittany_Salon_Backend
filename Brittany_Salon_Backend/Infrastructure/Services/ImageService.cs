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

        private const string PublicFolder = "public";
        private const int MaxImageWidth = 800;
        private const int MaxImageHeight = 800;
        private const long MaxBytes = 5 * 1024 * 1024; // 5MB

        private static readonly HashSet<string> AllowedContentTypes = new()
        {
            "image/jpeg", "image/png", "image/webp"
        };

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> ProcessAndSaveImageAsync(IFormFile imageFile, string category, int entityId)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new ArgumentException("El archivo de imagen no es válido.");

            if (imageFile.Length > MaxBytes)
                throw new ArgumentException("La imagen excede el tamaño permitido (5MB).");

            if (!AllowedContentTypes.Contains(imageFile.ContentType))
                throw new ArgumentException("Formato no permitido. Use JPG, PNG o WebP.");

            // category ej: imageUser, imageService, etc.
            category = category.Trim().Trim('/');

            var categoryDirectory = Path.Combine(_environment.ContentRootPath, PublicFolder, category);
            if (!Directory.Exists(categoryDirectory))
                Directory.CreateDirectory(categoryDirectory);

            var fileName = $"{category}_{entityId}_{Guid.NewGuid():N}.webp";
            var filePath = Path.Combine(categoryDirectory, fileName);

            using var inputStream = imageFile.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(MaxImageWidth, MaxImageHeight),
                Mode = ResizeMode.Max
            }));

            await image.SaveAsync(filePath, new WebpEncoder { Quality = 75 });

            // URL pública
            return $"/{category}/{fileName}";
        }

        public bool DeleteImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;

            try
            {
                // Ej: /imageUser/xxx.webp
                var clean = imageUrl.Split('?')[0].Trim();
                clean = clean.TrimStart('/');

                var filePath = Path.Combine(_environment.ContentRootPath, PublicFolder, clean);

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
