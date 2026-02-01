namespace Brittany_Salon_Backend.Application.DTOs.Authentication
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;  // "EMPLOYEE" "CLIENT"
        public string Token { get; set; } = string.Empty;  //JWT
    }
}
