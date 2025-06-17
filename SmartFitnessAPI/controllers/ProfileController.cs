using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
namespace SmartFitnessApi.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IAccountService _acct;

        public ProfileController(IAccountService acct) => _acct = acct;

        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("id");
                if (userIdClaim == null)
                    return Unauthorized();

                var userId = int.Parse(userIdClaim.Value);
                var profile = await _acct.GetProfileAsync(userId);
                return Ok(profile);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut]
        public async Task<ActionResult<ProfileDto>> UpdateProfile(ProfileDto input)
        {
            try
            {
                var userIdClaim = User.FindFirst("id");
                if (userIdClaim == null)
                    return Unauthorized();

                var userId = int.Parse(userIdClaim.Value);
                var updated = await _acct.UpdateProfileAsync(userId, input);
                return Ok(updated);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}
