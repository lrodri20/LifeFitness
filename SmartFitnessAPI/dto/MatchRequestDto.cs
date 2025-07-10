using SmartFitnessApi.Models.enums;

namespace SmartFitnessApi.Data.Dtos
{
    public class MatchRequestDto
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public int RequesteeId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public string RequesterName { get; set; }
        public string RequesteeName { get; set; }
    }
}