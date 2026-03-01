namespace Brittany_Salon_Backend.Application.Services.Interfaces
{
    public interface IPasswordResetStore
    {
        void SaveCode(string email, string code);
        bool ValidateCode(string email, string code);
        void RemoveCode(string email);
    }
}
