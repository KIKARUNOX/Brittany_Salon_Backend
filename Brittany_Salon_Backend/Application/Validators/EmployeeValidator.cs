using Brittany_Salon_Backend.Application.DTOs.Employee;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Brittany_Salon_Backend.Application.Validators
{
    public static partial class EmployeeValidator
    {
        // Tamaño máximo de imagen: 5MB
        private const long MaxImageSize = 5 * 1024 * 1024;

        // Extensiones de imagen permitidas
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

        // Content types permitidos
        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"];

        /// <summary>
        /// Valida todos los campos del DTO de creación de empleado
        /// </summary>
        public static List<string> ValidateCreate(EmployeeCreateDto dto, IDevLogger? logger = null)
        {
            var errors = new List<string>();

            logger?.LogDebug("Iniciando validación de empleado...");

            // Validar Nombre
            errors.AddRange(ValidateName(dto.Name));

            // Validar Email
            errors.AddRange(ValidateEmail(dto.Email));

            // Validar Teléfono
            errors.AddRange(ValidatePhone(dto.Phone));

            // Validar Contraseña
            errors.AddRange(ValidatePassword(dto.Password));

            // Validar Especialidad
            errors.AddRange(ValidateSpecialty(dto.Specialty));

            // Validar Imagen (si se proporciona)
            if (dto.Image != null)
            {
                errors.AddRange(ValidateImageFile(dto.Image, logger));
            }

            if (errors.Count > 0)
            {
                logger?.LogWarning("Validación fallida con {Count} errores: {Errors}", errors.Count, string.Join(", ", errors));
            }
            else
            {
                logger?.LogInfo("Validación exitosa para empleado: {Email}", dto.Email);
            }

            return errors;
        }

        /// <summary>
        /// Valida el nombre del empleado
        /// </summary>
        public static List<string> ValidateName(string? name)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add("El nombre es obligatorio.");
                return errors;
            }

            var trimmedName = name.Trim();

            if (trimmedName.Length < 2)
                errors.Add("El nombre debe tener al menos 2 caracteres.");

            if (trimmedName.Length > 100)
                errors.Add("El nombre no puede exceder 100 caracteres.");

            // Solo letras, espacios y caracteres especiales comunes en nombres
            if (!NameRegex().IsMatch(trimmedName))
                errors.Add("El nombre solo puede contener letras, espacios, apóstrofes y guiones.");

            // No permitir múltiples espacios consecutivos
            if (trimmedName.Contains("  "))
                errors.Add("El nombre no puede contener espacios múltiples consecutivos.");

            return errors;
        }

        /// <summary>
        /// Valida el correo electrónico
        /// </summary>
        public static List<string> ValidateEmail(string? email)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add("El correo electrónico es obligatorio.");
                return errors;
            }

            var trimmedEmail = email.Trim().ToLower();

            if (trimmedEmail.Length > 150)
                errors.Add("El correo electrónico no puede exceder 150 caracteres.");

            // Validar formato de email
            if (!EmailRegex().IsMatch(trimmedEmail))
                errors.Add("El formato del correo electrónico no es válido.");

            // Validar dominios no permitidos (temporales)
            string[] blockedDomains = ["tempmail.com", "throwaway.com", "mailinator.com", "guerrillamail.com"];
            var domain = trimmedEmail.Split('@').LastOrDefault();
            if (domain != null && blockedDomains.Contains(domain))
                errors.Add("No se permiten correos electrónicos temporales.");

            return errors;
        }

        /// <summary>
        /// Valida el número de teléfono
        /// </summary>
        public static List<string> ValidatePhone(int phone)
        {
            var errors = new List<string>();

            if (phone <= 0)
            {
                errors.Add("El número de teléfono es obligatorio.");
                return errors;
            }

            var phoneString = phone.ToString();

            // Teléfono de Costa Rica: 8 dígitos
            if (phoneString.Length != 8)
                errors.Add("El número de teléfono debe tener exactamente 8 dígitos.");

            // Debe comenzar con 2, 4, 5, 6, 7 u 8 (prefijos válidos en CR)
            char firstDigit = phoneString[0];
            char[] validPrefixes = ['2', '4', '5', '6', '7', '8'];
            if (!validPrefixes.Contains(firstDigit))
                errors.Add("El número de teléfono debe comenzar con 2, 4, 5, 6, 7 u 8.");

            return errors;
        }

        /// <summary>
        /// Valida la contraseña
        /// </summary>
        public static List<string> ValidatePassword(string? password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("La contraseña es obligatoria.");
                return errors;
            }

            if (password.Length < 8)
                errors.Add("La contraseña debe tener al menos 8 caracteres.");

            if (password.Length > 255)
                errors.Add("La contraseña no puede exceder 255 caracteres.");

            if (!password.Any(char.IsUpper))
                errors.Add("La contraseña debe contener al menos una letra mayúscula.");

            if (!password.Any(char.IsLower))
                errors.Add("La contraseña debe contener al menos una letra minúscula.");

            if (!password.Any(char.IsDigit))
                errors.Add("La contraseña debe contener al menos un número.");

            if (!SpecialCharRegex().IsMatch(password))
                errors.Add("La contraseña debe contener al menos un carácter especial (!@#$%^&*(),.?\":{}|<>).");

            // No permitir espacios
            if (password.Contains(' '))
                errors.Add("La contraseña no puede contener espacios.");

            return errors;
        }

        /// <summary>
        /// Valida la especialidad
        /// </summary>
        public static List<string> ValidateSpecialty(string? specialty)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(specialty))
                return errors; // Especialidad es opcional

            var trimmedSpecialty = specialty.Trim();

            if (trimmedSpecialty.Length < 3)
                errors.Add("La especialidad debe tener al menos 3 caracteres.");

            if (trimmedSpecialty.Length > 100)
                errors.Add("La especialidad no puede exceder 100 caracteres.");

            // Solo letras, números, espacios y caracteres comunes
            if (!SpecialtyRegex().IsMatch(trimmedSpecialty))
                errors.Add("La especialidad contiene caracteres no permitidos.");

            return errors;
        }

        /// <summary>
        /// Valida el archivo de imagen (IFormFile)
        /// </summary>
        public static List<string> ValidateImageFile(IFormFile? file, IDevLogger? logger = null)
        {
            var errors = new List<string>();

            if (file == null || file.Length == 0)
                return errors; // Imagen es opcional

            // LOG: Información detallada del archivo recibido
            logger?.LogDebug("========== VALIDACIÓN DE IMAGEN ==========");
            logger?.LogDebug("FileName: {FileName}", file.FileName);
            logger?.LogDebug("ContentType recibido: '{ContentType}'", file.ContentType);
            logger?.LogDebug("Tamaño: {Size} bytes ({SizeMB:F2} MB)", file.Length, file.Length / 1024.0 / 1024.0);
            logger?.LogDebug("ContentDisposition: {CD}", file.ContentDisposition);

            // Validar tamaño
            if (file.Length > MaxImageSize)
            {
                logger?.LogWarning("Imagen excede tamaño máximo: {Size} > {Max}", file.Length, MaxImageSize);
                errors.Add("La imagen no puede exceder 5MB.");
            }

            // Validar extensión (más confiable que ContentType)
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            logger?.LogDebug("Extensión extraída: '{Extension}'", extension);
            logger?.LogDebug("Extensiones permitidas: [{Allowed}]", string.Join(", ", AllowedExtensions));
            
            if (!AllowedExtensions.Contains(extension))
            {
                logger?.LogWarning("Extensión no permitida: '{Extension}'", extension);
                errors.Add($"Extensión de archivo no permitida. Formatos válidos: {string.Join(", ", AllowedExtensions)}");
            }

            // Validar content type (flexibilizado para diferentes clientes)
            var contentType = file.ContentType.ToLowerInvariant();
            logger?.LogDebug("ContentType normalizado: '{ContentType}'", contentType);
            logger?.LogDebug("ContentTypes permitidos: [{Allowed}]", string.Join(", ", AllowedContentTypes));
            
            bool isInAllowedList = AllowedContentTypes.Contains(contentType);
            bool startsWithImage = contentType.StartsWith("image/");
            bool isOctetStream = contentType == "application/octet-stream";
            
            logger?.LogDebug("¿Está en lista permitida?: {Result}", isInAllowedList);
            logger?.LogDebug("¿Comienza con 'image/'?: {Result}", startsWithImage);
            logger?.LogDebug("¿Es octet-stream?: {Result}", isOctetStream);
            
            bool isValidContentType = isInAllowedList || startsWithImage || isOctetStream;
            
            if (!isValidContentType)
            {
                logger?.LogWarning("ContentType NO válido: '{ContentType}' - No cumple ninguna condición", contentType);
                errors.Add($"Tipo de contenido no permitido: '{file.ContentType}'. Tipos válidos: JPEG, PNG, GIF, WebP");
            }
            else
            {
                logger?.LogInfo("ContentType válido: '{ContentType}'", contentType);
            }

            logger?.LogDebug("========== FIN VALIDACIÓN DE IMAGEN ==========");

            return errors;
        }

        // Expresiones regulares compiladas para mejor rendimiento
        [GeneratedRegex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\-']+$")]
        private static partial Regex NameRegex();

        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        private static partial Regex EmailRegex();

        [GeneratedRegex(@"[!@#$%^&*(),.?""':{}|<>]")]
        private static partial Regex SpecialCharRegex();

        [GeneratedRegex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ0-9\s\-,\.]+$")]
        private static partial Regex SpecialtyRegex();
    }
}
