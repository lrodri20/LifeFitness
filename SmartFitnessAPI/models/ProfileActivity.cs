namespace SmartFitnessApi.Models
{
    public class ProfileActivity
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;
        public int ActivityId { get; set; }
        public Activity Activity { get; set; } = null!;
        public bool IsPrimary { get; set; }

    }
}