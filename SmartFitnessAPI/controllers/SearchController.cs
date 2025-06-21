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
using SmartFitnessApi.Models.enums;
using SmartFitnessApi.Services;

namespace SmartFitnessApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        /// <summary>
        /// Search for potential workout partners based on user preferences
        /// </summary>
        /// <param name="radius">Override search radius in miles (optional, uses user preference if not provided)</param>
        /// <param name="limit">Number of results to return (default: 20, max: 50)</param>
        /// <param name="activityFilter">Filter by specific activity (optional)</param>
        /// <param name="fitnessLevelFilter">Filter by fitness level (optional)</param>
        /// <returns>List of potential matches</returns>
        [HttpGet("users")]
        public async Task<ActionResult<SearchResultsDto>> SearchPotentialMatches(
            [FromQuery] int? radius = null,
            [FromQuery] int limit = 20,
            [FromQuery] string? activityFilter = null,
            [FromQuery] FitnessLevel? fitnessLevelFilter = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == 0)
            {
                return Unauthorized("User not found in token");
            }

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
                var searchParams = new SearchParameters
                {
                    Radius = radius,
                    Limit = limit,
                    ActivityFilter = activityFilter,
                    FitnessLevelFilter = fitnessLevelFilter
                };

                var results = await _searchService.SearchPotentialMatchesAsync(userId, searchParams);
                return Ok(results);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while searching for matches");
            }
        }

        /// <summary>
        /// Get a preview of potential matches count with different radius options
        /// </summary>
        /// <returns>Count of potential matches at different distances</returns>
        [HttpGet("preview")]
        public async Task<ActionResult<SearchPreviewDto>> GetSearchPreview()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (userId == 0)
            {
                return Unauthorized("User not found in token");
            }

            try
            {
                var preview = await _searchService.GetSearchPreviewAsync(userId);
                return Ok(preview);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}