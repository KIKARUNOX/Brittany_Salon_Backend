namespace Brittany_Salon_Backend.Application.DTOs.Inventory
{
    public class InventoryReadDto
    {
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }
    }
}
