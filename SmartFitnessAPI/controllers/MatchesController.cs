using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartFitnessApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchingService _matchingService;

        public MatchesController(IMatchingService matchingService)
        {
            _matchingService = matchingService;
        }

        /// <summary>
        /// Get potential matches for the current user
        /// </summary>
        /// <param name="radius">Maximum distance in miles (default: 5)</param>
        /// <param name="limit">Maximum number of results (default: 20)</param>
        /// <returns>List of potential matches sorted by compatibility score</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProfileMatchingDto>>> GetPotentialMatches(
            [FromQuery] int radius = 5,
            [FromQuery] int limit = 20)
        {
            // Get the current user's ID from the JWT token
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == 0)
            {
                return Unauthorized("User not found in token");
            }

            // Validate parameters
            if (radius < 1 || radius > 100)
            {
                return BadRequest("Radius must be between 1 and 100 miles");
            }

            if (limit < 1 || limit > 50)
            {
                return BadRequest("Limit must be between 1 and 50");
            }

            try
            {
                var matches = await _matchingService.GetPotentialMatchesAsync(userId, radius, limit);
                return Ok(matches);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while fetching matches");
            }
        }
    }
}