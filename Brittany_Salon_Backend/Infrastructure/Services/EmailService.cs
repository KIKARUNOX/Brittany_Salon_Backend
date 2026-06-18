using Brittany_Salon_Backend.Infrastructure.Logging;
using Brittany_Salon_Backend.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Resend;

namespace Brittany_Salon_Backend.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly ResendSettings _settings;
        private readonly IDevLogger _logger;

        public EmailService(IResend resend, IOptions<ResendSettings> settings, IDevLogger logger)
        {
            _resend = resend;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendPasswordResetCodeAsync(string toEmail, string toName, string code)
        {
            var body = $"Hola {toName},\n\n" +
                       $"Tu codigo de recuperacion de contrasena es:\n\n" +
                       $"    {code}\n\n" +
                       $"Este codigo es valido por 15 minutos.\n\n" +
                       $"Si no solicitaste este codigo, ignora este mensaje.\n\n" +
                       $"Brittany Salon";

            var message = new EmailMessage
            {
                From = $"{_settings.SenderName} <{_settings.SenderEmail}>",
                Subject = "Codigo de recuperacion - Brittany Salon",
                TextBody = body
            };

            message.To.Add(toEmail);

            await _resend.EmailSendAsync(message);

            _logger.LogInfo($"Codigo de recuperacion enviado a: {toEmail}");
        }
    }
}
