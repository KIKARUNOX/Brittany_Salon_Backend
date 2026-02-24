namespace Brittany_Salon_Backend.Infrastructure.Settings
{
    /// <summary>
    /// Configuración de JWT (JSON Web Tokens)
    /// Se configura desde appsettings.json
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        /// <summary>
        /// Clave secreta para firmar los tokens (mínimo 32 caracteres)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Emisor del token
        /// </summary>
        public string Issuer { get; set; } = string.Empty;

        /// <summary>
        /// Audiencia del token
        /// </summary>
        public string Audience { get; set; } = string.Empty;

        /// <summary>
        /// Tiempo de expiración del Access Token en minutos (default: 15)
        /// </summary>
        public int AccessTokenExpirationMinutes { get; set; } = 15;

        /// <summary>
        /// Tiempo de expiración del Refresh Token en días (default: 7)
        /// </summary>
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// Validación básica de la configuración
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SecretKey))
                throw new InvalidOperationException("JwtSettings.SecretKey no está configurado");

            if (SecretKey.Length < 32)
                throw new InvalidOperationException("JwtSettings.SecretKey debe tener al menos 32 caracteres");

            if (string.IsNullOrWhiteSpace(Issuer))
                throw new InvalidOperationException("JwtSettings.Issuer no está configurado");

            if (string.IsNullOrWhiteSpace(Audience))
                throw new InvalidOperationException("JwtSettings.Audience no está configurado");

            if (AccessTokenExpirationMinutes <= 0)
                throw new InvalidOperationException("JwtSettings.AccessTokenExpirationMinutes debe ser mayor a 0");

            if (RefreshTokenExpirationDays <= 0)
                throw new InvalidOperationException("JwtSettings.RefreshTokenExpirationDays debe ser mayor a 0");
        }
    }
}
