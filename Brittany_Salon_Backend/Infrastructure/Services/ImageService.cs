using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Security.Cryptography;
using System.Text;

namespace Brittany_Salon_Backend.Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly string _basePath;
        private readonly string _baseUrl;

        public ImageService(IWebHostEnvironment environment)
        {
            _basePath = Path.Combine(environment.ContentRootPath, "public", "imageUser");
            _baseUrl = "/imageUser";

            EnsureDirectoryExists();
        }

        /// <summary>
        /// Asegura que el directorio de imágenes exista
        /// </summary>
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        /// <summary>
        /// Procesa y guarda una imagen de empleado
        /// </summary>
        public async Task<string> ProcessAndSaveEmployeeImageAsync(string base64Image, int employeeId)
        {
            // Paso 1: Validar y decodificar la imagen Base64
            byte[] imageBytes = DecodeBase64Image(base64Image);

            // Paso 2: Convertir a formato WebP
            byte[] webpBytes = await ConvertToWebPAsync(imageBytes);

            // Paso 3: Generar nombre de archivo seguro
            string secureFileName = GenerateSecureFileName(employeeId);

            // Paso 4: Guardar imagen en el sistema de archivos
            string savedPath = await SaveImageAsync(webpBytes, secureFileName);

            // Paso 5: Retornar URL accesible
            return GenerateImageUrl(secureFileName);
        }

        /// <summary>
        /// Decodifica una imagen desde Base64
        /// </summary>
        private byte[] DecodeBase64Image(string base64Image)
        {
            if (string.IsNullOrWhiteSpace(base64Image))
                throw new ArgumentException("La imagen no puede estar vacía.");

            // Remover el prefijo data:image/xxx;base64, si existe
            string base64Data = base64Image;
            if (base64Image.Contains(","))
            {
                base64Data = base64Image.Split(',')[1];
            }

            try
            {
                return Convert.FromBase64String(base64Data);
            }
            catch (FormatException)
            {
                throw new ArgumentException("El formato de imagen Base64 no es válido.");
            }
        }

        /// <summary>
        /// Convierte los bytes de imagen a formato WebP optimizado
        /// </summary>
        private async Task<byte[]> ConvertToWebPAsync(byte[] imageBytes)
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var image = await Image.LoadAsync(inputStream);

            using var outputStream = new MemoryStream();
            
            var encoder = new WebpEncoder
            {
                Quality = 80,
                FileFormat = WebpFileFormatType.Lossy
            };

            await image.SaveAsync(outputStream, encoder);
            
            return outputStream.ToArray();
        }

        /// <summary>
        /// Genera un nombre de archivo seguro y codificado
        /// </summary>
        private string GenerateSecureFileName(int employeeId)
        {
            // Crear un identificador único combinando ID + timestamp + random
            string uniqueData = $"{employeeId}_{DateTime.UtcNow.Ticks}_{Guid.NewGuid():N}";

            // Generar hash SHA256 para el nombre del archivo
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(uniqueData));

            // Convertir a string hexadecimal (primeros 16 caracteres para nombre más corto)
            string hashString = Convert.ToHexString(hashBytes)[..16].ToLower();

            return $"emp_{hashString}.webp";
        }

        /// <summary>
        /// Guarda la imagen en el sistema de archivos
        /// </summary>
        private async Task<string> SaveImageAsync(byte[] imageBytes, string fileName)
        {
            string filePath = Path.Combine(_basePath, fileName);

            await File.WriteAllBytesAsync(filePath, imageBytes);

            return filePath;
        }

        /// <summary>
        /// Genera la URL accesible para la imagen
        /// </summary>
        private string GenerateImageUrl(string fileName)
        {
            return $"{_baseUrl}/{fileName}";
        }

        /// <summary>
        /// Elimina una imagen del sistema de archivos
        /// </summary>
        public bool DeleteEmployeeImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;

            string fileName = Path.GetFileName(imageUrl);
            string filePath = Path.Combine(_basePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }

            return false;
        }
    }
}
