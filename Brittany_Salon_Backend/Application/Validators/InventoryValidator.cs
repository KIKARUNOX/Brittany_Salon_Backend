using Brittany_Salon_Backend.Application.DTOs.Inventory;
using Brittany_Salon_Backend.Infrastructure.Logging;

namespace Brittany_Salon_Backend.Application.Validators
{
    public static class InventoryValidator
    {
        public static List<string> ValidateCreate(InventoryCreateDto dto, IDevLogger? logger = null)
        {
            var errors = new List<string>();

            logger?.LogDebug("Iniciando validación de inventario (create)");

            errors.AddRange(ValidateProductId(dto.ProductId));
            errors.AddRange(ValidateQuantity(dto.Quantity));
            errors.AddRange(ValidateMinimumStock(dto.MinimumStock));
            errors.AddRange(ValidateCategory(dto.Category));

            if (errors.Count > 0)
                logger?.LogWarning("Validación de inventario (create) fallida: {Errors}", string.Join(", ", errors));

            return errors;
        }

        public static List<string> ValidateProductId(int productId)
        {
            var errors = new List<string>();

            if (productId <= 0)
                errors.Add("El ID del producto debe ser mayor a 0.");

            return errors;
        }

        public static List<string> ValidateQuantity(int quantity)
        {
            var errors = new List<string>();

            if (quantity < 0)
                errors.Add("La cantidad no puede ser negativa.");

            return errors;
        }

        public static List<string> ValidateMinimumStock(int minimumStock)
        {
            var errors = new List<string>();

            if (minimumStock < 0)
                errors.Add("El stock mínimo no puede ser negativo.");

            return errors;
        }

        public static List<string> ValidateCategory(string? category)
        {
            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(category))
            {
                var trimmed = category.Trim();
                if (trimmed.Length > 255)
                    errors.Add("La categoría no puede exceder 255 caracteres.");
            }

            return errors;
        }
    }
}
