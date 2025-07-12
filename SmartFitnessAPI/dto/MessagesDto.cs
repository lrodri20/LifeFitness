using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Data.Dtos
{
    public class ChatDto
    {
        public int ChatId { get; set; }
        public int MatchId { get; set; }
        public OtherUserDto OtherUser { get; set; }
        public string LastMessage { get; set; }
        public string Time { get; set; }      // e.g. "2:45 PM"
        public int UnreadCount { get; set; }
    }
    public class MessageDto
    {
        /// <summary>
        /// Synthetic ID for the UI (e.g. "msg42")
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Synthetic sender ID for the UI (e.g. "u17")
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// The message text/content
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// ISO-8601 timestamp
        /// </summary>
        public string SentAt { get; set; }
    }
    public class SendMessageDto
    {
        /// <summary>
        /// The text content of the new message.
        /// </summary>
        public string Text { get; set; }
    }
}