using System.ComponentModel.DataAnnotations;

namespace Brittany_Salon_Backend.Application.DTOs.Employee
{
    public class EmployeeCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int Phone { get; set; }

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Imagen en formato Base64 (data:image/png;base64,... o solo el contenido Base64)
        /// </summary>
        public string? ImageBase64 { get; set; }

        [MaxLength(100)]
        public string? Specialty { get; set; }

        public bool? IsActive { get; set; } = true;
    }
}
