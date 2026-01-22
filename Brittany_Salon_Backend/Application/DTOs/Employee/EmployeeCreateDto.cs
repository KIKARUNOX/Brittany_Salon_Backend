using System.ComponentModel.DataAnnotations;

namespace Brittany_Salon_Backend.Application.DTOs.Employee
{
    public class EmployeeCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Range(10000000, 99999999, ErrorMessage = "El teléfono debe tener 8 dígitos.")]
        public int Phone { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [MaxLength(150, ErrorMessage = "El correo no puede exceder 150 caracteres.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(255, ErrorMessage = "La contraseña no puede exceder 255 caracteres.")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Imagen en formato Base64 (data:image/png;base64,... o solo el contenido Base64)
        /// Tamaño máximo: 5MB
        /// Formatos permitidos: JPEG, PNG, GIF, WebP
        /// </summary>
        public string? ImageBase64 { get; set; }

        [MaxLength(100, ErrorMessage = "La especialidad no puede exceder 100 caracteres.")]
        public string? Specialty { get; set; }

        public bool? IsActive { get; set; } = true;
    }
}
