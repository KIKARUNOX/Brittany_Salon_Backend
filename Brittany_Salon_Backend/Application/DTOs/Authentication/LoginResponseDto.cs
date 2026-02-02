namespace Brittany_Salon_Backend.Application.DTOs.Authentication
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;  // "EMPLOYEE" o "CLIENT"
        public string Token { get; set; } = string.Empty;  // JWT (se llenará más adelante)

        // Campos específicos de EMPLOYEE
        public string? Specialty { get; set; }

        // Campos específicos de CLIENT
        public decimal? PendingBalance { get; set; }

        // Campos comunes
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
