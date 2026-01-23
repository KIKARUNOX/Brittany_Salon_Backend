using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Brittany_Salon_Backend.Application.DTOs.Service
{
    public class ServiceUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? ServiceDescription { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int DurationMinutes { get; set; }

        [MaxLength(50)]
        public string? ServiceType { get; set; }

        public bool IsActive { get; set; } = true;

        // ✅ Imagen opcional (si no mandan nada, NO se cambia)
        public IFormFile? Image { get; set; }
    }
}
