// DTOs
using System.ComponentModel.DataAnnotations;
using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    /// <summary>
    /// DTO for returning matching preferences
    /// </summary>
    public class MatchingPreferenceDto
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }

        // Distance Preferences
        public int MaxDistanceMiles { get; set; }

        // Age Preferences
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        // Gender Preferences
        public string GenderPreference { get; set; } = "Any";

        // Fitness Level Preferences
        public bool PreferSimilarFitnessLevel { get; set; }
        public int FitnessLevelTolerance { get; set; }

        // Environment Preferences
        public bool PreferHomeGym { get; set; }
        public bool PreferPublicGym { get; set; }
        public bool PreferOutdoor { get; set; }

        // Group Preferences
        public bool OpenToGroupWorkouts { get; set; }
        public int MaxGroupSize { get; set; }

        // Metadata
        public DateTime? LastUpdated { get; set; }
    }

    /// <summary>
    /// DTO for creating or updating matching preferences
    /// </summary>
    public class UpdateMatchingPreferenceDto : IValidatableObject
    {
        [Range(1, 100, ErrorMessage = "Maximum distance must be between 1 and 100 miles")]
        public int MaxDistanceMiles { get; set; } = 5;

        [Range(18, 100, ErrorMessage = "Minimum age must be at least 18")]
        public int? MinAge { get; set; }

        [Range(18, 100, ErrorMessage = "Maximum age must be between 18 and 100")]
        public int? MaxAge { get; set; }

        [RegularExpression("^(Any|Same|Different)$", ErrorMessage = "Gender preference must be 'Any', 'Same', or 'Different'")]
        public string GenderPreference { get; set; } = "Any";

        public bool PreferSimilarFitnessLevel { get; set; } = true;

        [Range(0, 3, ErrorMessage = "Fitness level tolerance must be between 0 and 3")]
        public int FitnessLevelTolerance { get; set; } = 1;

        public bool PreferHomeGym { get; set; }
        public bool PreferPublicGym { get; set; } = true;
        public bool PreferOutdoor { get; set; } = true;

        public bool OpenToGroupWorkouts { get; set; } = true;

        [Range(2, 20, ErrorMessage = "Maximum group size must be between 2 and 20")]
        public int MaxGroupSize { get; set; } = 4;

        // Custom validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinAge.HasValue && MaxAge.HasValue && MinAge > MaxAge)
            {
                yield return new ValidationResult(
                    "Minimum age cannot be greater than maximum age",
                    new[] { nameof(MinAge), nameof(MaxAge) }
                );
            }

            if (!PreferHomeGym && !PreferPublicGym && !PreferOutdoor)
            {
                yield return new ValidationResult(
                    "At least one environment preference must be selected",
                    new[] { nameof(PreferHomeGym), nameof(PreferPublicGym), nameof(PreferOutdoor) }
                );
            }
        }
    }

    /// <summary>
    /// DTO for displaying matching preferences in a user-friendly format
    /// </summary>
    public class MatchingPreferenceSummaryDto
    {
        public string DistanceRange { get; set; } = null!; // "Within 5 miles"
        public string AgeRange { get; set; } = null!; // "25-35 years old" or "Any age"
        public string GenderPreference { get; set; } = null!; // "Any gender", "Same gender", "Different gender"
        public string FitnessLevelPreference { get; set; } = null!; // "Similar fitness level (±1)" or "Any fitness level"
        public List<string> EnvironmentPreferences { get; set; } = new List<string>(); // ["Home gym", "Public gym", "Outdoor"]
        public string GroupPreference { get; set; } = null!; // "Open to groups up to 6 people" or "One-on-one only"
        public DateTime? LastUpdated { get; set; }
    }

    /// <summary>
    /// Simple DTO for quick preference updates
    /// </summary>
    public class QuickPreferenceUpdateDto
    {
        public int? MaxDistanceMiles { get; set; }
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public bool? OpenToGroupWorkouts { get; set; }
    }
    public record ActiveMatchesResponseDto(
        List<ActiveMatchDto> Matches,
        int TotalCount,
        int Offset,
        int Limit,
        MatchesStatisticsDto Statistics
    );
    public record ActiveMatchDto
    {
        public int MatchId { get; init; }
        public DateTime MatchedAt { get; init; }
        public DateTime LastInteraction { get; init; }
        public List<string> SharedActivities { get; init; } = new();
        public double CompatibilityScore { get; init; }
        public string MatchDuration { get; init; } = "";
        public bool IsRecentMatch { get; init; }
        public int WorkoutCount { get; init; }
        public WorkoutPartnerDto Partner { get; init; } = null!;
        public MatchInteractionDto Interaction { get; init; } = null!;
    }
    public record WorkoutPartnerDto
    {
        public int UserId { get; init; }
        public int ProfileId { get; init; }
        public string DisplayName { get; init; } = "";
        public int Age { get; init; }
        public string? ProfilePictureUrl { get; init; }
        public string? Bio { get; init; }
        public string City { get; init; } = "";
        public string State { get; init; } = "";
        public double Distance { get; init; }
        public FitnessLevel FitnessLevel { get; init; }      // or int
        public List<string> Activities { get; init; } = new();
        public List<string> Goals { get; init; } = new();
        public bool HasHomeGym { get; init; }
        public ContactInfoDto ContactInfo { get; init; } = null!;
        public AvailabilityDto Availability { get; init; } = null!;
    }
    public record ContactInfoDto
    {
        public string? PhoneNumber { get; init; }
        public string Email { get; init; } = "";
        public string PreferredContactMethod { get; init; } = "";
    }
    public class AvailabilityDto
    {
        public List<DayAvailabilityDto> WeeklySchedule { get; set; } = new();
        public string NextAvailable { get; set; } = "";
        public List<string> PreferredTimes { get; set; } = new();
    }
    public record MatchInteractionDto
    {
        public string InitialMessage { get; init; } = "";
        public DateTime? LastMessageAt { get; init; }
        public int MessageCount { get; init; }
        public DateTime? LastWorkoutTogether { get; init; }
        public string? LastWorkoutActivity { get; init; }
    }
    public class MatchesStatisticsDto
    {
        public int TotalActiveMatches { get; set; }
        public int NewMatchesThisWeek { get; set; }
        public int NewMatchesThisMonth { get; set; }
        public double AverageCompatibilityScore { get; set; }
        public Dictionary<string, int> MatchesByActivity { get; set; } = new();
        public string MostCommonActivity { get; set; } = "";
    }
    /// <summary>
    /// Represents availability for a single day of the week.
    /// </summary>
    public record DayAvailabilityDto
    {
        /// <summary>
        /// The name of the day, e.g. "Monday".
        /// </summary>
        public string Day { get; init; } = "";

        /// <summary>
        /// The list of available time‐slot labels for that day.
        /// </summary>
        public List<string> TimeSlots { get; init; } = new();

        /// <summary>
        /// True if this day is today (based on server local date).
        /// </summary>
        public bool IsToday { get; init; }

        /// <summary>
        /// True if this day is tomorrow (based on server local date).
        /// </summary>
        public bool IsTomorrow { get; init; }
    }
}