using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public interface IMatchRequestService
    {
        Task<MatchRequest> CreateMatchRequestAsync(int requesterId, int requesteeId, string? initialMessage = null);
        Task<List<MatchRequestDto>> GetMatchRequestsAsync(int userId, string direction, string status);
        Task<IEnumerable<MatchRequest>> GetPendingRequestsAsync(int userId);
        Task AcceptMatchRequestAsync(int requestId, int userId);
        Task RejectMatchRequestAsync(int requestId, int userId);

    }
}