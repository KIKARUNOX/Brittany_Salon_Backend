using Brittany_Salon_Backend.Application.DTOs.Authentication;

namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    }
}
