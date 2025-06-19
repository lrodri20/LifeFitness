namespace SmartFitnessApi.Models
{
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
}