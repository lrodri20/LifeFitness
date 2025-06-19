using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IMatchingService
    {
        Task<IEnumerable<ProfileMatchingDto>> GetPotentialMatchesAsync(int userId, int radiusMiles, int limit);
    }
}