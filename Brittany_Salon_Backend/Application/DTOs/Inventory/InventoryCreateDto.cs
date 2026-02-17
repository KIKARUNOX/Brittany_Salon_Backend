using System.ComponentModel.DataAnnotations;

namespace Brittany_Salon_Backend.Application.DTOs.Inventory
{
    public class InventoryCreateDto
    {
        [Required(ErrorMessage = "El ID del producto es obligatorio.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public int MinimumStock { get; set; }

        [MaxLength(255, ErrorMessage = "La categoría no puede exceder 255 caracteres.")]
        public string? Category { get; set; }
    }
}
