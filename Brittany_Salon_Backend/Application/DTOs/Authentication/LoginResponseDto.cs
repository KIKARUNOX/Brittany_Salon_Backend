namespace Brittany_Salon_Backend.Application.DTOs.Authentication
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;  // "EMPLOYEE" o "CLIENT"
        
        // JWT Tokens
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpirationDate { get; set; }
        public DateTime RefreshTokenExpirationDate { get; set; }

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
