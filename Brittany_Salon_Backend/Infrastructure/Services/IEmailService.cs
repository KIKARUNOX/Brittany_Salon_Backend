namespace Brittany_Salon_Backend.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetCodeAsync(string toEmail, string toName, string code);
    }
}
