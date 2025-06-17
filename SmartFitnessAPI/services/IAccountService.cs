using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IAccountService
    {
        Task<UserDto> SignUpAsync(SignUpRequest request);
        Task<User?> GetByEmailAsync(string email);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task<ProfileDto?> GetProfileAsync(int userId);
        Task<ProfileDto> UpdateProfileAsync(int userId, ProfileDto input);
        Task RevokeRefreshTokenAsync(string refreshToken);


    }
}