using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Data;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string message) : base(message) { }
}

public class AccountService : IAccountService
{
    private readonly SmartFitnessDbContext _dbContext;
    private readonly IAuthenticationService _authService;

    public AccountService(SmartFitnessDbContext dbContext, IAuthenticationService authService)
    {
        _dbContext = dbContext;
        _authService = authService;
    }

    public async Task<UserDto> SignUpAsync(SignUpRequest request)
    {
        if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            throw new UserAlreadyExistsException("A user with that email already exists.");

        // Hash the password
        var passwordHash = _authService.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName,
            PasswordHash = passwordHash
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName
        };
    }
    public async Task<User?> GetByEmailAsync(string email)
    {
        // Returns the User entity (including PasswordHash) or null
        return await _dbContext.Users
                               .SingleOrDefaultAsync(u => u.Email == email);
    }
    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _dbContext.Users
                            .SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
            throw new KeyNotFoundException("Email not found");

        // generate a cryptographically-random token
        var token = Guid.NewGuid().ToString("N");

        var pr = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Used = false
        };
        _dbContext.PasswordResetTokens.Add(pr);
        await _dbContext.SaveChangesAsync();

        return token;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest req)
    {
        var pr = await _dbContext.PasswordResetTokens
                          .Include(x => x.User)
                          .SingleOrDefaultAsync(x =>
                              x.User.Email == req.Email &&
                              x.Token == req.Token &&
                              !x.Used &&
                              x.ExpiresAt > DateTime.UtcNow);

        if (pr is null)
            throw new InvalidOperationException("Invalid or expired reset token");

        // hash and set the new password
        pr.User.PasswordHash = _authService.HashPassword(req.NewPassword);
        pr.Used = true;

        await _dbContext.SaveChangesAsync();
    }
    public async Task<ProfileDto> GetProfileAsync(int userId)
    {
        var profile = await _dbContext.Profiles
                               .AsNoTracking()
                               .SingleOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            throw new NotFoundException($"Profile for user {userId} not found.");

        // map entity → DTO
        return new ProfileDto(
            profile.FirstName,
            profile.LastName,
            profile.DisplayName,
            profile.DateOfBirth,
            profile.PhoneNumber,
            profile.AddressLine1,
            profile.City,
            profile.State,
            profile.PostalCode,
            profile.Country,
            profile.ProfilePictureUrl,
            profile.Bio
        );
    }

    public async Task<ProfileDto> UpdateProfileAsync(int userId, ProfileDto input)
    {
        // ensure the user actually exists
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            throw new NotFoundException($"User {userId} not found.");

        // load or create the profile
        var profile = await _dbContext.Profiles
                               .SingleOrDefaultAsync(p => p.UserId == userId);
        if (profile == null)
        {
            profile = new Profile { UserId = userId, CreatedAt = DateTime.UtcNow };
            _dbContext.Profiles.Add(profile);
        }

        // apply updates
        profile.FirstName = input.FirstName;
        profile.LastName = input.LastName;
        profile.DisplayName = input.DisplayName;
        profile.DateOfBirth = input.DateOfBirth;
        profile.PhoneNumber = input.PhoneNumber;
        profile.AddressLine1 = input.AddressLine1;
        profile.City = input.City;
        profile.State = input.State;
        profile.PostalCode = input.PostalCode;
        profile.Country = input.Country;
        profile.ProfilePictureUrl = input.ProfilePictureUrl;
        profile.Bio = input.Bio;
        profile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // return the updated DTO
        return new ProfileDto(
            profile.FirstName,
            profile.LastName,
            profile.DisplayName,
            profile.DateOfBirth,
            profile.PhoneNumber,
            profile.AddressLine1,
            profile.City,
            profile.State,
            profile.PostalCode,
            profile.Country,
            profile.ProfilePictureUrl,
            profile.Bio
        );
    }
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _dbContext.UserRefreshTokens
                             .SingleOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || token.RevokedAt != null)
        {
            // Either unknown or already revoked—nothing to do
            return;
        }

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = "User signed out";

        await _dbContext.SaveChangesAsync();
    }
    public class UserAlreadyExistsException : Exception
    {
        public string ParamName { get; }

        public UserAlreadyExistsException()
        {
        }

        public UserAlreadyExistsException(string message)
            : base(message)
        {
        }

        public UserAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UserAlreadyExistsException(string message, string paramName, Exception innerException)
            : base(message, innerException)
        {
            ParamName = paramName;
        }
    }
}
