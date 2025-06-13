using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;
using System.Threading.Tasks;

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
    }
}
