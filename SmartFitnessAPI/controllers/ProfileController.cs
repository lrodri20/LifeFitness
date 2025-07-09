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
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IAccountService _acct;
        private readonly IProfileService _profileService;

        public ProfileController(IAccountService acct, IProfileService profileService)
        {
            _profileService = profileService;
            _acct = acct;
        }
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
        // <summary>
        /// GET /api/profiles/{id}
        /// Returns the public profile information for a given user.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProfileDto>> GetById(int id)
        {
            var dto = await _profileService.GetProfileAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }
    }
}
