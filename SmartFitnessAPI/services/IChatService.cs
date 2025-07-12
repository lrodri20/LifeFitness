// Services/IChatService.cs
using SmartFitnessApi.Data.Dtos;

namespace SmartFitnessApi.Services
{
    public interface IChatService
    {
        Task<IEnumerable<ChatDto>> GetChatsAsync(int userId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int matchId, int userId);
    }
}
