namespace SmartFitnessApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public Profile? Profile { get; set; }
        public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();

    }
}