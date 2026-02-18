using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brittany_Salon_Backend.Domain.Entities
{
    [Table("Inventory")]
    public class Inventory
    {
        [Key]
        [Column("inventoryId")]
        public int InventoryId { get; set; }

        [Required]
        [Column("productId")]
        public int ProductId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("minimumStock")]
        public int MinimumStock { get; set; }

        [MaxLength(255)]
        [Column("category")]
        public string? Category { get; set; }

        [Column("isActive")]
        public bool IsActive { get; set; } = true;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
