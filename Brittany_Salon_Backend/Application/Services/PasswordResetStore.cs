using System.Collections.Concurrent;
using Brittany_Salon_Backend.Application.Services.Interfaces;

namespace Brittany_Salon_Backend.Application.Services
{
    public class PasswordResetStore : IPasswordResetStore
    {
        private readonly ConcurrentDictionary<string, (string Code, DateTime ExpiresAt)> _store =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(15);

        public void SaveCode(string email, string code)
        {
            _store[email] = (code, DateTime.UtcNow.Add(CodeTtl));
        }

        public bool ValidateCode(string email, string code)
        {
            if (!_store.TryGetValue(email, out var entry))
                return false;

            if (DateTime.UtcNow > entry.ExpiresAt)
            {
                _store.TryRemove(email, out _);
                return false;
            }

            return string.Equals(entry.Code, code, StringComparison.OrdinalIgnoreCase);
        }

        public void RemoveCode(string email)
        {
            _store.TryRemove(email, out _);
        }
    }
}
