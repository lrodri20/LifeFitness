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

                // Skip matches without any messages
                if (lastMsg == null)
                    continue;

                // 5) count unread messages
                var unreadCount = await _db.Messages
                    .Where(msg =>
                        msg.MatchId == match.Id
                        && msg.SenderId != userId
                        && !msg.IsRead)
                    .CountAsync();

                chats.Add(new ChatDto
                {
                    ChatId = match.Id,               // e.g. "c101"
                    MatchId = match.Id,             // e.g. "m101"
                    OtherUser = new OtherUserDto
                    {
                        Id = profile.UserId,
                        Name = profile.DisplayName,
                        AvatarUrl = profile.ProfilePictureUrl
                    },
                    LastMessage = lastMsg.Content,
                    Time = lastMsg.SentAt
                                           .ToLocalTime()
                                           .ToString("h:mm tt"),
                    UnreadCount = unreadCount
                });
            }

            return chats;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int matchId, int userId)
        {
            // 1) Ensure this user is part of the match
            var isParticipant = await _db.Matches
                .AnyAsync(m =>
                    m.Id == matchId &&
                    (m.User1Id == userId || m.User2Id == userId));

            if (!isParticipant)
                throw new UnauthorizedAccessException("You are not a member of this chat.");

            // 2) Query all messages in chronological order
            var dtos = await _db.Messages
                .Where(msg => msg.MatchId == matchId)
                .OrderBy(msg => msg.SentAt)
                .Select(msg => new MessageDto
                {
                    Id = $"msg{msg.Id}",
                    SenderId = $"u{msg.SenderId}",
                    Text = msg.Content,
                    SentAt = msg.SentAt.ToUniversalTime().ToString("o")
                })
                .ToListAsync();

            // 3) OPTIONAL: mark incoming messages as read
            var unread = await _db.Messages
                .Where(msg =>
                    msg.MatchId == matchId &&
                    msg.SenderId != userId &&
                    !msg.IsRead)
                .ToListAsync();

            if (unread.Count > 0)
            {
                unread.ForEach(m => m.IsRead = true);
                await _db.SaveChangesAsync();
            }

            return dtos;
        }
    }
}
