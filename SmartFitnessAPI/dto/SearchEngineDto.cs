using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    /// <summary>
    /// Search parameters
    /// </summary>
    public class SearchParameters
    {
        public int? Radius { get; set; }
        public int Limit { get; set; } = 20;
        public string? ActivityFilter { get; set; }
        public FitnessLevel? FitnessLevelFilter { get; set; }
    }

    /// <summary>
    /// Search results container
    /// </summary>
    public class SearchResultsDto
    {
        public List<PotentialMatchDto> Results { get; set; } = new List<PotentialMatchDto>();
        public int TotalFound { get; set; }
        public int ReturnedCount { get; set; }
        public SearchMetadataDto Metadata { get; set; } = new SearchMetadataDto();
    }

    /// <summary>
    /// Individual potential match result
    /// </summary>
    public class PotentialMatchDto
    {
        public int UserId { get; set; }
        public int ProfileId { get; set; }
        public string DisplayName { get; set; } = null!;
        public int Age { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public double Distance { get; set; }
        public double CompatibilityScore { get; set; }
        public FitnessLevel FitnessLevel { get; set; }
        public List<string> Activities { get; set; } = new List<string>();
        public List<string> Goals { get; set; } = new List<string>();
        public List<string> CommonActivities { get; set; } = new List<string>();
        public bool HasHomeGym { get; set; }

        // Match status if any previous interaction
        public int? PreviousMatchId { get; set; }
        public string? PreviousMatchStatus { get; set; }

        // Availability preview
        public List<string> TypicalAvailability { get; set; } = new List<string>();

        // Location info (city only for privacy)
        public string City { get; set; } = null!;
        public string State { get; set; } = null!;

        // Compatibility breakdown
        public CompatibilityBreakdownDto CompatibilityBreakdown { get; set; } = new CompatibilityBreakdownDto();
    }

    /// <summary>
    /// Compatibility score breakdown
    /// </summary>
    public class CompatibilityBreakdownDto
    {
        public double DistanceScore { get; set; }
        public double ActivityScore { get; set; }
        public double ScheduleScore { get; set; }
        public double FitnessLevelScore { get; set; }
        public double GoalScore { get; set; }
        public double TotalScore { get; set; }
    }

    /// <summary>
    /// Search metadata
    /// </summary>
    public class SearchMetadataDto
    {
        public int SearchRadiusMiles { get; set; }
        public DateTime SearchedAt { get; set; }
        public Dictionary<string, int> ResultsByDistance { get; set; } = new Dictionary<string, int>();
        public List<string> AppliedFilters { get; set; } = new List<string>();
        public string? SuggestedAction { get; set; }
    }

    /// <summary>
    /// Search preview showing potential matches at different distances
    /// </summary>
    public class SearchPreviewDto
    {
        public Dictionary<int, int> MatchesByRadius { get; set; } = new Dictionary<int, int>();
        public int CurrentRadius { get; set; }
        public string Recommendation { get; set; } = null!;
    }
}
