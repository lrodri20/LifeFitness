using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class Match
    {
        public int Id { get; set; }

        // The user who initiated the match request
        public int RequesterId { get; set; }
        public Profile Requester { get; set; } = null!;

        // The user who received the match request
        public int RequesteeId { get; set; }
        public Profile Requestee { get; set; } = null!;

        public MatchStatus Status { get; set; } = MatchStatus.Pending;
        public double CompatibilityScore { get; set; }

        // Match Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public DateTime? LastInteractionAt { get; set; }

        // Optional message with request
        public string? InitialMessage { get; set; }

        // Shared activities that led to the match
        public string? SharedActivitiesJson { get; set; } // Store as JSON array

    }
}