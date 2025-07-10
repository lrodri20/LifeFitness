
namespace SmartFitnessApi.Models
{
    /// <summary>
    /// Represents a chat message sent within a confirmed match.
    /// </summary>
    public class Message
    {
        public int Id { get; set; }

        // Foreign key to the match
        public int MatchId { get; set; }
        public Match Match { get; set; } = null!;

        // Who sent the message
        public int SenderId { get; set; }
        public Profile Sender { get; set; } = null!;

        // Message content and timestamp
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false;
    }
}