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
    [Route("api/[controller]")]
    public class MatchesController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchesController(IMatchService matchService)
        {
            _matchService = matchService;
        }
        /// <summary>
        /// Get all active matches (accepted workout partners)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ActiveMatchesResponseDto>> GetActiveMatches(
       [FromQuery] string sortBy = "recent",  // changed default to "recent"
       [FromQuery] int limit = 50,
       [FromQuery] int offset = 0)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var matches = await _matchService.GetActiveMatchesAsync(userId, sortBy, limit, offset);
                return Ok(matches);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
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