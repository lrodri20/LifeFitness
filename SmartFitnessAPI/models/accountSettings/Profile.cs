using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class Profile
    {
        public int Id { get; set; }

        // FK to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Basic Information
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }

        // Address Information
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        // Location for matching (stored separately for proximity searches)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Profile Details
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }

        // Fitness Information
        public FitnessLevel FitnessLevel { get; set; } = FitnessLevel.Beginner;
        public bool HasHomeGym { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<ProfileActivity> Activities { get; set; } = new List<ProfileActivity>();
        public ICollection<ProfileGoal> Goals { get; set; } = new List<ProfileGoal>();
        public ICollection<ProfileSchedule> Schedules { get; set; } = new List<ProfileSchedule>();
        public MatchingPreference? MatchingPreference { get; set; }
    }
}