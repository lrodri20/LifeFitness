// DTOs
using System.ComponentModel.DataAnnotations;
using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class ActiveMatchesResponseDto
    {
        public List<ActiveMatchDto> Matches { get; set; } = new List<ActiveMatchDto>();
        public int TotalCount { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public bool HasMore => Offset + Matches.Count < TotalCount;
        public MatchesStatisticsDto Statistics { get; set; } = new MatchesStatisticsDto();
    }

    /// <summary>
    /// DTO for an active match (workout partner)
    /// </summary>
    public class ActiveMatchDto
    {
        public int MatchId { get; set; }
        public DateTime MatchedAt { get; set; }
        public DateTime LastInteraction { get; set; }
        public WorkoutPartnerDto Partner { get; set; } = null!;
        public MatchInteractionDto Interaction { get; set; } = null!;
        public List<string> SharedActivities { get; set; } = new List<string>();
        public double CompatibilityScore { get; set; }

        // Computed properties
        public string MatchDuration { get; set; } = null!; // "2 months", "3 weeks", etc.
        public bool IsRecentMatch { get; set; } // Matched within last week
        public int WorkoutCount { get; set; } // Number of workouts logged together (future feature)
    }

    /// <summary>
    /// DTO for workout partner information
    /// </summary>
    public class WorkoutPartnerDto
    {
        public int UserId { get; set; }
        public int ProfileId { get; set; }
        public string DisplayName { get; set; } = null!;
        public int Age { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public double Distance { get; set; }
        public FitnessLevel FitnessLevel { get; set; }
        public List<string> Activities { get; set; } = new List<string>();
        public List<string> Goals { get; set; } = new List<string>();
        public bool HasHomeGym { get; set; }
        public ContactInfoDto? ContactInfo { get; set; } // Only for active matches
        public AvailabilityDto Availability { get; set; } = new AvailabilityDto();
    }

    /// <summary>
    /// Contact information (only visible for active matches)
    /// </summary>
    public class ContactInfoDto
    {
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string PreferredContactMethod { get; set; } = "In-app"; // In-app, Phone, Email
    }

    /// <summary>
    /// Availability information
    /// </summary>
    public class AvailabilityDto
    {
        public List<DayAvailabilityDto> WeeklySchedule { get; set; } = new List<DayAvailabilityDto>();
        public string NextAvailable { get; set; } = null!; // "Tomorrow at 6 AM", "Monday Morning"
        public List<string> PreferredTimes { get; set; } = new List<string>();
    }

    /// <summary>
    /// Daily availability
    /// </summary>
    public class DayAvailabilityDto
    {
        public string Day { get; set; } = null!;
        public List<string> TimeSlots { get; set; } = new List<string>();
        public bool IsToday { get; set; }
        public bool IsTomorrow { get; set; }
    }

    /// <summary>
    /// Match interaction details
    /// </summary>
    public class MatchInteractionDto
    {
        public string InitialMessage { get; set; } = null!;
        public DateTime? LastMessageAt { get; set; }
        public int MessageCount { get; set; }
        public DateTime? LastWorkoutTogether { get; set; }
        public string? LastWorkoutActivity { get; set; }
    }

    /// <summary>
    /// Statistics about user's matches
    /// </summary>
    public class MatchesStatisticsDto
    {
        public int TotalActiveMatches { get; set; }
        public int NewMatchesThisWeek { get; set; }
        public int NewMatchesThisMonth { get; set; }
        public double AverageCompatibilityScore { get; set; }
        public string MostCommonActivity { get; set; } = null!;
        public Dictionary<string, int> MatchesByActivity { get; set; } = new Dictionary<string, int>();
    }
}