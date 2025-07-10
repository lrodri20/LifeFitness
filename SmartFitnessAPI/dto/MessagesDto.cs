using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Data.Dtos
{
    public class ChatDto
    {
        public string ChatId { get; set; }
        public string MatchId { get; set; }
        public OtherUserDto OtherUser { get; set; }
        public string LastMessage { get; set; }
        public string Time { get; set; }      // e.g. "2:45 PM"
        public int UnreadCount { get; set; }
    }
}