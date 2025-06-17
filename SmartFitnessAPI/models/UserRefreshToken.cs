using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFitnessApi.Models
{
    [Table("UserRefreshTokens", Schema = "auth")]
    public class UserRefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Token { get; set; } = null!;

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }

        // navigation back to User
        public User User { get; set; } = null!;
    }
}
