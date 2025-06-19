using System.ComponentModel.DataAnnotations;
using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string? Description { get; set; }

        public string? IconUrl { get; set; }
        public ActivityCategory Category { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<ProfileActivity> ProfileActivities { get; set; } = new List<ProfileActivity>();

    }
}