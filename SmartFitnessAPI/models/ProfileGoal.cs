using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class ProfileGoal
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;
        public FitnessGoal Goal { get; set; }
        public int Priority { get; set; } = 1;

    }
}