using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IAccountService
    {
        Task<UserDto> SignUpAsync(SignUpRequest request);
        Task<User?> GetByEmailAsync(string email);
    }
}