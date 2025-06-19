using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IAuthenticationService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
        string GenerateJwtToken(User user);
        int TokenExpiryInSeconds { get; }

    }
}