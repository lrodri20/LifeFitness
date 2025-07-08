
namespace SmartFitnessApi.Models
{
    /// <summary>
    /// Represents a confirmed match between two users.
    /// </summary>
    public class Match
    {
        public int Id { get; set; }

        // User1 initiator or alphabetically first
        public int User1Id { get; set; }
        public Profile User1 { get; set; } = null!;

        // User2 counterpart
        public int User2Id { get; set; }
        public Profile User2 { get; set; } = null!;

        // When the match was created (mutual like)
        public DateTime CreatedAt { get; set; }

        // All messages in this match thread
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}