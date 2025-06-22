using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Models
{
    public class PendingMatchRequestDto
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public string RequesterUsername { get; set; }
        public int RequesteeId { get; set; }
        public string RequesteeUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}