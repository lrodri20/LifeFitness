using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class ProfileSchedule
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSlot TimeSlot { get; set; }
        public bool IsAvailable { get; set; } = true;

    }
}