using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFitnessApi.Models
{
    [Table("RevokedTokens", Schema = "auth")]
    public class RevokedToken
    {
        public int Id { get; set; }
        public string JwtId { get; set; } = null!;
        public DateTime RevokedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}