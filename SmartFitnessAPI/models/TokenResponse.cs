namespace SmartFitnessApi.Models
{
    /// <summary>
    /// Response model returned when a user successfully signs in.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// The JWT access token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Expiration duration (in seconds) of the token.
        /// </summary>
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; init; } = null!;
    }
}