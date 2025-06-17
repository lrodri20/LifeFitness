namespace SmartFitnessApi.Models
{
    /// <summary>
    /// Request payload to revoke a refresh token (i.e. sign out).
    /// </summary>
    public record SignOutRequest(string RefreshToken);
}
