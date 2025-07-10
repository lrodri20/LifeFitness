// Services/ChatService.cs
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Data;
using SmartFitnessApi.Data.Dtos;
using SmartFitnessApi.Models;     // Match, Message, Profile entities

namespace SmartFitnessApi.Services
{
    public class ChatService : IChatService
    {
        private readonly SmartFitnessDbContext _db;

        public ChatService(SmartFitnessDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ChatDto>> GetChatsAsync(int userId)
        {
            // 1) find all matches for this user
            var matches = await _db.Matches
                .Where(m => m.User1Id == userId || m.User2Id == userId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var chats = new List<ChatDto>();

            foreach (var match in matches)
            {
                // 2) identify the “other” user in the match
                var otherId = match.User1Id == userId
                    ? match.User2Id
                    : match.User1Id;

                // 3) load their profile
                var profile = await _db.Profiles
                    .Where(p => p.UserId == otherId)
                    .Select(p => new
                    {
                        p.UserId,
                        p.DisplayName,
                        p.ProfilePictureUrl
                    })
                    .FirstOrDefaultAsync();

                // 4) load the latest message in that match
                var lastMsg = await _db.Messages
                    .Where(msg => msg.MatchId == match.Id)
                    .OrderByDescending(msg => msg.SentAt)
                    .FirstOrDefaultAsync();

                // 5) count unread messages
                //    (assumes your Message entity has a bool IsRead property;
                //     otherwise this will always be zero)
                var unreadCount = await _db.Messages
                    .Where(msg =>
                        msg.MatchId == match.Id
                        && msg.SenderId != userId
                        && !msg.IsRead)
                    .CountAsync();

                chats.Add(new ChatDto
                {
                    ChatId = $"c{match.Id}",               // e.g. "c101"
                    MatchId = $"m{match.Id}",               // e.g. "m101"
                    OtherUser = new OtherUserDto
                    {
                        Id = profile.UserId,
                        Name = profile.DisplayName,
                        AvatarUrl = profile.ProfilePictureUrl
                    },
                    LastMessage = lastMsg?.Content ?? string.Empty,
                    Time = lastMsg?.SentAt
                                           .ToLocalTime()
                                           .ToString("h:mm tt") ?? string.Empty,
                    UnreadCount = unreadCount
                });
            }

            return chats;
        }
    }
}
