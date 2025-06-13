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
