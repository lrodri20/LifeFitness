using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
namespace SmartFitnessApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IAuthenticationService _authenticationService;

        public AccountController(IAccountService accountService, IAuthenticationService authenticationService)
        {
            _accountService = accountService;
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdUser = await _accountService.SignUpAsync(request);
                return CreatedAtAction(nameof(SignUp), new { id = createdUser.Id }, createdUser);
            }
            catch (UserAlreadyExistsException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return Ok();
        }

        /// <summary>
        /// Signs in an existing user.
        /// </summary>
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate credentials
            var user = await _accountService.GetByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            var isValid = _authenticationService.VerifyPassword(user.PasswordHash, request.Password);
            if (!isValid)
                return Unauthorized(new { message = "Invalid email or password." });

            // Generate JWT
            var token = _authenticationService.GenerateJwtToken(user);

            var response = new TokenResponse
            {
                Token = token,
                ExpiresIn = _authenticationService.TokenExpiryInSeconds
            };

            return Ok(response);
        }
        /// <summary>
        /// Test endpoint to validate the JWT bearer token and show remaining time until expiration.
        /// </summary>
        [HttpGet("session-test")]
        [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult TestSession()
        {
            var expClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            if (expClaim == null)
                return Unauthorized(new { message = "Expiration claim not found." });

            if (!long.TryParse(expClaim, out var expSeconds))
                return Unauthorized(new { message = "Invalid expiration claim." });

            var expDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
            var remaining = expDate - DateTime.UtcNow;

            return Ok(new
            {
                user = User.Identity?.Name,
                expiresAt = expDate,
                remainingSeconds = (int)remaining.TotalSeconds
            });
        }
        /// <summary>
        /// Request a password reset token (you can email this to the user).
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
        {
            try
            {
                var token = await _accountService.GeneratePasswordResetTokenAsync(req.Email);
                // TODO: send `token` to the user via email.
                return Ok(new { Token = token });
            }
            catch (KeyNotFoundException)
            {
                // do not reveal that the email does not exist
                return Ok();
            }
        }

        /// <summary>
        /// Reset a password using the token the user received.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
        {
            try
            {
                await _accountService.ResetPasswordAsync(req);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
