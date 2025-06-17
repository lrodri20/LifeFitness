using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartFitnessApi.Models;

namespace SmartFitnessApi.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private const int SaltSize = 16; // 128-bit
        private const int KeySize = 32;  // 256-bit
        private const int Iterations = 10000;
        private readonly JwtSettings _settings;
        private readonly byte[] _secretKeyBytes;

        public AuthenticationService(IOptions<JwtSettings> options)
        {
            // capture the settings once
            _settings = options.Value;
            _secretKeyBytes = Encoding.UTF8.GetBytes(_settings.SecretKey);
        }

        public int TokenExpiryInSeconds
            => _settings.ExpiryInSeconds;
        /// <summary>
        /// Hashes a password using PBKDF2 with SHA-256.
        /// </summary>  

        public string HashPassword(string password)
        {
            // Generate a salt
            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            // Derive a subkey (hash) from the password and salt
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] key = deriveBytes.GetBytes(KeySize);

            // Format: {iterations}.{salt}.{hash}
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            var parts = hashedPassword.Split('.');
            if (parts.Length != 3)
                return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] key = Convert.FromBase64String(parts[2]);

            // Derive the key from the provided password
            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] keyToCheck = deriveBytes.GetBytes(KeySize);

            // Compare in constant time
            return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
        }
        public string GenerateJwtToken(User user)
        {
            // 1) create signing creds
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(_secretKeyBytes),
                SecurityAlgorithms.HmacSha256);

            // 2) define standard claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
                new Claim("id",                               user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,      user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString())
            };

            // 3) create the token
            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(_settings.ExpiryInSeconds),
                signingCredentials: creds
            );

            // 4) serialize to string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}