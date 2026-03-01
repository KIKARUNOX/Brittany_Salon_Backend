using System.Net;
using System.Net.Mail;
using Brittany_Salon_Backend.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Brittany_Salon_Backend.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtp)
        {
            _smtp = smtp.Value;
        }

        public async Task SendPasswordResetCodeAsync(string toEmail, string toName, string code)
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                EnableSsl = _smtp.EnableSsl,
                Credentials = new NetworkCredential(_smtp.SenderEmail, _smtp.Password)
            };

            var body = $"Hola {toName},\n\n" +
                       $"Tu codigo de recuperacion de contrasena es:\n\n" +
                       $"    {code}\n\n" +
                       $"Este codigo es valido por 15 minutos.\n\n" +
                       $"Si no solicitaste este codigo, ignora este mensaje.\n\n" +
                       $"Brittany Salon";

            var message = new MailMessage
            {
                From = new MailAddress(_smtp.SenderEmail, _smtp.SenderName),
                Subject = "Codigo de recuperacion - Brittany Salon",
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message);
        }
    }
}
