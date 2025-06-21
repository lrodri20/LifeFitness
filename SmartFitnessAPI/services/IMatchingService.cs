using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IMatchService
    {
        Task<ActiveMatchesResponseDto> GetActiveMatchesAsync(int userId, string sortBy, int limit, int offset);
        Task RemoveMatchAsync(int matchId, int userId);
        Task BlockUserAsync(int matchId, int userId);
        Task<ActiveMatchDto> GetMatchDetailsAsync(int matchId, int userId);
        Task<int> GetActiveMatchCountAsync(int userId);
        Task<bool> AreMatchedAsync(int userId1, int userId2);
    }
}