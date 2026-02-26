using System.ComponentModel.DataAnnotations;

namespace Brittany_Salon_Backend.Application.DTOs.Authentication
{
    // DTO para solicitar un nuevo Access Token usando Refresh Token
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "El refresh token es requerido")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
 