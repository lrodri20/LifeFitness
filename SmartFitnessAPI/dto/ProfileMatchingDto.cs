using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class ProfileMatchingDto
    {
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
    }
}