// ============================================
// CORRECTED API STRUCTURE
// ============================================

/*
DATABASE STRUCTURE:
- Users table: Basic user authentication
- Profiles table: User fitness profiles
- Matches table: Should be renamed to "MatchRequests" - stores all connection requests
  - Status: Pending, Accepted, Rejected, Expired, Blocked
  - When Status = Accepted, these become active "matches" (workout partners)
*/

// ============================================
// 1. SEARCH FOR POTENTIAL MATCHES
// ============================================

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;

namespace SmartFitnessApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Search for potential workout partners based on preferences
        /// </summary>
        /// <param name="radius">Search radius in miles (overrides user preference)</param>
        /// <param name="limit">Number of results to return</param>
        [HttpGet("search/users")]
        public async Task<ActionResult<IEnumerable<ProfileMatchingDto>>> SearchPotentialMatches(
            [FromQuery] int? radius = null,
            [FromQuery] int limit = 20)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Validate parameters
            if (radius.HasValue && (radius < 1 || radius > 100))
            {
                return BadRequest("Radius must be between 1 and 100 miles");
            }

            if (limit < 1 || limit > 50)
            {
                return BadRequest("Limit must be between 1 and 50");
            }

            try
            {
                var potentialMatches = await _searchService.SearchPotentialMatchesAsync(userId, radius, limit);
                return Ok(potentialMatches);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}