using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartFitnessApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/requests")]
    public class MatchRequestsController : ControllerBase
    {
        private readonly IMatchRequestService _requestService;

        public MatchRequestsController(IMatchRequestService requestService)
        {
            _requestService = requestService;
        }

        /// <summary>
        /// Send a match request to another user
        /// </summary>
        [HttpPost("send/{userId}")]
        public async Task<ActionResult<MatchRequestResponseDto>> SendMatchRequest(
            [FromRoute] int userId,
            [FromBody] SendMatchRequestDto request)
        {
            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (senderId == userId)
            {
                return BadRequest("You cannot send a match request to yourself");
            }

            try
            {
                var result = await _requestService.SendMatchRequestAsync(senderId, userId, request);
                return CreatedAtAction(nameof(GetRequest), new { requestId = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all match requests received by the current user
        /// </summary>
        [HttpGet("received")]
        public async Task<ActionResult<ReceivedRequestsResponseDto>> GetReceivedRequests(
            [FromQuery] string status = "pending",
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var requests = await _requestService.GetReceivedRequestsAsync(userId, status, limit, offset);
                return Ok(requests);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all match requests sent by the current user
        /// </summary>
        [HttpGet("sent")]
        public async Task<ActionResult<SentRequestsResponseDto>> GetSentRequests(
            [FromQuery] string status = "all",
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var requests = await _requestService.GetSentRequestsAsync(userId, status, limit, offset);
                return Ok(requests);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Accept a match request
        /// </summary>
        [HttpPost("{requestId}/accept")]
        public async Task<ActionResult<MatchRequestResponseDto>> AcceptRequest([FromRoute] int requestId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var result = await _requestService.AcceptRequestAsync(requestId, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Reject a match request
        /// </summary>
        [HttpPost("{requestId}/reject")]
        public async Task<ActionResult<MatchRequestResponseDto>> RejectRequest([FromRoute] int requestId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var result = await _requestService.RejectRequestAsync(requestId, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get a specific request
        /// </summary>
        [HttpGet("{requestId}")]
        public async Task<ActionResult<MatchRequestResponseDto>> GetRequest([FromRoute] int requestId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                var request = await _requestService.GetRequestAsync(requestId, userId);
                return Ok(request);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
