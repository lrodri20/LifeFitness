using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IMatchService
    {
        Task<IEnumerable<MatchDto>> GetMatchesAsync(int userId, string sortBy = "compatibility");
        Task RemoveMatchAsync(int matchId, int userId);
        Task BlockUserAsync(int matchId, int userId);
        Task<ActiveMatchDto> GetMatchDetailsAsync(int matchId, int userId);
        Task<int> GetActiveMatchCountAsync(int userId);
        Task<bool> AreMatchedAsync(int userId1, int userId2);
    }
}