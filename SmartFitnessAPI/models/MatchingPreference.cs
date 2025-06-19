using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class MatchingPreference
    {
        public int Id { get; set; }

        // FK to Profile
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;

        // Distance Preferences
        public int MaxDistanceMiles { get; set; } = 5;

        // Age Preferences
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        // Gender Preferences
        public GenderPreference GenderPreference { get; set; } = GenderPreference.Any;

        // Fitness Level Preferences
        public bool PreferSimilarFitnessLevel { get; set; } = true;
        public int FitnessLevelTolerance { get; set; } = 1; // How many levels apart is acceptable

        // Environment Preferences
        public bool PreferHomeGym { get; set; }
        public bool PreferPublicGym { get; set; } = true;
        public bool PreferOutdoor { get; set; } = true;

        // Group Preferences
        public bool OpenToGroupWorkouts { get; set; } = true;
        public int MaxGroupSize { get; set; } = 4;

    }
}