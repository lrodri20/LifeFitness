// Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;
using System.Security.Claims;

namespace SmartFitnessApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        /// <summary>
        /// GET /api/users/queue?sortBy=Recent
        /// </summary>
        [HttpGet("queue")]
        public async Task<ActionResult<IEnumerable<UserQueueDto>>> GetQueue(
            [FromQuery] string sortBy = "Recent"
        )
        {
            // extract numeric user ID from the JWT/sub claim
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized();

            var queue = await _usersService.GetUserQueueAsync(currentUserId, sortBy);
            return Ok(queue);
        }
    }
}
