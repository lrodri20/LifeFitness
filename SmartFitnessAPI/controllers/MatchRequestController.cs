using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;

namespace SmartFitnessApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/match-requests")]
    public class MatchRequestController : ControllerBase
    {
        private readonly IMatchRequestService _matchRequestService;

        public MatchRequestController(IMatchRequestService matchRequestService)
        {
            _matchRequestService = matchRequestService;
        }

        // /// <summary>
        // /// Get all match requests for the authenticated user
        // /// </summary>
        [HttpGet]
        [Route("requests")]
        public async Task<ActionResult<List<MatchRequestDto>>> GetMatchRequests(
      [FromQuery] string direction = "all",
      [FromQuery] string status = null)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var requests = await _matchRequestService.GetMatchRequestsAsync(userId, direction, status);
                return Ok(requests);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Send a match request to another user
        /// </summary>
        [HttpPost("{targetUserId}")]
        public async Task<ActionResult> SendMatchRequest(int targetUserId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _matchRequestService.CreateMatchRequestAsync(userId, targetUserId);
            return NoContent();
        }

        // /// <summary>
        // /// Accept a match request from another user
        // /// </summary>
        [HttpPost("accept/{requestId}")]
        public async Task<ActionResult> AcceptMatchRequest(int requestId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _matchRequestService.AcceptMatchRequestAsync(requestId, userId);
            return NoContent();
        }

        // /// <summary>
        // /// Decline a match request from another user
        // /// </summary>
        [HttpPost("decline/{requestId}")]
        public async Task<ActionResult> DeclineMatchRequest(int requestId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            await _matchRequestService.RejectMatchRequestAsync(requestId, userId);
            return NoContent();
        }
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PendingMatchRequestDto>>> GetPendingRequests()
        {
            // pull the current user’s ID (requestee) out of their JWT
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(raw, out var requesteeId) || requesteeId == 0)
                return Unauthorized();

            // fetch pending domain entities
            var pending = await _matchRequestService.GetPendingRequestsAsync(requesteeId);

            // map to DTOs
            var dtoList = pending.Select(r => new PendingMatchRequestDto
            {
                Id = r.Id,
                RequesterId = r.RequesterId,
                RequesterUsername = r.Requester.FirstName + " " + r.Requester.LastName,
                RequesteeId = r.RequesteeId,
                RequesteeUsername = r.Requestee.FirstName + " " + r.Requestee.LastName,
                CreatedAt = r.CreatedAt
            });

            return Ok(dtoList);
        }
    }
}
