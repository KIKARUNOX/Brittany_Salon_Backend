using Brittany_Salon_Backend.Application.DTOs.Client;
using Brittany_Salon_Backend.Infrastructure.Logging;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Brittany_Salon_Backend.Application.Validators
{
    public static partial class ClientValidator
    {
        // Tamaño máximo de imagen: 5MB
        private const long MaxImageSize = 5 * 1024 * 1024;

        // Extensiones de imagen permitidas
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

        // Content types permitidos
        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"];

        /// <summary>
        /// Valida todos los campos del DTO de creación de cliente
        /// </summary>
        public static List<string> ValidateCreate(ClientCreateDto dto, IDevLogger? logger = null)
        {
            var errors = new List<string>();

            logger?.LogDebug("Iniciando validación de cliente...");

            // Validar Nombre
            errors.AddRange(ValidateName(dto.Name));

            // Validar Email
            errors.AddRange(ValidateEmail(dto.Email));

            // Validar Teléfono (opcional)
            if (!string.IsNullOrWhiteSpace(dto.Phone))
            {
                errors.AddRange(ValidatePhone(dto.Phone));
            }

            // Validar Contraseña
            errors.AddRange(ValidatePassword(dto.Password));

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
                logger?.LogInfo("Validación exitosa para cliente: {Email}", dto.Email);
            }

            return errors;
        }

        /// <summary>
        /// Valida nombre
        /// </summary>
        private static List<string> ValidateName(string name)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add("El nombre es obligatorio.");
                return errors;
            }

            var trimmed = name.Trim();

            if (trimmed.Length < 2)
                errors.Add("El nombre debe tener al menos 2 caracteres.");

            if (trimmed.Length > 100)
                errors.Add("El nombre no puede exceder 100 caracteres.");

            // Solo letras, espacios y caracteres especiales comunes
            if (!Regex.IsMatch(trimmed, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s\-']+$"))
                errors.Add("El nombre solo puede contener letras, espacios, guiones y apóstrofes.");

            return errors;
        }

        /// <summary>
        /// Valida email
        /// </summary>
        private static List<string> ValidateEmail(string email)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add("El correo electrónico es obligatorio.");
                return errors;
            }

            var trimmed = email.Trim();

            if (trimmed.Length > 150)
                errors.Add("El correo electrónico no puede exceder 150 caracteres.");

            // Validación básica de formato de email
            if (!Regex.IsMatch(trimmed, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                errors.Add("El formato del correo electrónico no es válido.");

            return errors;
        }

        /// <summary>
        /// Valida teléfono (opcional)
        /// </summary>
        private static List<string> ValidatePhone(string phone)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(phone))
                return errors; // Teléfono es opcional

            var trimmed = phone.Trim();

            if (trimmed.Length > 20)
                errors.Add("El teléfono no puede exceder 20 caracteres.");

            // Solo dígitos, espacios, guiones, paréntesis
            if (!Regex.IsMatch(trimmed, @"^[\d\s\-\(\)\+]+$"))
                errors.Add("El teléfono solo puede contener dígitos, espacios, guiones, paréntesis y el símbolo +.");

            return errors;
        }

        /// <summary>
        /// Valida contraseña
        /// </summary>
        private static List<string> ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("La contraseña es obligatoria.");
                return errors;
            }

            if (password.Length < 6)
                errors.Add("La contraseña debe tener al menos 6 caracteres.");

            if (password.Length > 255)
                errors.Add("La contraseña no puede exceder 255 caracteres.");

            return errors;
        }

        /// <summary>
        /// Valida archivo de imagen
        /// </summary>
        private static List<string> ValidateImageFile(IFormFile image, IDevLogger? logger = null)
        {
            var errors = new List<string>();

            logger?.LogDebug("Validando archivo de imagen: {FileName}, Tamaño: {Size} bytes", image.FileName, image.Length);

            // Validar tamaño
            if (image.Length > MaxImageSize)
            {
                errors.Add("La imagen no puede exceder 5MB.");
                return errors;
            }

            // Validar extensión
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (!AllowedExtensions.Contains(extension))
            {
                errors.Add("Formato de imagen no válido. Formatos permitidos: JPEG, PNG, GIF, WebP.");
                return errors;
            }

            // Validar content type
            if (!AllowedContentTypes.Contains(image.ContentType.ToLower()))
            {
                errors.Add("Tipo de contenido de imagen no válido.");
                return errors;
            }

            logger?.LogDebug("Archivo de imagen válido: {FileName}", image.FileName);
            return errors;
        }

        /// <summary>
        /// Valida los campos del DTO de actualización de cliente
        /// Solo valida los campos que se proporcionan (no null)
        /// </summary>
        public static List<string> ValidateUpdate(ClientUpdateDto dto, IDevLogger? logger = null)
        {
            var errors = new List<string>();

            logger?.LogDebug("Iniciando validación de actualización de cliente...");

            // Validar Nombre (si se proporciona)
            if (!string.IsNullOrWhiteSpace(dto.Name))
                errors.AddRange(ValidateName(dto.Name));

            // Validar Email (si se proporciona)
            if (!string.IsNullOrWhiteSpace(dto.Email))
                errors.AddRange(ValidateEmail(dto.Email));

            // Validar Teléfono (si se proporciona)
            if (!string.IsNullOrWhiteSpace(dto.Phone))
                errors.AddRange(ValidatePhone(dto.Phone));

            // Validar Contraseña (si se proporciona)
            if (!string.IsNullOrWhiteSpace(dto.Password))
                errors.AddRange(ValidatePassword(dto.Password));

            // Validar Imagen (si se proporciona)
            if (dto.Image != null)
            {
                errors.AddRange(ValidateImageFile(dto.Image, logger));
            }

            if (errors.Count > 0)
            {
                logger?.LogWarning("Validación de update fallida con {Count} errores: {Errors}", errors.Count, string.Join(", ", errors));
            }

            return errors;
        }
    }
}