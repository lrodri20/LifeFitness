using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Data.Dtos;
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
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _matchService;
        private readonly IPreferencesService _prefsService;

        public MatchesController(IMatchService matchService, IPreferencesService prefsService)
        {
            _matchService = matchService;
            _prefsService = prefsService;
        }
        /// <summary>
        /// Get all active matches (accepted workout partners)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches(
             [FromQuery] string sortBy = "compatibility")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var prefs = await _prefsService.GetMatchingPreferencesAsync(userId);
            if (prefs == null)
                return NotFound("Preferences not set");

            var matches = await _matchService.GetMatchesAsync(userId, sortBy);
            return Ok(matches);
        }

        /// <summary>
        /// Remove an active match (unfriend)
        /// </summary>
        [HttpDelete("{matchId}")]
        public async Task<ActionResult> RemoveMatch([FromRoute] int matchId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                await _matchService.RemoveMatchAsync(matchId, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Block a user (prevents future match requests)
        /// </summary>
        [HttpPost("{matchId}/block")]
        public async Task<ActionResult> BlockUser([FromRoute] int matchId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                await _matchService.BlockUserAsync(matchId, userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}