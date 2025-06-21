using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface ISearchService
    {
        Task<SearchResultsDto> SearchPotentialMatchesAsync(int userId, SearchParameters parameters);
        Task<SearchPreviewDto> GetSearchPreviewAsync(int userId);
    }
}