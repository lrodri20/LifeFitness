using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IPreferencesService
    {
        Task<MatchingPreferenceDto> GetMatchingPreferencesAsync(int userId);
        Task<MatchingPreferenceDto> UpdateMatchingPreferencesAsync(int userId, UpdateMatchingPreferenceDto preferences);
        Task DeleteMatchingPreferencesAsync(int userId);
    }
}