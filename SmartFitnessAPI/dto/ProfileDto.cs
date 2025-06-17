namespace SmartFitnessApi.Models
{
    public record ProfileDto(
         string? FirstName,
         string? LastName,
         string? DisplayName,
         DateTime? DateOfBirth,
         string? PhoneNumber,
         string? AddressLine1,
         string? City,
         string? State,
         string? PostalCode,
         string? Country,
         string? ProfilePictureUrl,
         string? Bio
     );
}