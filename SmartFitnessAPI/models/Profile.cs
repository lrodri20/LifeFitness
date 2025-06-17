namespace SmartFitnessApi.Models
{
    public class Profile
    {
        public int Id { get; set; }

        // FK to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}